using System;
using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    [Serializable]
    public sealed class MassNode
    {
        public Vector3 position, previous, velocity, force, original;
        public float inverseMass, radius;
        public bool grounded;
        // Static-world broad phase, reused during constraint sweeps. Requery
        // whenever corrections leave the conservative neighbourhood.
        internal readonly Collider[] nearby = new Collider[32];
        internal int nearbyCount = -1;
        internal Vector3 nearbyCenter;
        public MassNode(Vector3 p, float mass, float radius)
        { position = previous = original = p; inverseMass = 1f / mass; this.radius = radius; }
    }

    [Serializable]
    public sealed class Beam
    {
        public int a, b;
        public float rest, initial, compliance, yield, failure, plastic, stress, lambda;
        public bool broken, mount;
        public int attachment = -1;
        public float plasticRate = 24;
        public float damping = .08f;
        public Beam(int a, int b, float length, float compliance, float yield, float failure, bool mount)
        { this.a = a; this.b = b; rest = initial = length; this.compliance = compliance; this.yield = yield; this.failure = failure; this.mount = mount; }
    }

    public struct ContactSample
    {
        public Vector3 point, impulse;
        public float age;
    }

    // World-space XPBD. No rigid chassis, kinematic recovery forces or shape-matching reset.
    public sealed class SoftStructure
    {
        public readonly List<MassNode> nodes = new List<MassNode>();
        public readonly List<Beam> beams = new List<Beam>();
        public readonly List<ContactSample> contacts = new List<ContactSample>();
        readonly List<int[]> attachments = new List<int[]>();
        public int Substeps = 10, Iterations = 8;
        public float PeakImpact, Time;
        public double PlasticWork;

        readonly SphereCollider probe;
        readonly int collisionMask = 1 << 0;

        public SoftStructure(SphereCollider probe) { this.probe = probe; }
        public int AddNode(Vector3 p, float mass = 24, float radius = .10f)
        { nodes.Add(new MassNode(p, mass, radius)); return nodes.Count - 1; }
        public int AddBeam(int a, int b, float compliance = 1e-8f, float yield = .075f, float failure = .7f, bool mount = false)
        {
            beams.Add(new Beam(a, b, Vector3.Distance(nodes[a].position, nodes[b].position), compliance, yield, failure, mount));
            return beams.Count - 1;
        }
        public int BrokenCount { get { int n = 0; foreach (var b in beams) if (b.broken) n++; return n; } }
        public void GroupAttachment(int[] members)
        {
            int id=attachments.Count;attachments.Add(members);
            foreach(int i in members) beams[i].attachment=id;
        }
        void Fracture(Beam beam)
        {
            beam.broken=true;
            if(beam.attachment>=0)foreach(int i in attachments[beam.attachment])beams[i].broken=true;
        }
        public int PlasticCount { get { int n = 0; foreach (var b in beams) if (b.plastic > .002f) n++; return n; } }
        public float PlasticTotal { get { float n = 0; foreach (var b in beams) n += b.plastic; return n; } }

        public void Step(float dt, Action<float> forces)
        {
            Begin(dt);
            float h=dt/Substeps;
            for(int sub=0;sub<Substeps;sub++)
            {
                Predict(h,forces);
                for(int it=0;it<Iterations;it++) Solve(h,it);
                Finish(h);
            }
        }
        public void Begin(float dt)
        {
            Time += dt;
            PeakImpact *= Mathf.Exp(-dt * 2);
            for (int c = contacts.Count - 1; c >= 0; c--)
            {
                var s = contacts[c]; s.age += dt;
                if (s.age > 1) contacts.RemoveAt(c); else contacts[c] = s;
            }
        }
        public void Predict(float h,Action<float> forces)
        {
                foreach (var n in nodes) { n.force = Vector3.zero; }
                forces?.Invoke(h);
                foreach (var n in nodes)
                {
                    n.previous = n.position; n.nearbyCount = -1;
                    n.velocity += (Physics.gravity + n.force * n.inverseMass) * h;
                    n.velocity *= Mathf.Exp(-.035f * h);
                    n.position += n.velocity * h;
                    n.grounded = false;
                }
                foreach (var b in beams) b.lambda = 0;
        }
        public void Solve(float h,int it)
        {
                    // Alternate sweep direction to reduce directional solver bias.
                    for (int j = 0; j < beams.Count; j++)
                    {
                        var b = beams[(it & 1) == 0 ? j : beams.Count - 1 - j];
                        if (b.broken) continue;
                        var a = nodes[b.a]; var z = nodes[b.b];
                        Vector3 d = z.position - a.position;
                        float len = d.magnitude;
                        if (len < .00001f) continue;
                        float error = len - b.rest;
                        float alpha = b.compliance / (h * h);
                        float dl = (-error - alpha * b.lambda) / (a.inverseMass + z.inverseMass + alpha);
                        if(!b.mount)
                        {
                            // A yielded member cannot keep applying an unbounded elastic
                            // restoring impulse. Limit its load so impact motion can crush
                            // the structure; the dissipative return below retains that shape.
                            float limit=b.yield*Mathf.Max(.3f,b.initial)/2e-6f*h*h*1.8f;
                            dl=Mathf.Clamp(b.lambda+dl,-limit,limit)-b.lambda;
                        }
                        b.lambda += dl;
                        Vector3 correction = d / len * dl;
                        a.position -= correction * a.inverseMass;
                        z.position += correction * z.inverseMass;
                    }
                    foreach (var n in nodes) ResolvePosition(n, it == 0);
        }
        public void Finish(float h)
        {
                foreach (var b in beams)
                {
                    if (b.broken) continue;
                    // The constraint's elastic force drives yield, including stiff constraints
                    // whose post-solve positional strain alone would hide the impact load.
                    float force = b.lambda / (h * h);
                    float elasticStrain = -force * 2e-6f / Mathf.Max(.3f, b.initial);
                    b.stress = Mathf.Abs(elasticStrain) / b.yield;
                    float magnitude = Mathf.Abs(elasticStrain);
                    float geometricStrain = Mathf.Abs(Vector3.Distance(nodes[b.a].position,nodes[b.b].position)-b.rest)/b.initial;
                    if ((b.mount ? magnitude > b.failure : geometricStrain > b.failure) || b.plastic > b.initial * .5f)
                    { Fracture(b); continue; }
                    if (magnitude > b.yield)
                    {
                        float change = Mathf.Sign(elasticStrain) * Mathf.Min(magnitude - b.yield, .9f) * b.initial * b.plasticRate * h;
                        // Return toward the actual strained length, never past it.
                        // Force-based flow without this bound can inject energy into
                        // stiff bracing and trigger runaway failure of the entire car.
                        float error=Vector3.Distance(nodes[b.a].position,nodes[b.b].position)-b.rest;
                        change=Mathf.Sign(change)==Mathf.Sign(error)?Mathf.Sign(error)*Mathf.Min(Mathf.Abs(change),Mathf.Abs(error)):0;
                        float next = Mathf.Clamp(b.rest + change, b.initial * .32f, b.initial * 1.65f);
                        float amount = Mathf.Abs(next - b.rest);
                        b.rest = next; b.plastic += amount; PlasticWork += amount * Mathf.Abs(force);
                    }
                }
                foreach (var n in nodes)
                {
                    Vector3 incoming = n.velocity;
                    n.velocity = (n.position - n.previous) / h;
                    if (n.grounded)
                    {
                        n.velocity.x *= Mathf.Exp(-1.2f * h);
                        n.velocity.z *= Mathf.Exp(-1.2f * h);
                    }
                    Vector3 impulse = (n.velocity - incoming) / n.inverseMass;
                    if (impulse.magnitude > 45 && n.grounded)
                    {
                        PeakImpact = Mathf.Max(PeakImpact, impulse.magnitude / h);
                        if (contacts.Count < 120) contacts.Add(new ContactSample { point = n.position, impulse = impulse });
                    }
                }
                // Axial damping preserves translation and rotation while dissipating ringing.
                foreach (var b in beams)
                {
                    if (b.broken) continue;
                    var a = nodes[b.a]; var z = nodes[b.b];
                    Vector3 axis = (z.position - a.position).normalized;
                    float speed = Vector3.Dot(z.velocity - a.velocity, axis);
                    float impulse = speed * b.damping / (a.inverseMass + z.inverseMass);
                    a.velocity += axis * impulse * a.inverseMass;
                    z.velocity -= axis * impulse * z.inverseMass;
                }
        }
        void ResolvePosition(MassNode n, bool sweep)
        {
            if (probe == null) return;
            Vector3 travel = n.position - n.previous;
            if (sweep && travel.sqrMagnitude > .000001f && Physics.SphereCast(n.previous, n.radius, travel.normalized,
                out RaycastHit hit, travel.magnitude, collisionMask, QueryTriggerInteraction.Ignore))
            {
                // Preserve tangential motion, otherwise ground contacts act as artificial brakes.
                Vector3 consumed = travel.normalized * Mathf.Max(0,hit.distance-.0001f);
                n.position = n.previous + consumed + Vector3.ProjectOnPlane(travel-consumed,hit.normal);
                n.grounded = true;
            }
            if (n.nearbyCount < 0 || (n.position-n.nearbyCenter).sqrMagnitude > .04f)
            {
                n.nearbyCenter=n.position;
                n.nearbyCount=Physics.OverlapSphereNonAlloc(n.position,n.radius+.25f,n.nearby,collisionMask,QueryTriggerInteraction.Ignore);
            }
            int count=n.nearbyCount;
            for (int i = 0; i < count; i++)
            {
                var c = n.nearby[i];
                if(c.bounds.SqrDistance(n.position)>n.radius*n.radius)continue;
                if(probe.radius!=n.radius)probe.radius=n.radius;
                if (Physics.ComputePenetration(probe, n.position, Quaternion.identity, c, c.transform.position, c.transform.rotation,
                    out Vector3 direction, out float distance))
                { n.position += direction * (distance + .0001f); n.grounded = true; }
            }
        }

        public void Relocate(Vector3 target, Quaternion rotation, Vector3 center, int rootIndex)
        {
            // Only relocate still-connected structure. Detached debris stays in the lab.
            var linked = new HashSet<int>(); var queue = new Queue<int>(); linked.Add(rootIndex); queue.Enqueue(rootIndex);
            while (queue.Count > 0)
            {
                int i = queue.Dequeue();
                foreach (var b in beams)
                {
                    if (b.broken) continue;
                    int other = b.a == i ? b.b : b.b == i ? b.a : -1;
                    if (other >= 0 && linked.Add(other)) queue.Enqueue(other);
                }
            }
            foreach (int i in linked)
            {
                var n = nodes[i]; n.position = target + rotation * (n.position - center);
                n.previous = n.position; n.nearbyCount = -1; n.velocity = Vector3.zero;
            }
        }
        public bool Finite()
        {
            foreach (var n in nodes) if (float.IsNaN(n.position.x) || float.IsInfinity(n.position.x) ||
                float.IsNaN(n.position.y) || float.IsInfinity(n.position.y) || float.IsNaN(n.position.z) ||
                float.IsInfinity(n.position.z) || n.position.sqrMagnitude > 1e9f) return false;
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    public sealed class WheelAssembly
    {
        public string name;
        public int hub, front, rear, upper;
        public readonly List<int> links = new List<int>();
        public bool steering, driven, punctured;
        public float radius = .36f, rimBend, compression, spin, rearLock;
        public Vector3 forward, up;
        public int LiveLinks(SoftStructure s) { int c = 0; foreach (int b in links) if (!s.beams[b].broken) c++; return c; }
    }
    public sealed class BodyPanel
    {
        public string name;
        public int[] nodes, mounts;
        public readonly List<int[]> faces = new List<int[]>();
        public int LiveMounts(SoftStructure s) { int c = 0; foreach (int b in mounts) if (!s.beams[b].broken) c++; return c; }
    }
    public sealed class MechanicalPart
    {
        public string name;
        public int a, b;
        public float span, crush, sensitivity;
        public float Condition => Mathf.Clamp01(1 - crush * sensitivity);
        public Vector3 Position(SoftStructure s) => (s.nodes[a].position + s.nodes[b].position) * .5f;
    }

    public sealed class SacrificialSedan
    {
        public readonly SoftStructure structure;
        public readonly List<WheelAssembly> wheels = new List<WheelAssembly>();
        public readonly List<BodyPanel> panels = new List<BodyPanel>();
        public readonly List<MechanicalPart> parts = new List<MechanicalPart>();
        public readonly List<int[]> shell = new List<int[]>();
        public int chassisCount;
        public float throttle, steering, brake, temperature = 80, fuel = 1;
        public float DriveForceLastStep, DistanceTravelled;
        public bool handbrake;
        public Vector3 Center { get { Vector3 p = Vector3.zero; for (int i = 12; i < 30; i++) p += structure.nodes[i].position; return p / 18; } }
        public Vector3 Forward => ((structure.nodes[Index(1,0,4)].position - structure.nodes[Index(1,0,2)].position)).normalized;
        public Vector3 Right => (structure.nodes[Index(2,0,3)].position - structure.nodes[Index(0,0,3)].position).normalized;
        public Vector3 Up => Vector3.Cross(Forward, Right).normalized;
        public Vector3 Velocity { get { Vector3 v = Vector3.zero; for (int i = 12; i < 30; i++) v += structure.nodes[i].velocity; return v / 18; } }
        public float Speed => Vector3.Dot(Velocity, Forward);
        public Quaternion Orientation => Forward.sqrMagnitude > .5f && Up.sqrMagnitude > .5f ? Quaternion.LookRotation(Forward, Up) : Quaternion.identity;
        public static int Index(int x, int y, int z) => z * 6 + y * 3 + x;

        public SacrificialSedan(Vector3 origin, SphereCollider probe)
        {
            structure = new SoftStructure(probe);
            for (int z = 0; z < 7; z++)
                for (int y = 0; y < 2; y++)
                    for (int x = 0; x < 3; x++)
                        structure.AddNode(origin + new Vector3((x - 1) * .83f, y == 0 ? .58f : 1.04f, (z - 3) * .8f));
            chassisCount = structure.nodes.Count;
            for (int a = 0; a < chassisCount; a++)
                for (int b = a + 1; b < chassisCount; b++)
                {
                    var d = structure.nodes[a].original - structure.nodes[b].original;
                    if (Mathf.Abs(d.x) <= .84f && Mathf.Abs(d.y) <= .47f && Mathf.Abs(d.z) <= .81f)
                    {
                        bool end = a / 6 <= 1 || b / 6 >= 5;
                        int beam=structure.AddBeam(a, b, end ? 2e-8f : 6e-9f, end ? .017f : .15f, end ? .85f : 1.6f);
                        structure.beams[beam].plasticRate=end?120:24;
                    }
                }
            // Rendered lower shell uses the exact collision / structural nodes.
            for (int z = 0; z < 6; z++)
            {
                Quad(Index(0,0,z), Index(0,1,z), Index(0,1,z+1), Index(0,0,z+1));
                Quad(Index(2,0,z+1), Index(2,1,z+1), Index(2,1,z), Index(2,0,z));
                for (int x = 0; x < 2; x++) Quad(Index(x,0,z+1), Index(x+1,0,z+1), Index(x+1,0,z), Index(x,0,z));
            }
            Quad(Index(0,0,0), Index(2,0,0), Index(2,1,0), Index(0,1,0));
            Quad(Index(2,0,6), Index(0,0,6), Index(0,1,6), Index(2,1,6));
            int[] roof = new int[4];
            Vector3[] roofP = { new Vector3(-.68f,1.72f,-.75f), new Vector3(.68f,1.72f,-.75f), new Vector3(.68f,1.72f,.65f), new Vector3(-.68f,1.72f,.65f) };
            for (int i=0;i<4;i++) roof[i] = structure.AddNode(origin + roofP[i], 16);
            for (int i=0;i<4;i++) for (int j=i+1;j<4;j++) structure.AddBeam(roof[i],roof[j], 2e-8f, .13f, 1.2f);
            int[] baseRoof = { Index(0,1,2),Index(2,1,2),Index(2,1,4),Index(0,1,4) };
            for (int i=0;i<4;i++) { structure.AddBeam(roof[i],baseRoof[i], 1e-8f,.16f,1.2f); structure.AddBeam(roof[i],baseRoof[(i+1)%4], 2e-8f,.16f,1.2f); }
            Quad(roof[0],roof[3],roof[2],roof[1]);
            // Windows are open; narrow structural pillars remain visible in the render layer.
            Panel("Hood", origin, new [] { new Vector3(-.85f,1.09f,.82f),new Vector3(.85f,1.09f,.82f),new Vector3(.85f,1.09f,2.4f),new Vector3(-.85f,1.09f,2.4f) },
                new []{Index(0,1,4),Index(2,1,4),Index(2,1,6)});
            Panel("Trunk", origin, new [] { new Vector3(.85f,1.09f,-.82f),new Vector3(-.85f,1.09f,-.82f),new Vector3(-.85f,1.09f,-2.4f),new Vector3(.85f,1.09f,-2.4f) },
                new []{Index(2,1,2),Index(0,1,2),Index(0,1,0)});
            for (int side=0;side<2;side++)
            {
                float x=side==0?-.88f:.88f; int sx=side==0?0:2;
                Panel(side==0?"Left door":"Right door",origin,new []{new Vector3(x,.63f,.78f),new Vector3(x,1.12f,.78f),new Vector3(x,1.12f,-.78f),new Vector3(x,.63f,-.78f)},
                    new []{Index(sx,0,4),Index(sx,1,4),Index(sx,1,2)});
            }
            for (int z=1;z<=5;z+=4) for (int side=0;side<2;side++)
            {
                int x=side==0?0:2;
                var w=new WheelAssembly { name=(z==5?"Front ":"Rear ")+(side==0?"L":"R"), steering=z==5, driven=z==1,
                    front=Index(x,0,z+1), rear=Index(x,0,z-1), upper=Index(x,1,z),
                    hub=structure.AddNode(origin+new Vector3(side==0?-1.01f:1.01f,.4f,(z-3)*.8f),30,.36f) };
                w.links.Add(structure.AddBeam(w.hub,w.front,2e-7f,.23f,1.25f,true));
                w.links.Add(structure.AddBeam(w.hub,w.rear,2e-7f,.23f,1.25f,true));
                w.links.Add(structure.AddBeam(w.hub,w.upper,2e-5f,.3f,1.5f,true));
                w.links.Add(structure.AddBeam(w.hub,Index(1,0,z),4e-7f,.3f,1.5f,true));
                wheels.Add(w);
            }
            Part("Radiator",Index(0,1,6),Index(2,1,5),2.5f);
            Part("Engine",Index(1,0,4),Index(1,1,5),1.35f);
            Part("Transmission",Index(1,0,3),Index(1,0,4),1.2f);
            Part("Fuel tank",Index(0,0,1),Index(2,0,1),1.5f);
        }
        void Quad(int a,int b,int c,int d) { shell.Add(new[]{a,b,c,d}); }
        void Part(string name,int a,int b,float sensitivity) { parts.Add(new MechanicalPart { name=name,a=a,b=b,span=Vector3.Distance(structure.nodes[a].position,structure.nodes[b].position),sensitivity=sensitivity }); }
        void Panel(string name,Vector3 origin,Vector3[] points,int[] anchors)
        {
            var p=new BodyPanel {name=name,nodes=new int[10],mounts=new int[3]};
            Vector3 center=(points[0]+points[1]+points[2]+points[3])*.25f;
            Vector3 normal=Vector3.Cross(points[1]-points[0],points[3]-points[0]).normalized;
            if(Vector3.Dot(normal,center-new Vector3(0,.65f,0))<0)normal=-normal;
            // A supported sheet, not a floppy coplanar four-point truss. The inner
            // reinforcement adds bending stiffness while all rest lengths can yield.
            for(int row=0;row<3;row++)for(int col=0;col<3;col++)
            {
                Vector3 a=Vector3.Lerp(points[0],points[1],col*.5f),b=Vector3.Lerp(points[3],points[2],col*.5f);
                p.nodes[row*3+col]=structure.AddNode(origin+Vector3.Lerp(a,b,row*.5f),2,.055f);
            }
            p.nodes[9]=structure.AddNode(origin+center-normal*.18f,2,.045f);
            for(int i=0;i<9;i++)
            {
                int brace=structure.AddBeam(p.nodes[i],p.nodes[9],3e-9f,.07f,1.3f);structure.beams[brace].damping=.3f;
                for(int j=i+1;j<9;j++)if(Mathf.Abs(i%3-j%3)<=1&&Mathf.Abs(i/3-j/3)<=1)
                {int edge=structure.AddBeam(p.nodes[i],p.nodes[j],3e-9f,.065f,1.3f);structure.beams[edge].damping=.2f;}
            }
            for(int row=0;row<2;row++)for(int col=0;col<2;col++)
            {int i=row*3+col;p.faces.Add(new[]{p.nodes[i],p.nodes[i+1],p.nodes[i+4],p.nodes[i+3]});}
            int[] corners={0,2,8};
            // Each hinge/latch is one physical mount with a triangulated attachment.
            // Break the whole attachment when one brace fails; no invisible tethers.
            for(int i=0;i<3;i++)
            {
                int anchor=anchors[i];int x=anchor%3, y=(anchor/3)%2, z=anchor/6;
                int[] supports={anchor,Index(1,y,z),Index(x,1-y,z)};
                var group=new int[3];
                for(int k=0;k<3;k++)group[k]=structure.AddBeam(p.nodes[corners[i]],supports[k],2e-9f,i==2?.12f:.24f,i==2?.22f:.50f,true);
                structure.GroupAttachment(group);p.mounts[i]=group[0];
            }
            panels.Add(p);
        }
        public void Tick(float dt)
        {
            Vector3 before=Center;
            foreach(var p in parts)
            {
                float strain=Mathf.Abs(Vector3.Distance(structure.nodes[p.a].position,structure.nodes[p.b].position)-p.span)/p.span;
                p.crush=Mathf.Max(p.crush,strain);
            }
            temperature=Mathf.Clamp(temperature+((1-parts[0].Condition)*2.2f*Mathf.Abs(throttle)-.25f*parts[0].Condition)*dt,75,160);
            fuel=Mathf.Max(0,fuel-(1-parts[3].Condition)*.003f*dt);
            DriveForceLastStep=0;
            structure.Step(dt,ApplyWheelForces);
            DistanceTravelled+=Vector3.Distance(before,Center);
        }
        void ApplyWheelForces(float dt)
        {
            float power=parts[1].Condition*parts[2].Condition*Mathf.Clamp01((160-temperature)/35)*(fuel>0?1:0);
            Vector3 up=Up;
            foreach(var w in wheels)
            {
                var hub=structure.nodes[w.hub];
                Vector3 mountForward=(structure.nodes[w.front].position-structure.nodes[w.rear].position).normalized;
                Vector3 mountUp=(structure.nodes[w.upper].position-(structure.nodes[w.front].position+structure.nodes[w.rear].position)*.5f).normalized;
                w.up=mountUp;
                w.forward=Quaternion.AngleAxis(w.steering?steering*29:0,mountUp)*mountForward;
                int links=w.LiveLinks(structure);
                w.radius=w.punctured?.255f:.36f;
                hub.radius=w.radius;
                // Tire abuse is local: direct hub deceleration and displaced suspension, not global impact HP.
                float mountDist=Vector3.Distance(hub.position,structure.nodes[w.upper].position);
                w.rimBend=Mathf.Max(w.rimBend,Mathf.Max(0,Mathf.Abs(mountDist-.67f)-.2f));
                if(w.rimBend>.23f) w.punctured=true;
                w.compression=0;
                w.rearLock=Mathf.MoveTowards(w.rearLock,handbrake&&!w.steering?1:0,dt*(handbrake?9:5));
                if(links<2) continue; // A hanging wheel cannot transmit useful drive torque.
                if(!Physics.Raycast(hub.position, -up, out RaycastHit hit,w.radius+.08f,1<<0,QueryTriggerInteraction.Ignore)) continue;
                if(Vector3.Dot(hit.normal,up)<.25f) continue;
                w.compression=Mathf.Clamp01((w.radius+.08f-hit.distance)/.12f);
                Vector3 f=Vector3.ProjectOnPlane(w.forward,hit.normal).normalized;
                Vector3 lateral=Vector3.Cross(hit.normal,f).normalized;
                float longitudinal=Vector3.Dot(hub.velocity,f), slip=Vector3.Dot(hub.velocity,lateral);
                float grip=w.punctured?1300:3200;
                // A locked rear tire slides sideways. Service braking keeps cornering
                // grip on all four tires; neither path injects artificial yaw torque.
                float sideGrip=grip*Mathf.Lerp(1,.18f,w.rearLock);
                float corneringStiffness=Mathf.Lerp(1900,300,w.rearLock);
                float sideForce=Mathf.Clamp(-slip*corneringStiffness,-sideGrip,sideGrip);
                float service=Mathf.Clamp01(brake);
                float drive=w.driven?throttle*power*2900*(1-service)*(1-w.rearLock):0;
                drive*=Mathf.Clamp01((32-Mathf.Abs(longitudinal))/8);
                float brakeCapacity=Mathf.Max(service*grip*(w.steering?1.18f:.96f),w.rearLock*grip*.78f);
                float stop=Mathf.Clamp(-longitudinal*2200,-brakeCapacity,brakeCapacity);
                Vector3 force=f*(drive+stop-longitudinal*(w.punctured?130:35))+lateral*sideForce;
                force=Vector3.ClampMagnitude(force,grip*1.3f);
                hub.force+=force;
                DriveForceLastStep+=Mathf.Abs(drive)*dt;
                w.spin+=longitudinal/w.radius*dt*(1-w.rearLock);
            }
        }
        public void Launch(Vector3 velocity)
        {
            // Controlled test impulse. Broken debris is deliberately excluded.
            var connected=new HashSet<int>{Index(1,0,3)};
            bool changed=true;
            while(changed) { changed=false; foreach(var b in structure.beams) if(!b.broken && (connected.Contains(b.a)||connected.Contains(b.b))) { changed|=connected.Add(b.a); changed|=connected.Add(b.b); } }
            foreach(int i in connected) structure.nodes[i].velocity=velocity;
        }
        public void Recover(Vector3 target)
        { structure.Relocate(target,Quaternion.Inverse(Orientation),Center,Index(1,0,3)); }
    }
}




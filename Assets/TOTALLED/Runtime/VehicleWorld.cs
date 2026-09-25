using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    // Shared substeps allow a contact to load both deformable graphs before yield.
    public sealed class VehicleWorld
    {
        sealed class Surface {public int a,b,c;public float perimeter;public bool torn;}
        readonly Dictionary<SacrificialSedan,List<Surface>> surfaces=new Dictionary<SacrificialSedan,List<Surface>>();
        public int ContactCount {get;private set;}
#if UNITY_EDITOR
        public double solveMs,contactMs;
#endif
        readonly NodeBroadphase broadphase=new NodeBroadphase();
        Vector3[] starts=new Vector3[4];
        UnityEngine.Bounds[] carBounds=new UnityEngine.Bounds[4];
        public void Step(IList<SacrificialSedan> cars,float dt)
        {
            ContactCount=0;
#if UNITY_EDITOR
            solveMs=contactMs=0;
#endif
            if(starts.Length<cars.Count){System.Array.Resize(ref starts,cars.Count*2);System.Array.Resize(ref carBounds,cars.Count*2);}
            for(int i=0;i<cars.Count;i++){var car=cars[i];starts[i]=car.Center;Register(car);car.PrepareTick(dt);car.structure.Begin(dt);}
            const int substeps=10,iterations=8;float h=dt/substeps;
            for(int sub=0;sub<substeps;sub++)
            {
                foreach(var car in cars)car.structure.Predict(h,car.ApplyWheelForces);
                for(int it=0;it<iterations;it++)
                {
                    #if UNITY_EDITOR
                    long stamp=System.Diagnostics.Stopwatch.GetTimestamp();
#endif
                    foreach(var car in cars)car.structure.Solve(h,it);
#if UNITY_EDITOR
                    solveMs+=(System.Diagnostics.Stopwatch.GetTimestamp()-stamp)*1000.0/System.Diagnostics.Stopwatch.Frequency;
#endif
                    if(it!=3&&it!=7)continue;
                    #if UNITY_EDITOR
                    stamp=System.Diagnostics.Stopwatch.GetTimestamp();
#endif
                    for(int i=0;i<cars.Count;i++)carBounds[i]=Bounds(cars[i]);
                    for(int a=0;a<cars.Count;a++)for(int b=a+1;b<cars.Count;b++)
                    {
                        if(!carBounds[a].Intersects(carBounds[b]))continue;
                        if(it==3){Contact(cars[a],cars[b],h);Contact(cars[b],cars[a],h);}
                        else {Contact(cars[b],cars[a],h);Contact(cars[a],cars[b],h);}
                    }
#if UNITY_EDITOR
                    contactMs+=(System.Diagnostics.Stopwatch.GetTimestamp()-stamp)*1000.0/System.Diagnostics.Stopwatch.Frequency;
#endif
                }
                foreach(var car in cars)car.structure.Finish(h);
            }
            for(int i=0;i<cars.Count;i++)cars[i].DistanceTravelled+=Vector3.Distance(starts[i],cars[i].Center);
        }
        static Bounds Bounds(SacrificialSedan car)
        {
            var box=new Bounds(car.structure.nodes[0].position,Vector3.one);
            foreach(var n in car.structure.nodes){box.Encapsulate(n.position);box.Encapsulate(n.previous);}
            box.Expand(.8f);return box;
        }
        void Register(SacrificialSedan car)
        {
            if(surfaces.ContainsKey(car))return;var faces=new List<Surface>();surfaces.Add(car,faces);
            foreach(var q in car.shell)
            {
                // The cabin side opening is occupied by the deformable door.
                // Do not put an invisible second skin directly behind it.
                bool opening=true;int side=q[0]%3;
                foreach(int id in q)if(id>=42||id/6<2||id/6>4||id%3!=side||side==1)opening=false;
                if(!opening)Quad(car,faces,q);
            }
            foreach(var panel in car.panels)foreach(var q in panel.faces)Quad(car,faces,q);
            Quad(car,faces,new[]{SacrificialSedan.Index(0,1,4),SacrificialSedan.Index(2,1,4),44,45});
            Quad(car,faces,new[]{SacrificialSedan.Index(2,1,2),SacrificialSedan.Index(0,1,2),42,43});
        }
        static void Quad(SacrificialSedan car,List<Surface> faces,int[] q)
        {Triangle(car,faces,q[0],q[1],q[2]);Triangle(car,faces,q[0],q[2],q[3]);}
        static void Triangle(SacrificialSedan car,List<Surface> faces,int a,int b,int c)
        {
            var n=car.structure.nodes;
            faces.Add(new Surface{a=a,b=b,c=c,perimeter=(n[a].position-n[b].position).magnitude+(n[b].position-n[c].position).magnitude+(n[c].position-n[a].position).magnitude});
        }
        void Contact(SacrificialSedan pointCar,SacrificialSedan faceCar,float h)
        {
            var points=pointCar.structure;var face=faceCar.structure;broadphase.Refit(points);
            foreach(var t in surfaces[faceCar])
            {
                var a=face.nodes[t.a];var b=face.nodes[t.b];var c=face.nodes[t.c];
                if(t.torn)continue;
                if((a.position-b.position).magnitude+(b.position-c.position).magnitude+(c.position-a.position).magnitude>t.perimeter*1.8f){t.torn=true;continue;}
                var box=new Bounds(a.position,Vector3.zero);box.Encapsulate(b.position);box.Encapsulate(c.position);box.Encapsulate(a.previous);box.Encapsulate(b.previous);box.Encapsulate(c.previous);
                broadphase.Query(box);
                foreach(int pointIndex in broadphase.candidates)
                {
                    var p=points.nodes[pointIndex];
                    Vector3 weights=Closest(p.position,a.position,b.position,c.position);
                    Vector3 closest=a.position*weights.x+b.position*weights.y+c.position*weights.z;
                    Vector3 delta=p.position-closest;float distance=delta.magnitude,radius=p.radius+.025f;
                    Vector3 normal=Vector3.Cross(b.position-a.position,c.position-a.position).normalized;
                    if(normal.sqrMagnitude<.5f)continue;
                    Vector3 oldClosest=a.previous*weights.x+b.previous*weights.y+c.previous*weights.z;
                    float oldSide=Vector3.Dot(p.previous-oldClosest,normal),side=Vector3.Dot(delta,normal);
                    bool crossed=oldSide*side<0&&Mathf.Abs(oldSide)<.6f&&weights.x>.001f&&weights.y>.001f&&weights.z>.001f;
                    if(distance>=radius&&!crossed)continue;
                    Vector3 direction=crossed?normal*Mathf.Sign(oldSide):(distance>1e-5f?delta/distance:normal*Mathf.Sign(oldSide));
                    float depth=crossed?radius+Mathf.Abs(side):radius-distance;
                    float inverse=p.inverseMass+weights.x*weights.x*a.inverseMass+weights.y*weights.y*b.inverseMass+weights.z*weights.z*c.inverseMass;
                    float lambda=Mathf.Min(depth,.12f)/inverse;
                    Vector3 correction=direction*lambda;
                    p.position+=correction*p.inverseMass;broadphase.Update(pointIndex);
                    a.position-=correction*(weights.x*a.inverseMass);b.position-=correction*(weights.y*b.inverseMass);c.position-=correction*(weights.z*c.inverseMass);
                    ContactCount++;
                    if(lambda/h>15)
                    {
                        points.PeakImpact=Mathf.Max(points.PeakImpact,lambda/(h*h));face.PeakImpact=Mathf.Max(face.PeakImpact,lambda/(h*h));
                        if(points.contacts.Count<120)points.contacts.Add(new ContactSample{point=closest,impulse=correction/h});
                        if(face.contacts.Count<120)face.contacts.Add(new ContactSample{point=closest,impulse=-correction/h});
                    }
                }
            }
        }
        // Barycentric closest point, including edge and vertex Voronoi regions.
        static Vector3 Closest(Vector3 p,Vector3 a,Vector3 b,Vector3 c)
        {
            Vector3 ab=b-a,ac=c-a,ap=p-a;float d1=Vector3.Dot(ab,ap),d2=Vector3.Dot(ac,ap);
            if(d1<=0&&d2<=0)return new Vector3(1,0,0);
            Vector3 bp=p-b;float d3=Vector3.Dot(ab,bp),d4=Vector3.Dot(ac,bp);
            if(d3>=0&&d4<=d3)return new Vector3(0,1,0);
            float vc=d1*d4-d3*d2;if(vc<=0&&d1>=0&&d3<=0){float v=d1/(d1-d3);return new Vector3(1-v,v,0);}
            Vector3 cp=p-c;float d5=Vector3.Dot(ab,cp),d6=Vector3.Dot(ac,cp);
            if(d6>=0&&d5<=d6)return new Vector3(0,0,1);
            float vb=d5*d2-d1*d6;if(vb<=0&&d2>=0&&d6<=0){float w=d2/(d2-d6);return new Vector3(1-w,0,w);}
            float va=d3*d6-d5*d4;if(va<=0&&d4-d3>=0&&d5-d6>=0){float w=(d4-d3)/((d4-d3)+(d5-d6));return new Vector3(0,1-w,w);}
            float denom=va+vb+vc;if(Mathf.Abs(denom)<1e-12f)return new Vector3(1,0,0);
            return new Vector3(va,vb,vc)/denom;
        }
    }
}

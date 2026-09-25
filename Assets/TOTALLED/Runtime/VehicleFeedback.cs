using System;
using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    // Cosmetic feedback observes the solver; it never pushes or repairs the car.
    public sealed class VehicleFeedback : MonoBehaviour
    {
        SacrificialSedan car;
        AudioSource engine, tires, impacts;
        AudioClip thud;
        float lastPeak, nextImpact;
        Mesh skidMesh;
        public int SkidVertices=>vertices.Count;
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<int> indices=new List<int>();
        readonly Vector3[] last=new Vector3[4];
        readonly bool[] marking=new bool[4];
        public void Initialize(SacrificialSedan vehicle)
        {
            car=vehicle;
            engine=Source("Engine",Tone("Old four cylinder",true,false),true);
            tires=Source("Tire scrub",Tone("Tire squeal",false,false),true);
            thud=Tone("Metal impact",false,true);impacts=Source("Impacts",null,false);
            var marks=new GameObject("Persistent tire marks");marks.layer=2;
            skidMesh=new Mesh();skidMesh.MarkDynamic();marks.AddComponent<MeshFilter>().sharedMesh=skidMesh;
            var renderer=marks.AddComponent<MeshRenderer>();renderer.sharedMaterial=SedanView.Material(new Color(.052f,.049f,.045f),0,.05f);
            renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
        }
        AudioSource Source(string name,AudioClip clip,bool loop)
        {
            var child=new GameObject(name);child.transform.SetParent(transform,false);
            var source=child.AddComponent<AudioSource>();source.clip=clip;source.loop=loop;source.volume=loop?0:1;
            source.spatialBlend=.7f;source.minDistance=5;source.maxDistance=42;source.rolloffMode=AudioRolloffMode.Linear;source.dopplerLevel=0;
            if(loop)source.Play();return source;
        }
        static AudioClip Tone(string name,bool motor,bool impact)
        {
            const int rate=22050;int length=impact?8820:22050;var samples=new float[length];var random=new System.Random(73);float low=0;
            for(int i=0;i<length;i++)
            {
                float t=i/(float)rate,noise=(float)random.NextDouble()*2-1;low=Mathf.Lerp(low,noise,.15f);
                if(motor)samples[i]=(.40f*Mathf.Sin(t*2*Mathf.PI*45)+.22f*Mathf.Sin(t*2*Mathf.PI*90)+.10f*Mathf.Sin(t*2*Mathf.PI*135)+low*.13f)*(.85f+.15f*Mathf.Sin(t*2*Mathf.PI*15));
                else if(impact)samples[i]=(low*.8f+Mathf.Sin(t*2*Mathf.PI*66)*.45f)*Mathf.Exp(-t*16)*Mathf.Min(1,t*500);
                else samples[i]=noise*.13f+Mathf.Sin(t*2*Mathf.PI*810+Mathf.Sin(t*2*Mathf.PI*7)*2)*.14f;
            }
            var clip=AudioClip.Create(name,length,1,rate,false);clip.SetData(samples,0);return clip;
        }
        void Update()
        {
            if(car==null)return;
            float dt=Time.deltaTime,speed=Mathf.Abs(car.Speed);
            // Time belongs to the physical specimen, so pause also silences feedback.
            if(car.structure.Time>lastTime)lastAdvance=Time.unscaledTime;lastTime=car.structure.Time;bool movingTime=Time.unscaledTime-lastAdvance<.08f;
            engine.transform.position=car.parts[1].Position(car.structure);
            tires.transform.position=car.Center;impacts.transform.position=car.Center;
            engine.pitch=Mathf.Lerp(engine.pitch,.70f+Mathf.Min(speed,22)*.052f+Mathf.Abs(car.throttle)*.45f,1-Mathf.Exp(-dt*5));
            engine.volume=movingTime&&car.parts[1].Condition>.05f&&car.fuel>0?.085f+Mathf.Abs(car.throttle)*.09f:0;
            float scrub=0;bool changed=false;
            for(int i=0;i<car.wheels.Count;i++)
            {
                var wheel=car.wheels[i];float slip=Mathf.Max(Mathf.Abs(wheel.lateralSlip),speed*wheel.rearLock*.6f);
                bool markingNow=movingTime&&wheel.compression>.05f&&slip>1.6f&&speed>2;
                if(markingNow)scrub=Mathf.Max(scrub,slip);
                Vector3 hub=car.structure.nodes[wheel.hub].position;
                bool ground=Physics.Raycast(hub,Vector3.down,out RaycastHit hit,wheel.radius+.25f,1<<0,QueryTriggerInteraction.Ignore);
                markingNow&=ground;Vector3 p=ground?hit.point+hit.normal*.008f:hub;
                if(markingNow&&marking[i])
                {
                    Vector3 path=p-last[i];float distance=path.magnitude;
                    if(distance>.12f&&distance<2)
                    {
                        Vector3 width=Vector3.Cross(hit.normal,path.normalized).normalized*.10f;
                        vertices.AddRange(new[]{last[i]-width,last[i]+width,p+width,p-width});last[i]=p;changed=true;
                    }
                    else if(distance>=2)last[i]=p;
                }
                else last[i]=p;
                marking[i]=markingNow;
            }
            tires.volume=Mathf.MoveTowards(tires.volume,Mathf.Clamp01((scrub-1.6f)/6)*.15f,dt*.9f);
            if(changed)
            {
                if(vertices.Count>1024)vertices.RemoveRange(0,vertices.Count-1024);
                indices.Clear();for(int i=0;i<vertices.Count;i+=4)indices.AddRange(new[]{i,i+2,i+1,i,i+3,i+2});
                skidMesh.Clear();skidMesh.SetVertices(vertices);skidMesh.SetTriangles(indices,0);skidMesh.RecalculateNormals();skidMesh.RecalculateBounds();
            }
            float peak=car.structure.PeakImpact;
            if(movingTime&&peak>18000&&peak>lastPeak*1.12f&&Time.time>nextImpact)
            {impacts.pitch=1;impacts.PlayOneShot(thud,Mathf.Clamp01(peak/180000)*.6f);nextImpact=Time.time+.22f;}
            lastPeak=peak;
        }
        float lastTime,lastAdvance;
    }
}


using System;
using System.IO;
using Totalled;
using UnityEngine;

public static class CrashLabTargetChecks
{
    [Serializable] public class Timing {public int direction;public double meanMs,p95Ms,maxMs,solveMs,contactMs;}
    [Serializable] public class Timings {public string scope="Unity editor physics only; 150 ticks per collision scenario. Not browser FPS.";public System.Collections.Generic.List<Timing> scenarios=new System.Collections.Generic.List<Timing>();}
    public static void Run(CrashLab lab,Action<bool,string> check)
    {
        var timings=new Timings();
        var g=new GameObject("Target verification probe");g.layer=2;g.transform.position=Vector3.down*1000;
        var probe=g.AddComponent<SphereCollider>();probe.isTrigger=true;
        var gravity=Physics.gravity;
        try
        {
            Physics.gravity=Vector3.zero;
            var a=new SacrificialSedan(new Vector3(0,10,-6),probe);var b=new SacrificialSedan(new Vector3(0,10,0),probe);
            var pair=new[]{a,b};var world=new VehicleWorld();a.Launch(Vector3.forward*12);
            Vector3 initialMomentum=Momentum(pair);float initialEnergy=Energy(pair);
            for(int frame=0;frame<70;frame++)world.Step(pair,.02f);
            Vector3 expected=initialMomentum*Mathf.Exp(-.035f*1.4f);
            check((Momentum(pair)-expected).magnitude/expected.magnitude<.02f,"Free-space impact conserves combined momentum within 2%, accounting for existing air damping.");
            check(Energy(pair)<initialEnergy*1.05f,"Two-car collision does not create net kinetic energy.");
        }
        finally{Physics.gravity=gravity;}
        for(int direction=0;direction<3;direction++)
        {
            var target=new SacrificialSedan(Vector3.zero,probe);
            foreach(var n in target.structure.nodes){n.position=CrashLabArt.TargetPosition+Quaternion.Euler(0,direction==2?-90:90,0)*n.position;n.previous=n.position;}
            var car=new SacrificialSedan(direction!=1?new Vector3(12,0,-2):new Vector3(3,0,9),probe);
            var world=new VehicleWorld();var pair=new[]{car,target};
            for(int i=0;i<150;i++)world.Step(pair,.02f);
            Vector3 before=target.Center;float initial=target.structure.PlasticTotal;
            car.Launch((direction!=1?Vector3.forward:Vector3.right)*18);
            var timer=System.Diagnostics.Stopwatch.StartNew();
            var times=new double[150];int contacts=0;double solve=0,contact=0;
            for(int i=0;i<150;i++){var tick=System.Diagnostics.Stopwatch.StartNew();world.Step(pair,.02f);times[i]=tick.Elapsed.TotalMilliseconds;contacts+=world.ContactCount;solve+=world.solveMs;contact+=world.contactMs;}
            Debug.Log($"PHASE BENCH: solve {solve/150:0.00}, contact {contact/150:0.00} ms");Array.Sort(times);Debug.Log($"PAIR DISTRIBUTION: median {times[75]:0.00}, p95 {times[142]:0.00}, max {times[149]:0.00} ms");
            timer.Stop();Debug.Log($"PAIR BENCH: {timer.Elapsed.TotalMilliseconds/150:0.00} ms per physics tick");
            timings.scenarios.Add(new Timing{direction=direction,meanMs=timer.Elapsed.TotalMilliseconds/150,p95Ms=times[142],maxMs=times[149],solveMs=solve/150,contactMs=contact/150});
            float travel=Vector3.Distance(before,target.Center);
            check(car.structure.Finite()&&target.structure.Finite()&&contacts>0,"Two-way vehicle contact remains finite and detects surface contacts.");
            check(travel>.5f,$"Unpowered target is pushed by the impact ({travel:0.00} m).");
            check(car.structure.PlasticTotal>.1f&&target.structure.PlasticTotal>initial+.05f,
                $"Both vehicles deform: player {car.structure.PlasticTotal:0.00}, target {target.structure.PlasticTotal:0.00} m plastic travel.");
            if(direction!=1)
            {
                float sidePlastic=0;
                foreach(var beam in target.structure.beams)
                    if(beam.a<42&&beam.b<42&&beam.a/6>=2&&beam.a/6<=4&&beam.b/6>=2&&beam.b/6<=4)sidePlastic+=beam.plastic;
                check(sidePlastic>.08f,$"T-bone yields cabin-side members ({sidePlastic:0.000} m accumulated plastic travel).");
                var panel=target.panels[direction==0?3:2];var nodes=target.structure.nodes;
                Vector3 corner=(nodes[panel.nodes[0]].position+nodes[panel.nodes[2]].position+nodes[panel.nodes[6]].position+nodes[panel.nodes[8]].position)*.25f;
                Vector3 normal=Vector3.Cross(nodes[panel.nodes[2]].position-nodes[panel.nodes[0]].position,nodes[panel.nodes[6]].position-nodes[panel.nodes[0]].position).normalized;
                float dent=Mathf.Abs(Vector3.Dot(nodes[panel.nodes[4]].position-corner,normal));
                check(dent>.10f,$"T-bone leaves a permanent {panel.name} dent ({dent:0.000} m depth after settling).");
                var sideView=new SedanView(target,new Color(.16f,.38f,.40f));sideView.Refresh();
                Capture(lab,"side-dent-"+direction,target.Center+target.Right*(direction==0?5:-5)+Vector3.up*2-target.Forward*2,target.Center+Vector3.up*.2f);
                UnityEngine.Object.DestroyImmediate(sideView.root);
            }
            float damage=target.structure.PlasticTotal;
            car.Recover(target.Center-Vector3.forward*8+Vector3.up*.3f);car.Launch(Vector3.forward*18);
            for(int i=0;i<120;i++)world.Step(pair,.02f);
            check(target.structure.PlasticTotal>=damage&&target.structure.Finite(),"Second hit preserves target damage and remains stable.");
            var view=new SedanView(car);var targetView=new SedanView(target,new Color(.16f,.38f,.40f));view.Refresh();targetView.Refresh();
            Capture(lab,direction==0?"10-parked-target-impact":direction==1?"11-side-target-impact":"15-left-target-impact",target.Center+new Vector3(8,6,-10),(car.Center+target.Center)*.5f);
            UnityEngine.Object.DestroyImmediate(view.root);UnityEngine.Object.DestroyImmediate(targetView.root);
        }
        File.WriteAllText("Artifacts/collision-performance.json",JsonUtility.ToJson(timings,true));
        Capture(lab,"12-yard-showcase",new Vector3(-8,5,-11),new Vector3(4,1.1f,7));
        UnityEngine.Object.DestroyImmediate(g);
    }
    static Vector3 Momentum(SacrificialSedan[] cars)
    {Vector3 value=Vector3.zero;foreach(var car in cars)foreach(var n in car.structure.nodes)value+=n.velocity/n.inverseMass;return value;}
    static float Energy(SacrificialSedan[] cars)
    {float value=0;foreach(var car in cars)foreach(var n in car.structure.nodes)value+=n.velocity.sqrMagnitude/(2*n.inverseMass);return value;}
    static void Capture(CrashLab lab,string name,Vector3 eye,Vector3 look)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        var camera=lab.LabCamera;camera.transform.position=eye;camera.transform.LookAt(look);
        var rt=new RenderTexture(1600,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
        File.WriteAllBytes("Artifacts/"+name+".png",tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(rt);
    }
}

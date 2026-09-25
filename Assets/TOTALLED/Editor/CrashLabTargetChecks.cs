using System;
using System.IO;
using Totalled;
using UnityEngine;

public static class CrashLabTargetChecks
{
    public static void Run(CrashLab lab,Action<bool,string> check)
    {
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
        for(int direction=0;direction<2;direction++)
        {
            var target=new SacrificialSedan(Vector3.zero,probe);
            foreach(var n in target.structure.nodes){n.position=CrashLabArt.TargetPosition+Quaternion.Euler(0,90,0)*n.position;n.previous=n.position;}
            var car=new SacrificialSedan(direction==0?new Vector3(12,0,-2):new Vector3(3,0,9),probe);
            var world=new VehicleWorld();var pair=new[]{car,target};
            for(int i=0;i<150;i++)world.Step(pair,.02f);
            Vector3 before=target.Center;float initial=target.structure.PlasticTotal;
            car.Launch((direction==0?Vector3.forward:Vector3.right)*18);
            var timer=System.Diagnostics.Stopwatch.StartNew();
            int contacts=0;for(int i=0;i<150;i++){world.Step(pair,.02f);contacts+=world.ContactCount;}
            timer.Stop();Debug.Log($"PAIR BENCH: {timer.Elapsed.TotalMilliseconds/150:0.00} ms per physics tick");
            float travel=Vector3.Distance(before,target.Center);
            check(car.structure.Finite()&&target.structure.Finite()&&contacts>0,"Two-way vehicle contact remains finite and detects surface contacts.");
            check(travel>.5f,$"Unpowered target is pushed by the impact ({travel:0.00} m).");
            check(car.structure.PlasticTotal>.1f&&target.structure.PlasticTotal>initial+.05f,
                $"Both vehicles deform: player {car.structure.PlasticTotal:0.00}, target {target.structure.PlasticTotal:0.00} m plastic travel.");
            float damage=target.structure.PlasticTotal;
            car.Recover(target.Center-Vector3.forward*8+Vector3.up*.3f);car.Launch(Vector3.forward*18);
            for(int i=0;i<120;i++)world.Step(pair,.02f);
            check(target.structure.PlasticTotal>=damage&&target.structure.Finite(),"Second hit preserves target damage and remains stable.");
            var view=new SedanView(car);var targetView=new SedanView(target,new Color(.16f,.38f,.40f));view.Refresh();targetView.Refresh();
            Capture(lab,direction==0?"10-parked-target-impact":"11-side-target-impact",target.Center+new Vector3(8,6,-10),(car.Center+target.Center)*.5f);
            UnityEngine.Object.DestroyImmediate(view.root);UnityEngine.Object.DestroyImmediate(targetView.root);
        }
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


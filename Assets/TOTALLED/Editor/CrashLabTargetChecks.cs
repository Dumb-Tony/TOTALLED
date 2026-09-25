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
        var target=GameObject.Find("Parked target collision");Vector3 position=target.transform.position;
        for(int direction=0;direction<2;direction++)
        {
            var car=new SacrificialSedan(direction==0?new Vector3(12,0,-2):new Vector3(3,0,9),probe);
            for(int i=0;i<150;i++)car.Tick(.02f);
            car.Launch((direction==0?Vector3.forward:Vector3.right)*18);
            float peak=0;for(int i=0;i<150;i++){car.Tick(.02f);peak=Mathf.Max(peak,car.structure.PeakImpact);}
            check(car.structure.Finite()&&car.structure.PlasticTotal>.1f&&peak>1000,
                $"Parked target {(direction==0?"T-bone":"player side")} collision is finite and causes damage ({car.structure.PlasticTotal:0.00} m plastic travel).");
            check(target.transform.position==position,"Fixed target stays in place during the collision.");
            var view=new SedanView(car);view.Refresh();
            Capture(lab,direction==0?"10-parked-target-impact":"11-side-target-impact",new Vector3(20,6,-1),new Vector3(12,1,7));
            UnityEngine.Object.DestroyImmediate(view.root);
        }
        Capture(lab,"12-yard-showcase",new Vector3(-8,5,-11),new Vector3(4,1.1f,7));
        UnityEngine.Object.DestroyImmediate(g);
    }
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

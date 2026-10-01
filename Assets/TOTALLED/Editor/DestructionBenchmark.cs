using System;
using System.IO;
using Totalled;
using UnityEditor;
using UnityEngine;
public static class DestructionBenchmark
{
    [Serializable] public class Hit {public float speed,plastic,frontLength,repeatedPlastic;public int broken,loosePanels;}
    [Serializable] public class Report {public Hit[] hits;}
    public static void Idle()
    {
        CrashLabBuild.CreateScene();UnityEngine.Object.FindFirstObjectByType<CrashLab>().Initialize();
        var go=new GameObject("Idle probe");go.layer=2;go.transform.position=Vector3.down*1000;var probe=go.AddComponent<SphereCollider>();probe.isTrigger=true;
        var car=new SacrificialSedan(Vector3.zero,probe);for(int f=0;f<400;f++)car.Tick(.02f);
        foreach(var b in car.structure.beams)if(b.plastic>.0001f)Debug.Log($"IDLE BEAM {b.a}-{b.b}: {b.plastic} yield {b.yield}");
        Debug.Log("IDLE TOTAL "+car.structure.PlasticTotal);EditorApplication.Exit(0);
    }
    public static void Run()
    {
        CrashLabBuild.CreateScene();UnityEngine.Object.FindFirstObjectByType<CrashLab>().Initialize();
        var go=new GameObject("Destruction benchmark probe");go.layer=2;go.transform.position=Vector3.down*1000;var probe=go.AddComponent<SphereCollider>();probe.isTrigger=true;
        var result=new Report{hits=new Hit[2]};
        for(int i=0;i<2;i++){
            var car=new SacrificialSedan(Vector3.zero,probe);for(int f=0;f<250;f++)car.Tick(.02f);
            car.Recover(new Vector3(0,1.1f,14));car.Launch(Vector3.forward*(i==0?12:18));for(int f=0;f<150;f++)car.Tick(.02f);
            var hit=new Hit{speed=i==0?12:18,plastic=car.structure.PlasticTotal,broken=car.structure.BrokenCount};
            Vector3 tip=(car.structure.nodes[36].position+car.structure.nodes[38].position)*.5f;
            hit.frontLength=Vector3.Dot(tip-car.Center,car.Forward);
            foreach(var p in car.panels)if(p.LiveMounts(car.structure)<3)hit.loosePanels++;
            car.Recover(new Vector3(0,1.1f,14));car.Launch(Vector3.forward*hit.speed);for(int f=0;f<150;f++)car.Tick(.02f);
            hit.repeatedPlastic=car.structure.PlasticTotal;result.hits[i]=hit;
        }
        File.WriteAllText("Artifacts/destruction-benchmark.json",JsonUtility.ToJson(result,true));EditorApplication.Exit(0);
    }
}

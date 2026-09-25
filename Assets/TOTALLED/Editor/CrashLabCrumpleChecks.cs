using System;
using Totalled;
using UnityEngine;
public static class CrashLabCrumpleChecks
{
    public static void InspectIdle()
    {
        CrashLabBuild.CreateScene();UnityEngine.Object.FindFirstObjectByType<CrashLab>().Initialize();
        var go=new GameObject("Idle probe");go.layer=2;go.transform.position=Vector3.down*1000;var probe=go.AddComponent<SphereCollider>();probe.isTrigger=true;
        var car=new SacrificialSedan(Vector3.zero,probe);for(int i=0;i<400;i++)car.Tick(.02f);
        Debug.Log("IDLE TOTAL "+car.structure.PlasticTotal);
        foreach(var b in car.structure.beams)if(b.plastic>.0005f)Debug.Log($"IDLE {b.a}-{b.b} plastic={b.plastic} yield={b.yield} from={car.structure.nodes[b.a].original} to={car.structure.nodes[b.b].original}");
        UnityEngine.Object.DestroyImmediate(go);CrashLabBuild.VerifyVisuals();
    }
    public static void Run(Action<bool,string> check)
    {
        var go=new GameObject("Crumple test probe");go.layer=2;go.transform.position=Vector3.down*1000;var probe=go.AddComponent<SphereCollider>();probe.isTrigger=true;
        try
        {
            foreach(int end in new[]{1,-1})
            {
                var car=new SacrificialSedan(Vector3.zero,probe);float mass=0;foreach(var n in car.structure.nodes)mass+=1/n.inverseMass;
                check(Mathf.Abs(mass-1272)<.01f,"Extra folding sections preserve the sedan's 1272 kg node mass.");
                for(int i=0;i<250;i++)car.Tick(.02f);
                car.Recover(new Vector3(0,1.1f,end*14));car.Launch(Vector3.forward*end*18);
                for(int i=0;i<110;i++)car.Tick(.02f);
                float outer=0,inner=0,cabin=0;int folds=0;
                foreach(var b in car.structure.beams)
                {
                    if(!car.IsBodyNode(b.a)||!car.IsBodyNode(b.b))continue;
                    float z=(car.structure.nodes[b.a].original.z+car.structure.nodes[b.b].original.z)*.5f;
                    if(end*z>2)outer+=b.plastic;else if(end*z>1.6f)inner+=b.plastic;else if(Mathf.Abs(z)<.8f)cabin+=b.plastic;
                    if((b.a>=90||b.b>=90)&&b.plastic>.001f)folds++;
                }
                check(outer>.01f&&inner>.01f&&folds>3,$"{(end>0?"Front":"Rear")} impact loads both folding stages: outer {outer:0.000} m, inner {inner:0.000} m; {folds} yielded fold members.");
                check(outer+inner>cabin*2,$"Crumple sections absorb more permanent deformation than the cabin ({outer+inner:0.000} vs {cabin:0.000} m).");
                check(car.structure.Finite(),"Refined crumple cage remains finite after a real wall collision.");
            }
        }
        finally{UnityEngine.Object.DestroyImmediate(go);}
    }
}

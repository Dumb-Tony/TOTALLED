using System;
using Totalled;
using UnityEngine;
public static class CrashLabBroadphaseChecks
{
    public static void Run(Action<bool,string> check)
    {
        var s=new SoftStructure(null);var random=new System.Random(913);
        for(int i=0;i<102;i++){
            int id=s.AddNode(new Vector3((float)random.NextDouble()*8,(float)random.NextDouble()*3,(float)random.NextDouble()*8),24,i%7==0?.36f:.1f);
            s.nodes[id].previous=s.nodes[id].position-new Vector3(.3f,.1f,.2f);
        }
        var tree=new NodeBroadphase();tree.Refit(s);bool matches=true;
        for(int round=0;round<200;round++){
            int moved=round%s.nodes.Count;
            s.nodes[moved].position+=new Vector3(.17f,-.03f,.21f);tree.Update(moved);
            var query=new Bounds(new Vector3((float)random.NextDouble()*8,(float)random.NextDouble()*3,(float)random.NextDouble()*8),new Vector3(1.5f,.5f,2));
            tree.Query(query);
            for(int i=0;i<s.nodes.Count;i++){
                var n=s.nodes[i];var swept=new Bounds(n.position,Vector3.zero);swept.Encapsulate(n.previous);swept.Expand(2*(n.radius+.025f));
                if(tree.candidates.Contains(i)!=swept.Intersects(query))matches=false;
            }
        }
        check(matches,"Swept-node hierarchy matches exhaustive contact candidates after 200 incremental position updates.");
    }
}

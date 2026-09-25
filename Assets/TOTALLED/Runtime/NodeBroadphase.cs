using System.Collections.Generic;
using UnityEngine;
namespace Totalled
{
    // A refitted hierarchy of swept node spheres. Detached nodes retain their
    // own leaves, so distant debris cannot force every node through narrow phase.
    public sealed class NodeBroadphase
    {
        Bounds[] boxes;
        int[] leaves;
        int count;
        SoftStructure structure;
        public readonly List<int> candidates=new List<int>(128);
        public void Refit(SoftStructure value)
        {
            structure=value;
            if(count!=value.nodes.Count){count=value.nodes.Count;boxes=new Bounds[count*4];leaves=new int[count];}
            Build(1,0,count);
        }
        Bounds Node(int i)
        {
            var n=structure.nodes[i];var box=new Bounds(n.position,Vector3.zero);
            box.Encapsulate(n.previous);box.Expand((n.radius+.025f)*2);return box;
        }
        void Build(int slot,int start,int length)
        {
            if(length==1){leaves[start]=slot;boxes[slot]=Node(start);return;}
            int half=length/2;Build(slot*2,start,half);Build(slot*2+1,start+half,length-half);
            boxes[slot]=boxes[slot*2];boxes[slot].Encapsulate(boxes[slot*2+1]);
        }
        public void Update(int node)
        {
            int slot=leaves[node];boxes[slot]=Node(node);
            while(slot>1){slot/=2;boxes[slot]=boxes[slot*2];boxes[slot].Encapsulate(boxes[slot*2+1]);}
        }
        public void Query(Bounds box){candidates.Clear();Query(box,1,0,count);}
        void Query(Bounds box,int slot,int start,int length)
        {
            if(!boxes[slot].Intersects(box))return;
            if(length==1){candidates.Add(start);return;}
            int half=length/2;Query(box,slot*2,start,half);Query(box,slot*2+1,start+half,length-half);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;
namespace Totalled
{
    // Added physical cross sections make each end fold in two shorter stages.
    public static class CrumpleCage
    {
        static readonly float[] stations={-2.4f,-2,-1.6f,-1.1f,0,.95f,1.6f,2,2.4f};
        public static void Build(SacrificialSedan car)
        {
            var s=car.structure;
            foreach(int near in new[]{0,5})
            {
                int[] middle=new int[6];
                for(int i=0;i<6;i++)
                {
                    var a=s.nodes[near*6+i];var b=s.nodes[(near+1)*6+i];
                    // Redistribute existing mass, keeping total vehicle mass constant.
                    a.inverseMass=1f/18;b.inverseMass=1f/18;
                    middle[i]=s.AddNode((a.original+b.original)*.5f,12,.09f);
                }
                if(near==0)car.rearFold=middle;else car.frontFold=middle;
                for(int a=0;a<6;a++)for(int b=a+1;b<6;b++)
                    if(Mathf.Abs(a%3-b%3)<=1)Beam(s,middle[a],middle[b],.050f);
                for(int section=0;section<2;section++)for(int a=0;a<6;a++)for(int b=0;b<6;b++)
                {
                    if(Mathf.Abs(a%3-b%3)>1)continue;
                    int first=section==0?near*6+a:middle[a];
                    int second=section==0?middle[b]:(near+1)*6+b;
                    bool tip=near==0?section==0:section==1;
                    Beam(s,first,second,tip?.028f:.045f);
                }
                var refined=new List<int[]>();
                foreach(var q in car.shell)
                {
                    bool eligible=true;int min=9,max=-1;
                    foreach(int node in q){if(node>=42){eligible=false;break;}min=Mathf.Min(min,node/6);max=Mathf.Max(max,node/6);}
                    if(!eligible||min!=near||max!=near+1){refined.Add(q);continue;}
                    int m0=middle[q[0]%6],m1=middle[q[1]%6];
                    refined.Add(new[]{q[0],q[1],m1,m0});refined.Add(new[]{m0,m1,q[2],q[3]});
                }
                car.shell.Clear();car.shell.AddRange(refined);
            }
        }
        static void Beam(SoftStructure s,int a,int b,float yield)
        {
            // Yield calibration uses beam length; subdivision must not halve a
            // rail's force capacity merely because the segment became shorter.
            float length=Vector3.Distance(s.nodes[a].position,s.nodes[b].position);
            yield*=Mathf.Max(1,.8f/Mathf.Max(.3f,length));
            int id=s.AddBeam(a,b,2e-8f,yield,1.15f);s.beams[id].plasticRate=95;
        }
        public static Vector3 Skin(SacrificialSedan car,Vector3 p)
        {
            int z=0;while(z<7&&p.z>stations[z+1])z++;
            float u=(p.x+.83f)/.83f;int x=Mathf.Clamp(Mathf.FloorToInt(u),0,1);u-=x;
            float v=(p.y-.58f)/.46f,w=(p.z-stations[z])/(stations[z+1]-stations[z]);Vector3 result=Vector3.zero;
            for(int k=0;k<2;k++)for(int j=0;j<2;j++)for(int i=0;i<2;i++)
                result+=car.structure.nodes[Node(car,x+i,j,z+k)].position*(i==0?1-u:u)*(j==0?1-v:v)*(k==0?1-w:w);
            return result;
        }
        static int Node(SacrificialSedan car,int x,int y,int station)
        {
            if(station==1)return car.rearFold[y*3+x];if(station==7)return car.frontFold[y*3+x];
            int z=station==0?0:station==8?6:station-1;return SacrificialSedan.Index(x,y,z);
        }
    }
}

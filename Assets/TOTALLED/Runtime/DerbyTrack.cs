using UnityEngine;
namespace Totalled
{
    public static class DerbyTrack
    {
        public const int Gates=24;
        public const float HalfWidth=6.5f;
        public static Vector3 Point(float angle,float lane=0)=>new Vector3((30+lane)*Mathf.Cos(angle),0,(44+lane)*Mathf.Sin(angle));
        public static Vector3 Tangent(float angle)=>new Vector3(-30*Mathf.Sin(angle),0,44*Mathf.Cos(angle)).normalized;
        public static float Angle(Vector3 p)=>Mathf.Atan2(p.z/44,p.x/30);
        public static Quaternion Heading(float angle)=>Quaternion.LookRotation(Tangent(angle));
        public static void Build()
        {
            var asphalt=CrashLabArt.Surface(new Color(.17f,.18f,.18f),0,18);
            var grass=CrashLabArt.Surface(new Color(.26f,.29f,.18f),0,22);
            var concrete=CrashLabArt.Surface(new Color(.49f,.47f,.41f),1,4);
            var gold=SedanView.Material(new Color(.95f,.59f,.12f));
            var dark=SedanView.Material(new Color(.09f,.11f,.12f));
            var white=SedanView.Material(new Color(.85f,.83f,.73f));
            Box("Circuit foundation",new Vector3(0,-.35f,0),new Vector3(150,.7f,180),grass,true);
            // A continuous road mesh has no physical seams under the wheels.
            var mesh=new Mesh();var v=new Vector3[194];var uv=new Vector2[194];var tri=new int[576];
            for(int i=0;i<=96;i++){
                float a=i*Mathf.PI*2/96;
                v[i*2]=Point(a,-HalfWidth)+Vector3.up*.012f;v[i*2+1]=Point(a,HalfWidth)+Vector3.up*.012f;
                uv[i*2]=new Vector2(0,i*.15f);uv[i*2+1]=new Vector2(1,i*.15f);
                if(i<96){int t=i*6,n=i*2;tri[t]=n;tri[t+1]=n+2;tri[t+2]=n+1;tri[t+3]=n+1;tri[t+4]=n+2;tri[t+5]=n+3;}
            }
            mesh.vertices=v;mesh.uv=uv;mesh.triangles=tri;mesh.RecalculateNormals();
            var road=new GameObject("Motor Works oval");road.AddComponent<MeshFilter>().sharedMesh=mesh;road.AddComponent<MeshRenderer>().sharedMaterial=asphalt;
            for(int i=0;i<96;i++){
                float a=i*Mathf.PI*2/96,b=(i+1)*Mathf.PI*2/96;
                foreach(int side in new[]{-1,1}){
                    Vector3 p=Point(a,side*7.3f),q=Point(b,side*7.3f);
                    Box("Concrete race wall",(p+q)*.5f+Vector3.up*.6f,new Vector3(.55f,1.2f,Vector3.Distance(p,q)+.12f),i%8<2?gold:concrete,true,Quaternion.LookRotation(q-p));
                    if(i%2==0)Box("Track edge",(Point(a,side*6.2f)+Point(b,side*6.2f))*.5f+Vector3.up*.025f,new Vector3(.22f,.02f,Vector3.Distance(p,q)),white,false,Quaternion.LookRotation(q-p));
                }
                if(i%6==0)Box("Direction chevron",Point(a)+Vector3.up*.03f,new Vector3(.25f,.02f,1.6f),gold,false,Heading(a+ .4f));
            }
            for(int x=0;x<12;x++)for(int z=0;z<2;z++)
                Box("Chequered start",new Vector3(24+x,.045f,z-.5f),new Vector3(1,.035f,.5f),(x+z)%2==0?white:dark);
            Box("Start gantry left",new Vector3(21,3,0),new Vector3(.45f,6,.45f),dark);
            Box("Start gantry right",new Vector3(39,3,0),new Vector3(.45f,6,.45f),dark);
            Box("Start gantry",new Vector3(30,5.8f,0),new Vector3(18,1,.5f),dark);
            Label("TOTALLED  /  MOTOR WORKS",new Vector3(22,5.9f,-.3f),.35f);
            Label("BANGER CIRCUIT",new Vector3(-10,1.6f,0),.7f);
            for(int row=0;row<4;row++)Box("Grandstand",new Vector3(-48-row*2,1+row*.55f,0),new Vector3(2,1.2f,40),row%2==0?dark:concrete);
            for(int i=0;i<8;i++){
                Vector3 p=Point(i*Mathf.PI/4,17);
                Box("Floodlight mast",p+Vector3.up*5,new Vector3(.24f,10,.24f),dark);
                Box("Floodlight head",p+Vector3.up*10,new Vector3(2,.6f,.5f),white);
            }
            for(int i=0;i<5;i++)Box("Workshop",new Vector3(-30+i*15,4,75),new Vector3(12,8,10),concrete);
        }
        static void Box(string n,Vector3 p,Vector3 size,Material m,bool solid=false,Quaternion rotation=default)=>CrashLabArt.Box(n,p,size,m,solid,rotation);
        static void Label(string text,Vector3 p,float size){
            var go=new GameObject(text);go.transform.position=p;var t=go.AddComponent<TextMesh>();t.text=text;t.fontSize=64;t.characterSize=size*.24f;t.color=new Color(.95f,.72f,.3f);
            CrashLabArt.DepthText(t);
        }
    }
    public sealed class RaceProgress
    {
        public int Passed=-1;
        public int Lap=>Mathf.Max(0,Passed/DerbyTrack.Gates);
        public bool Finished=>Lap>=3;
        public Vector3 previous;
        public RaceProgress(Vector3 start){previous=start;}
        public bool Advance(Vector3 position)
        {
            if(Finished){previous=position;return false;}
            int next=(Passed+1)%DerbyTrack.Gates;
            float a=next*Mathf.PI*2/DerbyTrack.Gates;Vector3 gate=DerbyTrack.Point(a),forward=DerbyTrack.Tangent(a);
            float old=Vector3.Dot(previous-gate,forward),now=Vector3.Dot(position-gate,forward);
            bool crossed=false;
            if(old<0&&now>=0){
                Vector3 hit=Vector3.Lerp(previous,position,-old/(now-old));
                if(Vector3.Distance(new Vector3(hit.x,0,hit.z),gate)<DerbyTrack.HalfWidth+1&&Mathf.Abs(hit.y)<4){Passed++;crossed=true;}
            }
            previous=position;return crossed;
        }
        public float Order(Vector3 position)
        {
            float a=((Passed+1)%DerbyTrack.Gates)*Mathf.PI*2/DerbyTrack.Gates;
            return Passed-Mathf.Clamp(Vector3.Distance(position,DerbyTrack.Point(a))/15,0,.99f);
        }
    }
}

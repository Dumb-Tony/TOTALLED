using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    // Trim is bound to local node quads, so it follows the same deformed body.
    public sealed class SedanTrim
    {
        sealed class Patch {public int[] nodes;public Vector4 rect;public Mesh mesh;public GameObject go;public bool torn,glass;public float maxSpan,depth;public float[] edges;}
        readonly SacrificialSedan car;
        readonly List<Patch> patches=new List<Patch>();
        readonly Transform root;
        readonly Material brakeLens;
        public SedanTrim(SacrificialSedan car,Transform parent,Material paint)
        {
            this.car=car;root=parent;
            var chrome=SedanView.Material(new Color(.56f,.58f,.56f),.6f,.4f);
            var black=CrashLabArt.Surface(new Color(.07f,.08f,.09f),3);
            var glass=new Material(Resources.Load<Material>("TOTALLED/Glass"));
            var head=SedanView.Material(new Color(.87f,.81f,.59f),.1f,.4f);
            var lens=new Texture2D(64,32,TextureFormat.RGB24,true);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {float dx=(x-31.5f)/32,dy=(y-15.5f)/16;float value=Mathf.Clamp01(1.15f-Mathf.Sqrt(dx*dx+dy*dy)*.75f);value*=x%5==0?.65f:1;lens.SetPixel(x,y,new Color(value,value,value));}
            lens.Apply();head.mainTexture=lens;
            var amber=SedanView.Material(new Color(.67f,.29f,.035f),.1f,.3f);
            var tail=SedanView.Material(new Color(.50f,.065f,.025f),.1f,.35f);
            brakeLens=tail;
            int[] front={I(0,0,6),I(2,0,6),I(2,1,6),I(0,1,6)};
            int[] back={I(2,0,0),I(0,0,0),I(0,1,0),I(2,1,0)};
            foreach(var end in new[]{front,back})
            {
                Add(end,new Vector4(-.035f,.025f,1.035f,.23f),chrome,"Bumper");
                Add(end,new Vector4(.29f,.34f,.71f,.84f),black,"Grille / plate recess");
                if(end==front)for(int line=0;line<4;line++)Add(end,new Vector4(.32f,.39f+line*.10f,.68f,.41f+line*.10f),chrome,"Grille bar");
                else Add(end,new Vector4(.39f,.43f,.61f,.69f),Plate(),"License plate");
                Add(end,new Vector4(.025f,.39f,.275f,.93f),black,"Lamp recess");
                Add(end,new Vector4(.725f,.39f,.975f,.93f),black,"Lamp recess");
                Add(end,new Vector4(.04f,.43f,.26f,.90f),end==front?head:tail,"Lamp lens");
                Add(end,new Vector4(.74f,.43f,.96f,.90f),end==front?head:tail,"Lamp lens");
                Add(end,new Vector4(.04f,.43f,.085f,.90f),end==front?amber:head,"Lamp indicator");
                Add(end,new Vector4(.915f,.43f,.96f,.90f),end==front?amber:head,"Lamp indicator");
            }
            Frame(new[]{I(0,1,4),I(2,1,4),44,45},paint,black,glass,false);
            foreach(float x in new[]{.17f,.57f})Add(new[]{I(0,1,4),I(2,1,4),44,45},new Vector4(x,.11f,x+.25f,.128f),black,"Windshield wiper");
            var stamping=new Material(paint);stamping.color=new Color(.80f,.81f,.82f);
            for(int panel=0;panel<2;panel++)
            {
                for(int face=0;face<4;face++)
                {
                    var q=car.panels[panel].faces[face];float u=face%2==0?.52f:.46f;
                    Add(q,new Vector4(u,0,u+.014f,1),stamping,"Pressed bonnet ridge");
                    if(panel==0&&face<2)for(int vent=0;vent<4;vent++)
                        Add(q,new Vector4(.15f,.055f+vent*.024f,.85f,.068f+vent*.024f),black,"Cowl vent");
                }
            }
            Frame(new[]{I(2,1,2),I(0,1,2),42,43},paint,black,glass,false);
            for(int side=0;side<2;side++)
            {
                int x=side==0?0:2;
                int[] window={I(x,1,2),I(x,1,4),side==0?45:44,side==0?42:43};
                Frame(window,paint,black,glass,true);
                Add(window,new Vector4(.43f,.01f,.49f,1),paint,"Window divider");
                var door=car.panels[side+2];int[] q={door.nodes[0],door.nodes[6],door.nodes[8],door.nodes[2]};
                Add(q,new Vector4(.12f,.73f,.30f,.80f),chrome,"Door handle");
                Add(q,new Vector4(.02f,.12f,.98f,.18f),black,"Door rubbing strip");
                for(int z=2;z<4;z++)Add(new[]{I(x,0,z),I(x,0,z+1),I(x,1,z+1),I(x,1,z)},new Vector4(0,.08f,1,.18f),black,"Sill trim");
            }
        }
        void Frame(int[] q,Material paint,Material gasket,Material glass,bool side)
        {
            float left=side?.17f:.055f,right=side?.08f:.055f;
            Add(q,new Vector4(0,0,1,.085f),paint,"Painted window surround");
            Add(q,new Vector4(0,.93f,1,1.035f),paint,"Painted window surround");
            Add(q,new Vector4(0,0,left,1),paint,"Painted window surround");
            Add(q,new Vector4(1-right,0,1,1),paint,"Painted window surround");
            Add(q,new Vector4(left,.065f,1-right,.095f),gasket,"Glass seal");
            Add(q,new Vector4(left,.915f,1-right,.94f),gasket,"Glass seal");
            Add(q,new Vector4(left-.01f,.08f,left+.018f,.93f),gasket,"Glass seal");
            Add(q,new Vector4(1-right-.018f,.08f,1-right+.01f,.93f),gasket,"Glass seal");
            Add(q,new Vector4(left+.012f,.09f,1-right-.012f,.925f),glass,"Breakable glass");
        }
        static Material Plate()
        {
            var material=SedanView.Material(new Color(.78f,.75f,.62f),0,.15f);var texture=new Texture2D(128,32,TextureFormat.RGB24,false);
            for(int y=0;y<32;y++)for(int x=0;x<128;x++)texture.SetPixel(x,y,x<2||x>125||y<2||y>29?new Color(.15f,.17f,.16f):new Color(.92f,.91f,.85f));
            string[] glyphs={"11111001000010000100001000010000100","10000100001000010000100001000011111","00000000000000011111000000000000000","01110100011001110101110011000101110","01110100011001110101110011000101110","01110100011000101110100011000101110"};
            for(int letter=0;letter<glyphs.Length;letter++)for(int row=0;row<7;row++)for(int col=0;col<5;col++)
                if(glyphs[letter][row*5+col]=='1')for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++)texture.SetPixel(23+letter*14+col*2+dx,23-row*2+dy,new Color(.07f,.10f,.11f));
            texture.Apply();texture.filterMode=FilterMode.Bilinear;material.mainTexture=texture;return material;
        }
        static int I(int x,int y,int z)=>SacrificialSedan.Index(x,y,z);
        void Add(int[] nodes,Vector4 rect,Material mat,string name)
        {
            var go=new GameObject(name);go.layer=2;go.transform.SetParent(root,false);
            var mesh=new Mesh();mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
            float span=0;for(int i=0;i<4;i++)span+=Vector3.Distance(car.structure.nodes[nodes[i]].position,car.structure.nodes[nodes[(i+1)%4]].position);
            bool isGlass=name=="Breakable glass";
            if(isGlass)go.GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            float[] edges=new float[4];for(int i=0;i<4;i++)edges[i]=Vector3.Distance(SedanShape.Position(car,nodes[i]),SedanShape.Position(car,nodes[(i+1)%4]));
            patches.Add(new Patch{nodes=nodes,rect=rect,go=go,mesh=mesh,maxSpan=span*1.6f,glass=isGlass,edges=edges,depth=(name=="License plate"||name=="Grille bar"||name=="Window divider"||name=="Lamp indicator"||name=="Glass seal")?.027f:name=="Lamp recess"?.008f:name=="Pressed bonnet ridge"||name=="Cowl vent"?.048f:.014f});
        }
        public void Refresh(bool visible)
        {
            brakeLens.color=car.brake>.05f||car.throttle*car.Speed<-.8f?new Color(1,.13f,.035f):new Color(.38f,.025f,.015f);
            foreach(var p in patches)
            {
                var a=SedanShape.Position(car,p.nodes[0]);var b=SedanShape.Position(car,p.nodes[1]);var c=SedanShape.Position(car,p.nodes[2]);var d=SedanShape.Position(car,p.nodes[3]);
                if(Vector3.Distance(a,b)+Vector3.Distance(b,c)+Vector3.Distance(c,d)+Vector3.Distance(d,a)>p.maxSpan)p.torn=true;
                if(p.glass)
                {
                    Vector3[] corners={a,b,c,d};
                    for(int edge=0;edge<4;edge++)if(Mathf.Abs(Vector3.Distance(corners[edge],corners[(edge+1)%4])/p.edges[edge]-1)>.12f)p.torn=true;
                }
                p.go.SetActive(visible&&!p.torn);if(!visible||p.torn)continue;
                Vector3 normal=Vector3.Cross(b-a,d-a).normalized;
                if(Vector3.Dot(normal,(a+b+c+d)*.25f-car.Center)<0)normal=-normal;
                Vector3 offset=normal*p.depth;
                var r=p.rect;Vector3[] v={Point(a,b,c,d,r.x,r.y)+offset,Point(a,b,c,d,r.z,r.y)+offset,Point(a,b,c,d,r.z,r.w)+offset,Point(a,b,c,d,r.x,r.w)+offset};
                p.mesh.Clear();p.mesh.vertices=new[]{v[0],v[1],v[2],v[3],v[0],v[1],v[2],v[3]};p.mesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up,Vector2.zero,Vector2.right,Vector2.one,Vector2.up};p.mesh.triangles=new[]{0,1,2,0,2,3,6,5,4,7,6,4};p.mesh.RecalculateNormals();p.mesh.RecalculateBounds();
            }
        }
        static Vector3 Point(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float u,float v)=>Vector3.LerpUnclamped(Vector3.LerpUnclamped(a,b,u),Vector3.LerpUnclamped(d,c,u),v);
    }
}

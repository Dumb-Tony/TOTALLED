using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    // Trim is bound to local node quads, so it follows the same deformed body.
    public sealed class SedanTrim
    {
        sealed class Patch {public int[] nodes;public Vector4 rect;public Mesh mesh;public GameObject go;public bool torn;public float maxSpan,depth;}
        readonly SacrificialSedan car;
        readonly List<Patch> patches=new List<Patch>();
        readonly Transform root;
        public SedanTrim(SacrificialSedan car,Transform parent)
        {
            this.car=car;root=parent;
            var chrome=SedanView.Material(new Color(.56f,.58f,.56f),.6f,.4f);
            var black=CrashLabArt.Surface(new Color(.07f,.08f,.09f),3);
            var glass=SedanView.Material(new Color(.17f,.25f,.28f),.25f,.65f);
            var tint=new Texture2D(32,32,TextureFormat.RGB24,true);
            for(int y=0;y<32;y++)for(int x=0;x<32;x++)
            {float value=Mathf.Lerp(.45f,1.7f,y/31f);if(y>17&&y<20)value*=1.12f;tint.SetPixel(x,y,new Color(value*.78f,value*.9f,value));}
            tint.Apply();glass.mainTexture=tint;
            var head=SedanView.Material(new Color(.87f,.81f,.59f),.1f,.4f);
            var tail=SedanView.Material(new Color(.50f,.065f,.025f),.1f,.35f);
            int[] front={I(0,0,6),I(2,0,6),I(2,1,6),I(0,1,6)};
            int[] back={I(2,0,0),I(0,0,0),I(0,1,0),I(2,1,0)};
            foreach(var end in new[]{front,back})
            {
                Add(end,new Vector4(-.035f,.025f,1.035f,.23f),chrome,"Bumper");
                Add(end,new Vector4(.29f,.34f,.71f,.84f),black,"Grille / plate recess");
                if(end==front)for(int line=0;line<4;line++)Add(end,new Vector4(.32f,.39f+line*.10f,.68f,.41f+line*.10f),chrome,"Grille bar");
                else Add(end,new Vector4(.39f,.43f,.61f,.69f),Plate(),"License plate");
                Add(end,new Vector4(.04f,.43f,.26f,.90f),end==front?head:tail,"Lamp lens");
                Add(end,new Vector4(.74f,.43f,.96f,.90f),end==front?head:tail,"Lamp lens");
            }
            Add(new[]{I(0,1,4),I(2,1,4),44,45},new Vector4(.035f,.08f,.965f,.91f),glass,"Windshield");
            Add(new[]{I(2,1,2),I(0,1,2),42,43},new Vector4(.035f,.08f,.965f,.91f),glass,"Rear glass");
            for(int side=0;side<2;side++)
            {
                int x=side==0?0:2;
                int[] window={I(x,1,2),I(x,1,4),side==0?45:44,side==0?42:43};
                Add(window,new Vector4(.03f,.05f,.97f,.95f),glass,"Side windows");
                Add(window,new Vector4(.46f,.01f,.50f,1),black,"Window divider");
                var door=car.panels[side+2];int[] q={door.nodes[0],door.nodes[6],door.nodes[8],door.nodes[2]};
                Add(q,new Vector4(.12f,.73f,.30f,.80f),chrome,"Door handle");
                Add(q,new Vector4(.02f,.12f,.98f,.18f),black,"Door rubbing strip");
                for(int z=2;z<4;z++)Add(new[]{I(x,0,z),I(x,0,z+1),I(x,1,z+1),I(x,1,z)},new Vector4(0,.08f,1,.18f),black,"Sill trim");
            }
        }
        static Material Plate()
        {
            var material=SedanView.Material(new Color(.78f,.75f,.62f),0,.15f);var texture=new Texture2D(64,32,TextureFormat.RGB24,false);
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {bool border=x<3||x>60||y<3||y>28;bool letters=y>9&&y<23&&x>8&&x<55&&((x%9<2)||(y%11<2));texture.SetPixel(x,y,border||letters?new Color(.10f,.13f,.14f):Color.white);}
            texture.Apply();material.mainTexture=texture;return material;
        }
        static int I(int x,int y,int z)=>SacrificialSedan.Index(x,y,z);
        void Add(int[] nodes,Vector4 rect,Material mat,string name)
        {
            var go=new GameObject(name);go.layer=2;go.transform.SetParent(root,false);
            var mesh=new Mesh();mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=mat;
            float span=0;for(int i=0;i<4;i++)span+=Vector3.Distance(car.structure.nodes[nodes[i]].position,car.structure.nodes[nodes[(i+1)%4]].position);
            patches.Add(new Patch{nodes=nodes,rect=rect,go=go,mesh=mesh,maxSpan=span*1.6f,depth=(name=="License plate"||name=="Grille bar"||name=="Window divider")?.022f:.014f});
        }
        public void Refresh(bool visible)
        {
            foreach(var p in patches)
            {
                var a=car.structure.nodes[p.nodes[0]].position;var b=car.structure.nodes[p.nodes[1]].position;var c=car.structure.nodes[p.nodes[2]].position;var d=car.structure.nodes[p.nodes[3]].position;
                if(Vector3.Distance(a,b)+Vector3.Distance(b,c)+Vector3.Distance(c,d)+Vector3.Distance(d,a)>p.maxSpan)p.torn=true;
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



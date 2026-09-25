using UnityEngine;
using UnityEngine.Rendering;

namespace Totalled
{
    // Original, generated textures and set dressing; no external game assets.
    public static class CrashLabArt
    {
        public static readonly Vector3 TargetPosition=new Vector3(12,0,9);
        public static Material Surface(Color tint,int kind,float tiling=1)
        {
            var m=SedanView.Material(tint,kind==2?.25f:0,kind==2?.25f:.08f);
            const int size=128;var tex=new Texture2D(size,size,TextureFormat.RGB24,true);
            tex.name=new[]{"Asphalt aggregate","Weathered concrete","Oxidized enamel","Rubber tread","Aged brick"}[kind];
            var pixels=new Color[size*size];var random=new System.Random(730+kind);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float n=(float)random.NextDouble(),v=.72f+n*.28f;
                if(kind==0) {v=.65f+n*.35f;if((x+y*3)%61==0)v*=.5f;}
                if(kind==1) {v=.78f+n*.22f;if(y%32==0)v*=.7f;}
                if(kind==2) {v=.94f+n*.06f;if(Mathf.PerlinNoise(x*.045f,y*.045f)>.76f)v*=.76f;}
                if(kind==4){v=.68f+n*.32f;if(y%16<2||(x+(y/16%2)*16)%32<2)v=.40f;}
                if(kind==3)v=((x+y/4)%18<4)?.32f:.72f+n*.2f;
                pixels[y*size+x]=new Color(v,v,v);
            }
            tex.SetPixels(pixels);tex.Apply();tex.wrapMode=TextureWrapMode.Repeat;tex.anisoLevel=4;
            m.mainTexture=tex;m.mainTextureScale=Vector2.one*tiling;return m;
        }
        public static GameObject Box(string name,Vector3 pos,Vector3 size,Material mat,bool solid=false,Quaternion rotation=default)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=pos;g.transform.localScale=size;
            if(rotation!=default)g.transform.rotation=rotation;
            g.GetComponent<Renderer>().sharedMaterial=mat;
            if(name.Contains("skid"))g.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            if(!solid){g.layer=2;g.GetComponent<Collider>().enabled=false;}return g;
        }
        public static void Sign(string text,Vector3 position,float size,Color color,Quaternion rotation=default)
        {
            var go=new GameObject(text);go.transform.position=position;if(rotation!=default)go.transform.rotation=rotation;
            var t=go.AddComponent<TextMesh>();t.text=text;t.characterSize=size;t.fontSize=64;t.anchor=TextAnchor.MiddleCenter;t.color=color;
            DepthText(t);
        }
        public static void DepthText(TextMesh text)
        {
            var r=text.GetComponent<MeshRenderer>();var m=new Material(Resources.Load<Material>("TOTALLED/YardText"));
            m.mainTexture=r.sharedMaterial.mainTexture;r.sharedMaterial=m;
        }
        public static void Dress(Camera camera)
        {
            YardReflections.Apply();
            QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;
            QualitySettings.shadowDistance=55;QualitySettings.shadowCascades=2;QualitySettings.antiAliasing=2;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.50f,.58f,.66f);
            RenderSettings.ambientEquatorColor=new Color(.38f,.36f,.34f);RenderSettings.ambientGroundColor=new Color(.20f,.18f,.16f);
            RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=65;RenderSettings.fogEndDistance=180;
            RenderSettings.fogColor=new Color(.65f,.62f,.57f);camera.backgroundColor=RenderSettings.fogColor;
            RenderSettings.skybox=Resources.Load<Material>("TOTALLED/YardSky");camera.clearFlags=CameraClearFlags.Skybox;
            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            {light.color=new Color(1,.82f,.62f);light.intensity=1.1f;light.shadowStrength=.85f;light.shadowBias=.08f;light.shadowNormalBias=.15f;light.transform.rotation=Quaternion.Euler(32,-32,0);}
            var asphalt=Surface(new Color(.23f,.24f,.25f),0,24);
            var concrete=Surface(new Color(.56f,.54f,.49f),1,3);
            var yellow=Surface(new Color(.8f,.55f,.16f),2);
            var dark=Surface(new Color(.18f,.20f,.21f),2);
            var pale=Surface(new Color(.65f,.62f,.49f),1);
            foreach(var text in Object.FindObjectsByType<TextMesh>(FindObjectsSortMode.None))
            {if(text.text=="TOTALLED / CRASH LAB")text.gameObject.SetActive(false);else DepthText(text);}
            foreach(var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
            {
                if(r.name=="Test slab")r.sharedMaterial=asphalt;
                if(r.name.Contains("wall")||r.name=="Ramp"||r.name=="Landing block")r.sharedMaterial=concrete;
                if(r.name=="Survey line")r.gameObject.SetActive(false);
                if(r.name=="Impact wall marker")r.sharedMaterial=yellow;
                if(r.name=="Offset barrier"||r.name=="Narrow pole")r.sharedMaterial=yellow;
            }
            // Lane paint, a dedicated target bay, and worn skid marks.
            for(int z=-20;z<21;z+=4)Box("Faded lane dash",new Vector3(-3,.012f,z),new Vector3(.10f,.008f,1.8f),pale);
            for(int x=8;x<=16;x+=8)Box("Target lane",new Vector3(x,.014f,7),new Vector3(.12f,.008f,15),yellow);
            for(int z=5;z<=13;z+=8)Box("Target bay",new Vector3(12,.015f,z),new Vector3(8,.008f,.12f),yellow);
            Sign("05 / PARKED TARGET",new Vector3(12,.022f,1),.15f,new Color(.8f,.7f,.4f),Quaternion.Euler(90,0,0));
            var rubber=Surface(new Color(.09f,.085f,.075f),0,2);
            for(int i=0;i<18;i++)
            {
                float a=i*.1f;
                for(int side=-1;side<=1;side+=2)Box("Old skid marks",new Vector3(-2+Mathf.Sin(a)*(6+side*.72f),.010f,-9+Mathf.Cos(a)*(6+side*.72f)),new Vector3(.19f,.004f,.76f),rubber,false,Quaternion.Euler(0,90+i*5.7f,0));
            }
            // Low-poly industrial skyline beyond the collision course.
            var brick=Surface(new Color(.36f,.28f,.23f),4,4);var window=SedanView.Material(new Color(.23f,.31f,.33f),.25f,.4f);
            for(int i=0;i<18;i++)
            {
                float x=-48+(i%9)*12,h=7+(i%3)*3,z=i<9?46:-46;
                Box("Dock warehouse",new Vector3(x,h/2,z),new Vector3(10,h,12),i%2==0?brick:concrete);
                Box("Roof coping",new Vector3(x,h+.2f,z),new Vector3(10.5f,.4f,12.5f),dark);
                for(int w=-3;w<=3;w+=3)for(int floor=3;floor<h;floor+=3)
                    Box("Warehouse window",new Vector3(x+w,floor,z-Mathf.Sign(z)*6.04f),new Vector3(1.4f,1.5f,.08f),window);
            }
            for(int i=-3;i<=3;i++)
            {
                float x=i*12;
                Box("Roll-up loading door",new Vector3(x,1.8f,39.90f),new Vector3(4,3.6f,.10f),dark);
                for(int slat=1;slat<12;slat++)Box("Shutter rib",new Vector3(x,slat*.29f,39.82f),new Vector3(3.9f,.045f,.04f),concrete);
                Box("Loading door lintel",new Vector3(x,3.7f,39.7f),new Vector3(4.5f,.22f,.32f),concrete);
            }
            for(int i=0;i<8;i++)
            {
                float z=-23+i*6;
                Box("Fence post",new Vector3(30.5f,1.2f,z),new Vector3(.08f,2.4f,.08f),dark);
                Box("Fence rail",new Vector3(30.5f,2.2f,z+3),new Vector3(.04f,.04f,6),dark);
                Box("Fence rail",new Vector3(30.5f,.4f,z+3),new Vector3(.04f,.04f,6),dark);
                for(int bar=0;bar<6;bar++)Box("Fence wire",new Vector3(30.5f,1.2f,z+bar),new Vector3(.015f,2,.015f),dark);
            }
            for(int i=0;i<4;i++)
            {
                var color=i%2==0?Surface(new Color(.25f,.38f,.37f),2):Surface(new Color(.52f,.28f,.17f),2);
                Vector3 p=new Vector3(-29,1.4f,-15+i*7);
                Box("Shipping container",p,new Vector3(3,2.8f,6),color,true);
                for(int z=-2;z<=2;z++)Box("Container rib",p+new Vector3(1.53f,0,z),new Vector3(.06f,2.65f,.09f),dark);
            }
            for(int i=-1;i<=1;i+=2)
            {
                Box("Yard light mast",new Vector3(i*28,5,22),new Vector3(.18f,10,.18f),dark);
                Box("Floodlight head",new Vector3(i*28,9.8f,21.7f),new Vector3(1.2f,.35f,.6f),pale);
            }
            Box("TOTALLED sign backing",new Vector3(-9,5,24),new Vector3(19,2.4f,.2f),dark);
            Sign("TOTALLED  /  MOTOR WORKS",new Vector3(-9,5,23.85f),.14f,new Color(.94f,.74f,.40f));
        }
    }
}

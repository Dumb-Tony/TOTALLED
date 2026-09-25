using UnityEngine;
using UnityEngine.Rendering;
namespace Totalled
{
    // A small original sky/yard approximation supplies stable material reflections
    // without six real-time scene renders every frame.
    public static class YardReflections
    {
        public static void Apply()
        {
            const int size=64;var cube=new Cubemap(size,TextureFormat.RGB24,true);cube.name="Crash Lab sky and yard reflection";
            for(int face=0;face<6;face++)
            {
                var pixels=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float u=(x+.5f)/size*2-1,v=(y+.5f)/size*2-1;Vector3 d;
                    switch(face){case 0:d=new Vector3(1,-v,-u);break;case 1:d=new Vector3(-1,-v,u);break;case 2:d=new Vector3(u,1,v);break;case 3:d=new Vector3(u,-1,-v);break;case 4:d=new Vector3(u,-v,1);break;default:d=new Vector3(-u,-v,-1);break;}
                    d.Normalize();Color c=d.y<0?Color.Lerp(new Color(.11f,.12f,.13f),new Color(.29f,.29f,.28f),d.y+1):Color.Lerp(new Color(.65f,.66f,.64f),new Color(.30f,.43f,.60f),Mathf.Sqrt(d.y));
                    float angle=Mathf.Atan2(d.z,d.x),building=.10f+.07f*Mathf.Sin(Mathf.Floor(angle*5)*2.7f);
                    if(d.y>0&&d.y<building)c*=.40f;
                    float cloud=Mathf.Pow(Mathf.Max(0,Vector3.Dot(d,new Vector3(-.4f,.8f,.3f).normalized)),18);
                    pixels[y*size+x]=Color.Lerp(c,new Color(.92f,.87f,.76f),cloud*.75f);
                }
                cube.SetPixels(pixels,(CubemapFace)face);
            }
            cube.Apply();RenderSettings.defaultReflectionMode=DefaultReflectionMode.Custom;RenderSettings.customReflection=cube;RenderSettings.reflectionIntensity=.8f;
        }
    }
}

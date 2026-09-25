using UnityEngine;

namespace Totalled
{
    // Small styling offsets ride on the physical nodes; they never alter the solver.
    public static class SedanShape
    {
        public static Vector3 Position(SacrificialSedan car,int index)
        {
            var node=car.structure.nodes[index];
            Vector3 origin=car.structure.nodes[19].original-new Vector3(0,.58f,0);
            Vector3 local=node.original-origin,offset=Vector3.zero;
            if(index>=42&&index<46)
                offset=new Vector3(0,-.12f,local.z>0?-.24f:.16f);
            else if(index<86)
            {
                float end=Mathf.Clamp01((Mathf.Abs(local.z)-.9f)/1.5f);
                offset=new Vector3(-Mathf.Sign(local.x)*.065f*end,-.10f*end*Mathf.Clamp01((local.y-.58f)/.46f),0);
            }
            return node.position+car.Right*offset.x+car.Up*offset.y+car.Forward*offset.z;
        }
        public static Material Paint(Color color)
        {
            var material=SedanView.Material(Color.white,.2f,.28f);
            const int size=256;var tex=new Texture2D(size,size,TextureFormat.RGB24,true);var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=x/(float)(size-1),v=y/(float)(size-1);
                float grain=Mathf.PerlinNoise(x*.65f,y*.65f),patch=Mathf.PerlinNoise(x*.018f+3,y*.021f+12);
                float edge=Mathf.Min(Mathf.Min(u,1-u),Mathf.Min(v,1-v));
                float rust=Mathf.Clamp01((patch-(.78f-Mathf.Exp(-v*12)*.32f-Mathf.Exp(-edge*55)*.08f))*15);
                Color enamel=color*Mathf.Lerp(.76f,1.08f,grain);
                enamel*=Mathf.Lerp(.65f,1,Mathf.Clamp01(edge*55));
                pixels[y*size+x]=Color.Lerp(enamel,new Color(.19f,.095f,.035f)*Mathf.Lerp(.65f,1.4f,grain),rust);
            }
            tex.SetPixels(pixels);tex.Apply();tex.anisoLevel=4;tex.wrapMode=TextureWrapMode.Clamp;material.mainTexture=tex;return material;
        }
    }
}


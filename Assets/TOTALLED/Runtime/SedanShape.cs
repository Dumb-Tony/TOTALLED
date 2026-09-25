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
                offset=new Vector3(0,-.16f,local.z>0?-.12f:.02f);
            else if(index<86||car.IsBodyNode(index))
            {
                float end=Mathf.Clamp01((Mathf.Abs(local.z)-.9f)/1.5f);
                offset=new Vector3(Mathf.Sign(local.x)*(.11f-.075f*end),-.12f*end*Mathf.Clamp01((local.y-.58f)/.46f),-Mathf.Sign(local.z)*Mathf.Clamp01((Mathf.Abs(local.z)-2.1f)/.3f)*Mathf.Abs(local.x)*.12f);
            }
            return node.position+car.Right*offset.x+car.Up*offset.y+car.Forward*offset.z;
        }
        public static Material Paint(Color color)
        {
            var material=SedanView.Material(Color.white,.38f,.48f);
            const int size=256;var tex=new Texture2D(size,size,TextureFormat.RGB24,true);var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float u=x/(float)(size-1),v=y/(float)(size-1);
                float grain=Mathf.PerlinNoise(x*.65f,y*.65f),patch=Mathf.PerlinNoise(x*.018f+3,y*.021f+12);
                float edge=Mathf.Min(Mathf.Min(u,1-u),Mathf.Min(v,1-v));
                float rust=Mathf.Clamp01((patch-.69f)*12)*Mathf.Exp(-edge*95)*Mathf.Clamp01((grain-.35f)*4);
                Color enamel=color*Mathf.Lerp(.94f,1.035f,grain);
                enamel*=Mathf.Lerp(.65f,1,Mathf.Clamp01(edge*55));
                pixels[y*size+x]=Color.Lerp(enamel,new Color(.19f,.095f,.035f)*Mathf.Lerp(.65f,1.4f,grain),rust);
            }
            tex.SetPixels(pixels);tex.Apply();tex.anisoLevel=4;tex.wrapMode=TextureWrapMode.Clamp;material.mainTexture=tex;return material;
        }
    }
}

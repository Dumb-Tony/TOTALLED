using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    public static class SedanWheelMesh
    {
        public static Mesh Rim()
        {
            const int steps=32;
            float[] heights={-.66f,-.66f,-.82f,-.88f,-.88f,.88f,.88f,.82f,.66f,.66f};
            float[] radii={0,.32f,.40f,.46f,.50f,.50f,.46f,.40f,.32f,0};
            var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int ring=0;ring<heights.Length;ring++)for(int segment=0;segment<=steps;segment++)
            {float angle=segment*Mathf.PI*2/steps;vertices.Add(new Vector3(Mathf.Sin(angle)*radii[ring],heights[ring],Mathf.Cos(angle)*radii[ring]));}
            for(int ring=0;ring<heights.Length-1;ring++)for(int segment=0;segment<steps;segment++)
            {int a=ring*(steps+1)+segment,b=a+1,c=b+steps+1,d=a+steps+1;triangles.AddRange(new[]{a,b,c,a,c,d});}
            var mesh=new Mesh{name="Recessed steel rim and rolled bead"};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        public static Mesh Tire()
        {
            const int steps=32;
            float[] height={-1,-.89f,-.65f,.65f,.89f,1};float[] radius={.34f,.46f,.5f,.5f,.46f,.34f};
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var tread=new List<int>();var wall=new List<int>();
            for(int ring=0;ring<height.Length;ring++)for(int segment=0;segment<=steps;segment++)
            {
                float angle=segment*Mathf.PI*2/steps;
                vertices.Add(new Vector3(Mathf.Sin(angle)*radius[ring],height[ring],Mathf.Cos(angle)*radius[ring]));uv.Add(new Vector2(segment/(float)steps*4,ring/(float)(height.Length-1)));
            }
            for(int ring=0;ring<height.Length-1;ring++)for(int segment=0;segment<steps;segment++)
            {
                int a=ring*(steps+1)+segment,b=a+1,c=b+steps+1,d=a+steps+1;
                var indices=ring==2?tread:wall;indices.AddRange(new[]{a,b,c,a,c,d});
            }
            var mesh=new Mesh{name="Rounded tire with tread and sidewall"};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.subMeshCount=2;mesh.SetTriangles(tread,0);mesh.SetTriangles(wall,1);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
    }
}

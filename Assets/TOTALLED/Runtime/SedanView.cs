using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Totalled
{
    public sealed class SedanView
    {
        public readonly GameObject root;
        readonly SacrificialSedan car;
        readonly Mesh shellMesh, debugMesh;
        readonly MeshRenderer shellRenderer, debugRenderer;
        readonly List<Mesh> panelMeshes = new List<Mesh>();
        readonly List<MeshRenderer> panelRenderers = new List<MeshRenderer>();
        readonly List<Transform> wheelObjects = new List<Transform>();
        readonly List<Transform> rimObjects = new List<Transform>();
        readonly List<Transform> nodeObjects = new List<Transform>();
        readonly List<Transform> componentObjects = new List<Transform>();
        readonly List<Transform> pillars = new List<Transform>();
        readonly Material paint, rubber, steel, nodeMat, componentMat;
        public bool bodyVisible = true, debugVisible, componentsVisible;
        public int debugMode;
        public static Material Material(Color color, float metallic=0, float smoothness=.3f)
        {
            var m=new Material(Shader.Find("Standard")); m.color=color; m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",smoothness); return m;
        }
        public SedanView(SacrificialSedan car)
        {
            this.car=car;root=new GameObject("Sacrificial Sedan • deformable specimen");root.layer=2;
            paint=Material(new Color(.88f,.52f,.08f),.55f);rubber=Material(new Color(.045f,.052f,.061f));steel=Material(new Color(.28f,.32f,.34f),.8f);
            nodeMat=Material(new Color(.2f,1,.83f));componentMat=Material(new Color(.15f,.65f,.85f));
            shellMesh=NewMesh("Node skinned body",paint,out shellRenderer);
            foreach(var p in car.panels) { panelMeshes.Add(NewMesh(p.name,paint,out MeshRenderer r)); panelRenderers.Add(r); }
            var lineMaterial=new Material(Shader.Find("Hidden/Internal-Colored"));
            lineMaterial.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);lineMaterial.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull",(int)CullMode.Off);lineMaterial.SetInt("_ZWrite",0);
            debugMesh=NewMesh("Structural instrumentation",lineMaterial,out debugRenderer);
            foreach(var w in car.wheels)
            {
                var tire=Primitive(PrimitiveType.Cylinder,w.name+" tire",rubber);wheelObjects.Add(tire);
                var rim=Primitive(PrimitiveType.Cylinder,w.name+" rim",steel);rimObjects.Add(rim);
            }
            foreach(var n in car.structure.nodes) nodeObjects.Add(Primitive(PrimitiveType.Sphere,"Mass node",nodeMat));
            foreach(var p in car.parts) componentObjects.Add(Primitive(PrimitiveType.Cube,p.name,componentMat));
            for(int i=0;i<4;i++) pillars.Add(Primitive(PrimitiveType.Cylinder,"Cabin pillar",steel));
            Refresh();
        }
        Mesh NewMesh(string name, Material mat, out MeshRenderer renderer)
        {
            var g=new GameObject(name);g.layer=2;g.transform.SetParent(root.transform);
            var mesh=new Mesh {name=name};mesh.MarkDynamic();g.AddComponent<MeshFilter>().sharedMesh=mesh;
            renderer=g.AddComponent<MeshRenderer>();renderer.sharedMaterial=mat;return mesh;
        }
        Transform Primitive(PrimitiveType type,string name,Material mat)
        {
            var g=GameObject.CreatePrimitive(type);g.name=name;g.layer=2;g.transform.SetParent(root.transform);
            g.GetComponent<Collider>().enabled=false;
            g.GetComponent<MeshRenderer>().sharedMaterial=mat;return g.transform;
        }
        void Faces(Mesh mesh,IEnumerable<int[]> faces)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>();
            foreach(var q in faces)
            {
                int v=vertices.Count;
                foreach(int n in q) vertices.Add(car.structure.nodes[n].position);
                triangles.AddRange(new[]{v,v+1,v+2,v,v+2,v+3});
                // Reverse faces need separate vertices so their normals do not cancel.
                foreach(int n in q) vertices.Add(car.structure.nodes[n].position);
                triangles.AddRange(new[]{v+6,v+5,v+4,v+7,v+6,v+4});
            }
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        public void Refresh()
        {
            Faces(shellMesh,car.shell);shellRenderer.enabled=bodyVisible;
            for(int i=0;i<car.panels.Count;i++) { Faces(panelMeshes[i],new[]{car.panels[i].nodes});panelRenderers[i].enabled=bodyVisible; }
            for(int i=0;i<car.wheels.Count;i++)
            {
                var w=car.wheels[i];var n=car.structure.nodes[w.hub];
                Vector3 axle=Vector3.Cross(w.up.sqrMagnitude>.1f?w.up:car.Up,w.forward.sqrMagnitude>.1f?w.forward:car.Forward).normalized;
                Quaternion rotation=Quaternion.FromToRotation(Vector3.up,axle);
                wheelObjects[i].SetPositionAndRotation(n.position,rotation);wheelObjects[i].localScale=new Vector3(w.radius*2,.13f,w.radius*2);
                wheelObjects[i].gameObject.SetActive(!w.punctured);
                rimObjects[i].SetPositionAndRotation(n.position,rotation*Quaternion.AngleAxis(Mathf.Sin(w.spin)*w.rimBend*25,Vector3.right));
                rimObjects[i].localScale=new Vector3(.49f,.145f,.49f);
            }
            for(int i=0;i<nodeObjects.Count;i++)
            { nodeObjects[i].position=car.structure.nodes[i].position;nodeObjects[i].localScale=Vector3.one*.065f;nodeObjects[i].gameObject.SetActive(debugVisible); }
            for(int i=0;i<componentObjects.Count;i++)
            {
                componentObjects[i].position=car.parts[i].Position(car.structure);componentObjects[i].rotation=car.Orientation;
                componentObjects[i].localScale=new Vector3(.45f,.25f,.35f);componentObjects[i].gameObject.SetActive(componentsVisible);
            }
            int[] anchors={SacrificialSedan.Index(0,1,2),SacrificialSedan.Index(2,1,2),SacrificialSedan.Index(2,1,4),SacrificialSedan.Index(0,1,4)};
            for(int i=0;i<4;i++)
            {
                Vector3 a=car.structure.nodes[42+i].position,b=car.structure.nodes[anchors[i]].position;
                pillars[i].position=(a+b)*.5f;pillars[i].rotation=Quaternion.FromToRotation(Vector3.up,a-b);pillars[i].localScale=new Vector3(.065f,(a-b).magnitude*.5f,.065f);pillars[i].gameObject.SetActive(bodyVisible);
            }
            debugRenderer.enabled=debugVisible;
            if(!debugVisible) return;
            var verts=new List<Vector3>();var colors=new List<Color>();var indices=new List<int>();
            foreach(var b in car.structure.beams)
            {
                Color c=b.broken?new Color(1,.15f,.25f):debugMode==1?Color.Lerp(new Color(.1f,.4f,.9f),Color.yellow,Mathf.Clamp01(b.plastic/b.initial*4)):
                    Color.Lerp(new Color(.1f,.9f,.65f),Color.red,Mathf.Clamp01(b.stress));
                if(debugMode==2&&!b.broken) c.a=.12f;
                Line(verts,colors,indices,car.structure.nodes[b.a].position,car.structure.nodes[b.b].position,c);
            }
            foreach(var sample in car.structure.contacts)
                Line(verts,colors,indices,sample.point,sample.point+Vector3.ClampMagnitude(sample.impulse*.002f,2),new Color(1,.2f,.7f,1-sample.age));
            debugMesh.Clear();debugMesh.SetVertices(verts);debugMesh.SetColors(colors);debugMesh.SetIndices(indices,MeshTopology.Lines,0);debugMesh.RecalculateBounds();
        }
        static void Line(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Color color)
        { t.Add(v.Count);v.Add(a);c.Add(color);t.Add(v.Count);v.Add(b);c.Add(color); }
    }
}

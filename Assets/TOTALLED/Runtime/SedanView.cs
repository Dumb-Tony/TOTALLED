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
        readonly Material paint, creased, rubber, steel, nodeMat, componentMat;
        readonly float[] nodePlastic, nodeSpan;
        readonly Vector3[] originalPositions;
        readonly HashSet<int[]> tornFaces = new HashSet<int[]>();
        readonly List<int> insideFaces=new List<int>();
        bool reverseFace;
        readonly SedanTrim trim;
        readonly SedanInterior interior;
        public bool bodyVisible = true, debugVisible, componentsVisible;
        public int debugMode;
        public static Material Material(Color color, float metallic=0, float smoothness=.3f)
        {
            var template=Resources.Load<Material>("TOTALLED/Surface");
            var m=template!=null?new Material(template):new Material(Shader.Find("Standard"));
            m.color=color; m.SetFloat("_Metallic",metallic);m.SetFloat("_Glossiness",smoothness); return m;
        }
        public SedanView(SacrificialSedan car, Color? color=null)
        {
            this.car=car;root=new GameObject("Sacrificial Sedan • deformable specimen");root.layer=2;
            nodePlastic=new float[car.structure.nodes.Count];nodeSpan=new float[nodePlastic.Length];
            originalPositions=new Vector3[nodePlastic.Length];
            for(int i=0;i<originalPositions.Length;i++) originalPositions[i]=car.structure.nodes[i].position;
            paint=SedanShape.Paint(color??new Color(.43f,.14f,.095f));rubber=CrashLabArt.Surface(new Color(.10f,.105f,.11f),3);steel=Material(new Color(.42f,.44f,.43f),.6f);
            creased=new Material(paint);creased.color=new Color(.73f,.76f,.78f);creased.SetFloat("_Glossiness",.14f);creased.SetFloat("_Metallic",.12f);
            nodeMat=Material(new Color(.2f,1,.83f));componentMat=Material(new Color(.15f,.65f,.85f));
            shellMesh=NewMesh("Node skinned body",paint,out shellRenderer);
            var undercoat=Material(new Color(.075f,.08f,.075f),.15f,.12f);
            shellRenderer.sharedMaterials=new[]{paint,creased,undercoat};
            foreach(var p in car.panels) { panelMeshes.Add(NewMesh(p.name,paint,out MeshRenderer r));r.sharedMaterials=new[]{paint,creased,undercoat}; panelRenderers.Add(r); }
            var lineTemplate=Resources.Load<Material>("TOTALLED/DebugLines");
            var lineMaterial=lineTemplate!=null?new Material(lineTemplate):new Material(Shader.Find("Hidden/Internal-Colored"));
            lineMaterial.SetInt("_SrcBlend",(int)BlendMode.SrcAlpha);lineMaterial.SetInt("_DstBlend",(int)BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull",(int)CullMode.Off);lineMaterial.SetInt("_ZWrite",0);
            debugMesh=NewMesh("Structural instrumentation",lineMaterial,out debugRenderer);
            foreach(var w in car.wheels)
            {
                var tire=Primitive(PrimitiveType.Cylinder,w.name+" tire",rubber);wheelObjects.Add(tire);
                tire.GetComponent<MeshFilter>().sharedMesh=SedanWheelMesh.Tire();
                tire.GetComponent<MeshRenderer>().sharedMaterials=new[]{rubber,Material(new Color(.045f,.049f,.05f),0,.16f)};
                var rim=Primitive(PrimitiveType.Cylinder,w.name+" rim",steel);rimObjects.Add(rim);
                rim.GetComponent<MeshFilter>().sharedMesh=SedanWheelMesh.Rim();
                foreach(float side in new[]{-.69f,.69f})for(int spoke=0;spoke<6;spoke++)
                {
                    var slot=Primitive(PrimitiveType.Cube,"Wheel ventilation slot",rubber);slot.SetParent(rim,false);
                    float angle=spoke*Mathf.PI/3;
                    slot.localPosition=new Vector3(Mathf.Sin(angle)*.26f,side,Mathf.Cos(angle)*.26f);
                    slot.localRotation=Quaternion.Euler(0,spoke*60,0);slot.localScale=new Vector3(.12f,.025f,.10f);
                }
                foreach(float side in new[]{-.72f,.72f})for(int bolt=0;bolt<5;bolt++)
                {
                    var lug=Primitive(PrimitiveType.Cylinder,"Wheel lug",steel);lug.SetParent(rim,false);
                    float angle=bolt*Mathf.PI*2/5;lug.localPosition=new Vector3(Mathf.Sin(angle)*.16f,side,Mathf.Cos(angle)*.16f);lug.localScale=new Vector3(.055f,.04f,.055f);
                }
            }
            foreach(var n in car.structure.nodes) nodeObjects.Add(Primitive(PrimitiveType.Sphere,"Mass node",nodeMat));
            foreach(var p in car.parts) componentObjects.Add(Primitive(PrimitiveType.Cube,p.name,componentMat));
            trim=new SedanTrim(car,root.transform,paint);interior=new SedanInterior(car,root.transform);
            Refresh();
            if(Application.isPlaying)root.AddComponent<VehicleFeedback>().Initialize(car);
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
            insideFaces.Clear();
            var vertices=new List<Vector3>();var uv=new List<Vector2>();var intact=new List<int>();var damaged=new List<int>();
            foreach(var q in faces)
            {
                // A skin patch cannot bridge structure that has separated by metres.
                // Keep the tear permanent, just like the failed structural connection.
                if(!tornFaces.Contains(q))
                    for(int edge=0;edge<q.Length;edge++)
                    {
                        int a=q[edge],b=q[(edge+1)%q.Length];
                        float original=Vector3.Distance(originalPositions[a],originalPositions[b]);
                        if(Vector3.Distance(car.structure.nodes[a].position,car.structure.nodes[b].position)>original*1.8f)
                        {tornFaces.Add(q);break;}
                    }
                if(tornFaces.Contains(q))continue;
                float strain=0;foreach(int n in q)strain+=nodePlastic[n];
                var triangles=strain/q.Length>.02f?damaged:intact;
                var n0=car.structure.nodes[q[0]];var n1=car.structure.nodes[q[1]];var n2=car.structure.nodes[q[2]];var n3=car.structure.nodes[q[3]];
                Vector3 originalNormal=Vector3.Cross(n1.original-n0.original,n3.original-n0.original);
                reverseFace=Vector3.Dot(originalNormal,(n0.original+n1.original+n2.original+n3.original)*.25f-(car.structure.nodes[19].original+Vector3.up*.22f))<0;
                Vector3 p0=SedanShape.Position(car,q[0]),p1=SedanShape.Position(car,q[1]),p2=SedanShape.Position(car,q[2]),p3=SedanShape.Position(car,q[3]);
                bool flank=q[0]<42&&q[1]<42&&q[2]<42&&q[3]<42&&Mathf.Abs(n0.original.x)>.8f&&Mathf.Abs(n0.original.x-n2.original.x)<.01f&&n1.original.y>n0.original.y;
                if(flank)
                {
                    float localZ=(n0.original.z+n3.original.z)*.5f-(car.structure.nodes[19].original.z);
                    // Door openings reveal the actual cabin when a door comes off.
                    if(Mathf.Abs(localZ)<.79f)continue;
                    // Visual wheel openings interpolate the existing deformable skin.
                    // Collision graph and handling geometry are unchanged.
                    for(int segment=0;segment<8;segment++)
                    {
                        float u=segment/8f,v=(segment+1)/8f;
                        float originZ=car.structure.nodes[19].original.z;
                        float lo=Arch(Mathf.Lerp(n0.original.z,n3.original.z,u)-originZ),hi=Arch(Mathf.Lerp(n0.original.z,n3.original.z,v)-originZ);
                        var bottom0=Vector3.Lerp(p0,p3,u);var bottom1=Vector3.Lerp(p0,p3,v);
                        var top0=Vector3.Lerp(p1,p2,u);var top1=Vector3.Lerp(p1,p2,v);
                                                for(int band=0;band<3;band++)
                        {
                            float a=Mathf.Lerp(lo,1,band/3f),b=Mathf.Lerp(lo,1,(band+1)/3f),c=Mathf.Lerp(hi,1,(band+1)/3f),d=Mathf.Lerp(hi,1,band/3f);
                            Vector3 right=car.Right*Mathf.Sign((n0.original-car.structure.nodes[19].original).x);
                            Vector2 base0=SurfaceUV(n0.original,n0.original,n2.original),base1=SurfaceUV(n3.original,n0.original,n2.original);
                            float z0=Mathf.Lerp(base0.x,base1.x,u),z1=Mathf.Lerp(base0.x,base1.x,v);
                            Emit(vertices,uv,triangles,Side(bottom0,top0,right,a),Side(bottom0,top0,right,b),Side(bottom1,top1,right,c),Side(bottom1,top1,right,d),new[]{new Vector2(z0,a),new Vector2(z0,b),new Vector2(z1,c),new Vector2(z1,d)});
                        }
                    }
                }
                else
                {
                    bool top=Mathf.Abs(n0.original.y-n2.original.y)<.01f&&n0.original.y>car.structure.nodes[19].original.y+.4f;
                    int segments=top?4:1;
                    Vector2 t0=SurfaceUV(n0.original,n0.original,n2.original),t1=SurfaceUV(n1.original,n0.original,n2.original),t2=SurfaceUV(n2.original,n0.original,n2.original),t3=SurfaceUV(n3.original,n0.original,n2.original);
                    Vector3 normal=Vector3.Cross(p1-p0,p3-p0).normalized;if(Vector3.Dot(normal,car.Up)<0)normal=-normal;
                    for(int y=0;y<segments;y++)for(int x=0;x<segments;x++)
                    {
                        Vector3[] points=new Vector3[4];Vector2[] coords=new Vector2[4];
                        float[] us={x/(float)segments,(x+1f)/segments,(x+1f)/segments,x/(float)segments};
                        float[] vs={y/(float)segments,y/(float)segments,(y+1f)/segments,(y+1f)/segments};
                        for(int k=0;k<4;k++)
                        {
                            float u=us[k],v=vs[k];Vector3 rest=Vector3.Lerp(Vector3.Lerp(n0.original,n1.original,u),Vector3.Lerp(n3.original,n2.original,u),v);
                            float localX=rest.x-car.structure.nodes[19].original.x;
                            float crown=top?.045f*Mathf.Clamp01(1-localX*localX/(.94f*.94f)):0;
                            points[k]=Vector3.Lerp(Vector3.Lerp(p0,p1,u),Vector3.Lerp(p3,p2,u),v)+normal*crown;
                            coords[k]=Vector2.Lerp(Vector2.Lerp(t0,t1,u),Vector2.Lerp(t3,t2,u),v);
                        }
                        Emit(vertices,uv,triangles,points[0],points[1],points[2],points[3],coords);
                    }
                }
            }
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.subMeshCount=3;mesh.SetTriangles(intact,0);mesh.SetTriangles(damaged,1);mesh.SetTriangles(insideFaces,2);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        static Vector3 Side(Vector3 bottom,Vector3 top,Vector3 right,float t)
        {return Vector3.Lerp(bottom,top,t)+right*(Mathf.Sin(t*Mathf.PI)*.065f-.025f*(1-t));}
        Vector2 SurfaceUV(Vector3 p,Vector3 a,Vector3 b)
        {
            Vector3 origin=car.structure.nodes[19].original-new Vector3(0,.58f,0);p-=origin;a-=origin;b-=origin;
            if(Mathf.Abs(a.x-b.x)<.01f)return new Vector2((p.z+2.4f)/4.8f,(p.y-.58f)/.46f);
            if(Mathf.Abs(a.z-b.z)<.01f)return new Vector2((p.x+.88f)/1.76f,(p.y-.58f)/.46f);
            return new Vector2((p.x+.88f)/1.76f,(p.z+2.4f)/4.8f);
        }
        static float Arch(float z)
        {float d=Mathf.Min(Mathf.Abs(z-1.6f),Mathf.Abs(z+1.6f));return d>=.46f?0:Mathf.Clamp01((.40f+Mathf.Sqrt(.46f*.46f-d*d)-.58f)/.46f);}
        void Emit(List<Vector3> vertices,List<Vector2> uv,List<int> triangles,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Vector2[] coords)
        {
            int v=vertices.Count;vertices.AddRange(new[]{a,b,c,d,a,b,c,d});
            uv.AddRange(coords);uv.AddRange(coords);
            triangles.AddRange(reverseFace?new[]{v+2,v+1,v,v+3,v+2,v}:new[]{v,v+1,v+2,v,v+2,v+3});
            insideFaces.AddRange(reverseFace?new[]{v+4,v+5,v+6,v+4,v+6,v+7}:new[]{v+6,v+5,v+4,v+7,v+6,v+4});
        }
        public void Refresh()
        {
            trim?.Refresh(bodyVisible);interior?.Refresh(bodyVisible);
            System.Array.Clear(nodePlastic,0,nodePlastic.Length);System.Array.Clear(nodeSpan,0,nodeSpan.Length);
            foreach(var beam in car.structure.beams)
            {nodePlastic[beam.a]+=beam.plastic;nodePlastic[beam.b]+=beam.plastic;nodeSpan[beam.a]+=beam.initial;nodeSpan[beam.b]+=beam.initial;}
            for(int i=0;i<nodePlastic.Length;i++)nodePlastic[i]/=Mathf.Max(.001f,nodeSpan[i]);
            Faces(shellMesh,car.shell);shellRenderer.enabled=bodyVisible;
            for(int i=0;i<car.panels.Count;i++) { Faces(panelMeshes[i],car.panels[i].faces);panelRenderers[i].enabled=bodyVisible; }
            for(int i=0;i<car.wheels.Count;i++)
            {
                var w=car.wheels[i];var n=car.structure.nodes[w.hub];
                Vector3 axle=Vector3.Cross(w.up.sqrMagnitude>.1f?w.up:car.Up,w.forward.sqrMagnitude>.1f?w.forward:car.Forward).normalized;
                Quaternion rotation=WheelRotation(axle,w.spin);
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
            for(int i=0;i<pillars.Count;i++)
            {
                Vector3 a=SedanShape.Position(car,42+i),b=SedanShape.Position(car,anchors[i]);
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
        public static Quaternion WheelRotation(Vector3 axle,float radians)
        {return Quaternion.FromToRotation(Vector3.up,axle)*Quaternion.AngleAxis(radians*Mathf.Rad2Deg,Vector3.up);}
        static void Line(List<Vector3> v,List<Color> c,List<int> t,Vector3 a,Vector3 b,Color color)
        { t.Add(v.Count);v.Add(a);c.Add(color);t.Add(v.Count);v.Add(b);c.Add(color); }
    }
}

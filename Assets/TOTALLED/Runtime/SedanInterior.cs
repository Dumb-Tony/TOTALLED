using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    // Interior points are trilinearly skinned to the existing chassis lattice.
    public sealed class SedanInterior
    {
        readonly SacrificialSedan car;
        readonly Transform root;
        sealed class Part {public Mesh mesh;public Vector3[] rest,current;public GameObject go;}
        readonly List<Part> parts=new List<Part>();
        public SedanInterior(SacrificialSedan car,Transform parent)
        {
            this.car=car;root=parent;
            var vinyl=CrashLabArt.Surface(new Color(.095f,.11f,.115f),3,2);
            var cloth=CrashLabArt.Surface(new Color(.21f,.23f,.22f),0,3);
            var dash=SedanView.Material(new Color(.07f,.075f,.07f),0,.12f);
            var metal=SedanView.Material(new Color(.30f,.31f,.29f),.4f,.2f);
            var iron=CrashLabArt.Surface(new Color(.22f,.23f,.21f),0,2);
            var enginePaint=SedanView.Material(new Color(.28f,.35f,.29f),.2f,.18f);
            var alloy=SedanView.Material(new Color(.49f,.48f,.42f),.5f,.25f);
            // Rounded inner wheel tubs and rolled lips give the fenders depth.
            foreach(float side in new[]{-1f,1f})foreach(float axleZ in new[]{-1.6f,1.6f})
            {
                for(int segment=0;segment<16;segment++)
                {
                    float a=segment*Mathf.PI/16,b=(segment+1)*Mathf.PI/16;
                    Vector3 p=new Vector3(side*.945f,.40f+Mathf.Sin(a)*.47f,axleZ+Mathf.Cos(a)*.47f);
                    Vector3 q=new Vector3(side*.945f,.40f+Mathf.Sin(b)*.47f,axleZ+Mathf.Cos(b)*.47f);
                    Box("Rolled wheel arch",(p+q)*.5f,new Vector3(.035f,.035f,Vector3.Distance(p,q)+.009f),metal,Quaternion.LookRotation(q-p,Vector3.Cross(q-p,Vector3.right)));
                    p.x=side*.82f;q.x=p.x;
                    Box("Inner wheel tub",(p+q)*.5f,new Vector3(.19f,.035f,Vector3.Distance(p,q)+.009f),dash,Quaternion.LookRotation(q-p,Vector3.Cross(q-p,Vector3.right)));
                }
                Box("Lower sill",new Vector3(side*.93f,.595f,0),new Vector3(.075f,.09f,2.22f),dash);
            }
            foreach(float end in new[]{-1f,1f})
            {
                Box("Bumper impact rubber",new Vector3(0,.665f,end*2.44f),new Vector3(1.85f,.13f,.12f),dash);
                Box("Bumper upper chrome",new Vector3(0,.744f,end*2.445f),new Vector3(1.86f,.027f,.14f),metal);
            }
            Box("Boot liner",new Vector3(0,.64f,-1.58f),new Vector3(1.50f,.04f,1.42f),dash);
            Cylinder("Boot spare tire",new Vector3(0,.74f,-1.57f),new Vector3(.62f,.09f,.62f),dash);
            Cylinder("Spare steel wheel",new Vector3(0,.825f,-1.57f),new Vector3(.37f,.015f,.37f),metal);
            Cylinder("Spare center clamp",new Vector3(0,.85f,-1.57f),new Vector3(.07f,.03f,.07f),iron);
            Box("Engine bay floor",new Vector3(0,.635f,1.53f),new Vector3(1.35f,.035f,1.55f),dash);
            Box("Engine block",new Vector3(0,.77f,1.46f),new Vector3(.61f,.31f,.70f),enginePaint);
            foreach(float side in new[]{-1f,1f})
            {
                Box("Cylinder head",new Vector3(side*.22f,.89f,1.46f),new Vector3(.25f,.16f,.76f),alloy,Quaternion.Euler(0,0,-side*14));
                Box("Inner fender apron",new Vector3(side*.65f,.77f,1.55f),new Vector3(.10f,.22f,1.12f),iron);
                for(int pipe=0;pipe<4;pipe++)Box("Exhaust manifold",new Vector3(side*.37f,.76f,1.20f+pipe*.15f),new Vector3(.13f,.07f,.06f),iron,Quaternion.Euler(0,0,side*30));
            }
            Cylinder("Air cleaner",new Vector3(0,.99f,1.43f),new Vector3(.47f,.035f,.47f),alloy);
            Cylinder("Air cleaner center bolt",new Vector3(0,1.031f,1.43f),new Vector3(.035f,.015f,.035f),iron);
            Box("Battery",new Vector3(-.50f,.86f,1.95f),new Vector3(.28f,.20f,.28f),dash);
            Box("Battery label",new Vector3(-.50f,.968f,1.95f),new Vector3(.19f,.014f,.13f),alloy);
            Box("Battery positive terminal",new Vector3(-.59f,.98f,1.87f),new Vector3(.04f,.035f,.04f),SedanView.Material(new Color(.48f,.06f,.025f)));
            Box("Radiator core",new Vector3(0,.79f,2.14f),new Vector3(1.08f,.32f,.10f),dash);
            Box("Radiator top tank",new Vector3(0,.966f,2.14f),new Vector3(1.12f,.04f,.12f),iron);
            for(int fin=0;fin<17;fin++)Box("Radiator fin",new Vector3(-.48f+fin*.06f,.80f,2.08f),new Vector3(.015f,.28f,.014f),metal);
            Box("Coolant hose",new Vector3(.27f,.88f,1.94f),new Vector3(.085f,.085f,.36f),dash,Quaternion.Euler(0,25,0));
            Box("Firewall",new Vector3(0,.82f,.91f),new Vector3(1.35f,.31f,.04f),iron);
            foreach(float side in new[]{-1f,1f})
            {
                Box("Mirror stalk",new Vector3(side*1.0f,1.09f,.60f),new Vector3(.17f,.045f,.045f),dash);
                Box("Mirror housing",new Vector3(side*1.10f,1.13f,.57f),new Vector3(.13f,.14f,.22f),dash);
                Box("Mirror glass",new Vector3(side*1.10f,1.13f,.452f),new Vector3(.10f,.10f,.015f),metal);
            }
            foreach(float x in new[]{-.40f,.40f})
            {
                Box("Front seat cushion",new Vector3(x,.78f,.06f),new Vector3(.59f,.18f,.54f),cloth);
                Box("Front seat back",new Vector3(x,1.03f,-.23f),new Vector3(.57f,.62f,.16f),cloth,Quaternion.Euler(-9,0,0));
                Box("Headrest",new Vector3(x,1.38f,-.27f),new Vector3(.30f,.18f,.14f),vinyl);
                for(int seam=-2;seam<=2;seam++)Box("Seat stitching",new Vector3(x+seam*.09f,1.06f,-.135f),new Vector3(.012f,.34f,.01f),vinyl);
            }
            Box("Rear bench",new Vector3(0,.79f,-.63f),new Vector3(1.25f,.22f,.31f),cloth);
            Box("Rear seat back",new Vector3(0,1.0f,-.75f),new Vector3(1.23f,.43f,.13f),cloth);
            Box("Dashboard",new Vector3(0,1.02f,.59f),new Vector3(1.43f,.18f,.29f),dash);
            Box("Instrument binnacle",new Vector3(-.39f,1.14f,.58f),new Vector3(.45f,.14f,.24f),vinyl);
            Box("Center console",new Vector3(0,.80f,.17f),new Vector3(.20f,.20f,.75f),vinyl);
            Box("Gear lever",new Vector3(0,.97f,.19f),new Vector3(.035f,.17f,.035f),metal,Quaternion.Euler(-15,0,0));
            Box("Gear knob",new Vector3(0,1.07f,.21f),new Vector3(.08f,.06f,.07f),dash);
            Box("Cabin floor",new Vector3(0,.60f,0),new Vector3(1.52f,.035f,1.6f),vinyl);
            for(int i=0;i<16;i++)
            {
                float a=i*Mathf.PI*2/16,b=(i+1)*Mathf.PI*2/16;
                Vector3 p=new Vector3(-.4f+Mathf.Cos(a)*.17f,1.13f+Mathf.Sin(a)*.17f,.33f);
                Vector3 q=new Vector3(-.4f+Mathf.Cos(b)*.17f,1.13f+Mathf.Sin(b)*.17f,.33f);
                Box("Steering wheel rim",(p+q)*.5f,new Vector3(.035f,.035f,Vector3.Distance(p,q)+.015f),dash,Quaternion.LookRotation(q-p));
            }
            Box("Steering wheel spoke",new Vector3(-.4f,1.13f,.33f),new Vector3(.30f,.03f,.04f),metal);
            MergeMaterials();
        }
        void MergeMaterials()
        {
            var groups=new Dictionary<Material,List<Part>>();
            foreach(var p in parts){var material=p.go.GetComponent<MeshRenderer>().sharedMaterial;if(!groups.ContainsKey(material))groups[material]=new List<Part>();groups[material].Add(p);}
            parts.Clear();
            foreach(var group in groups)
            {
                var vertices=new List<Vector3>();var uv=new List<Vector2>();var triangles=new List<int>();
                foreach(var p in group.Value)
                {
                    int start=vertices.Count;vertices.AddRange(p.rest);uv.AddRange(p.mesh.uv);foreach(int index in p.mesh.triangles)triangles.Add(start+index);
                    p.go.SetActive(false);
                    if(Application.isPlaying){UnityEngine.Object.Destroy(p.mesh);UnityEngine.Object.Destroy(p.go);}else{UnityEngine.Object.DestroyImmediate(p.mesh);UnityEngine.Object.DestroyImmediate(p.go);}
                }
                var go=new GameObject("Skinned cabin and engine / "+group.Key.name);go.layer=2;go.transform.SetParent(root,false);
                var mesh=new Mesh();mesh.MarkDynamic();mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(triangles,0);
                go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=group.Key;
                parts.Add(new Part{mesh=mesh,rest=vertices.ToArray(),current=vertices.ToArray(),go=go});
            }
        }
        void Cylinder(string name,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.layer=2;go.transform.SetParent(root,false);go.GetComponent<Collider>().enabled=false;
            var mesh=UnityEngine.Object.Instantiate(go.GetComponent<MeshFilter>().sharedMesh);go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;
            var rest=mesh.vertices;for(int i=0;i<rest.Length;i++)rest[i]=position+Vector3.Scale(rest[i],scale);
            parts.Add(new Part{mesh=mesh,rest=rest,current=(Vector3[])rest.Clone(),go=go});
        }
        void Box(string name,Vector3 pos,Vector3 scale,Material material,Quaternion rotation=default)
        {
            var go=new GameObject(name);go.layer=2;go.transform.SetParent(root,false);
            var mesh=new Mesh();mesh.MarkDynamic();go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
            var vertices=new List<Vector3>();var indices=new List<int>();var uv=new List<Vector2>();
            Vector3[] corners={new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)};
            int[][] faces={new[]{0,3,2,1},new[]{4,5,6,7},new[]{0,4,7,3},new[]{1,2,6,5},new[]{3,7,6,2},new[]{0,1,5,4}};
            foreach(var f in faces)
            {int start=vertices.Count;foreach(int j in f){var p=Vector3.Scale(corners[j],scale*.5f);vertices.Add(pos+(rotation==default?p:rotation*p));}indices.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});uv.AddRange(new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up});}
            mesh.vertices=vertices.ToArray();mesh.uv=uv.ToArray();mesh.triangles=indices.ToArray();
            parts.Add(new Part{mesh=mesh,rest=vertices.ToArray(),current=vertices.ToArray(),go=go});
        }
        Vector3 Skin(Vector3 p)=>CrumpleCage.Skin(car,p);
        public void Refresh(bool visible)
        {
            foreach(var part in parts)
            {part.go.SetActive(visible);if(!visible)continue;for(int i=0;i<part.current.Length;i++)part.current[i]=Skin(part.rest[i]);part.mesh.vertices=part.current;part.mesh.RecalculateNormals();part.mesh.RecalculateBounds();}
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace Totalled
{
    public sealed class CrashLab : MonoBehaviour
    {
        public SacrificialSedan Active { get; private set; }
        public SedanView View { get; private set; }
        public Camera LabCamera { get; private set; }
        readonly List<SacrificialSedan> cars=new List<SacrificialSedan>();
        readonly List<SedanView> views=new List<SedanView>();
        SphereCollider probe;
        bool paused, slow, orbit, body=true, debug, components;
        int debugMode;
        float yaw=25, pitch=24, distance=9;
        public float LaunchSpeed=18;
        Vector3 cameraTarget;
        GUIStyle titleStyle, textStyle, smallStyle;
        public void Initialize()
        {
            if(Active!=null) return;
            Time.fixedDeltaTime=.02f;
            Application.targetFrameRate=60;
            Physics.gravity=new Vector3(0,-9.81f,0);
            RenderSettings.ambientLight=new Color(.57f,.63f,.7f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.12f,.16f,.21f);RenderSettings.fogDensity=.007f;
            var lightObject=new GameObject("Late afternoon test light");var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.3f;
            light.shadows=LightShadows.Soft;lightObject.transform.rotation=Quaternion.Euler(43,-35,0);
            var cameraObject=new GameObject("Crash Lab camera");LabCamera=cameraObject.AddComponent<Camera>();LabCamera.tag="MainCamera";
            LabCamera.backgroundColor=new Color(.12f,.16f,.21f);LabCamera.clearFlags=CameraClearFlags.SolidColor;LabCamera.farClipPlane=200;LabCamera.fieldOfView=52;
            cameraObject.AddComponent<AudioListener>();
            var p=new GameObject("Collision query probe");p.layer=2;probe=p.AddComponent<SphereCollider>();probe.isTrigger=true;probe.radius=.1f;p.transform.position=Vector3.down*1000;
            var concrete=SedanView.Material(new Color(.33f,.38f,.40f));var ground=SedanView.Material(new Color(.105f,.135f,.155f));
            var yellow=SedanView.Material(new Color(.98f,.65f,.09f));var stripe=SedanView.Material(new Color(.3f,.38f,.40f));
            Box("Test slab",new Vector3(0,-.3f,0),new Vector3(64,.6f,64),ground);
            Box("Front impact wall",new Vector3(0,1.5f,24),new Vector3(40,3,1),concrete);
            Box("Rear impact wall",new Vector3(0,1.5f,-24),new Vector3(40,3,1),concrete);
            Box("Side impact wall",new Vector3(24,1.5f,0),new Vector3(1,3,40),concrete);
            Box("Offset barrier",new Vector3(-6,.7f,12),new Vector3(2,1.4f,3),yellow);
            Box("Narrow pole",new Vector3(7,1.5f,14),new Vector3(.45f,3,.45f),yellow);
            Box("Ramp",new Vector3(-12,.75f,3),new Vector3(5,.5f,8),concrete,Quaternion.Euler(-12,0,0));
            Box("Landing block",new Vector3(-12,.3f,11),new Vector3(5,.6f,2),concrete);
            for(int i=-28;i<=28;i+=4)
            {
                Box("Survey line",new Vector3(i,.006f,0),new Vector3(.025f,.01f,60),stripe,default,false);
                Box("Survey line",new Vector3(0,.006f,i),new Vector3(60,.01f,.025f),stripe,default,false);
            }
            for(int i=-18;i<=18;i+=2) Box("Impact wall marker",new Vector3(i,1.6f,23.48f),new Vector3(.7f,2.8f,.04f),yellow,Quaternion.Euler(0,0,-20),false);
            Label("TOTALLED / CRASH LAB",new Vector3(-17,3.9f,23.4f),.38f);
            Label("01  /  FRONTAL IMPACT",new Vector3(-5,3.35f,23.4f),.23f);
            Label("02 / OFFSET",new Vector3(-8,2.4f,12),.18f);
            Physics.SyncTransforms();NewSpecimen();MoveCamera(true);
        }
        void Start()
        {
            Initialize();
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-crashlab-smoke")>=0)gameObject.AddComponent<CrashLabSmoke>();
        }
        public void NewSpecimen()
        {
            if(Active!=null) { Active.throttle=0;Active.brake=1; }
            Active=new SacrificialSedan(new Vector3(cars.Count*3.5f,0,0),probe);cars.Add(Active);View=new SedanView(Active);views.Add(View);
        }
        static void Box(string name,Vector3 pos,Vector3 scale,Material m,Quaternion rot=default,bool collides=true)
        {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=pos;g.transform.localScale=scale;
            if(rot!=default) g.transform.rotation=rot;
            g.GetComponent<MeshRenderer>().sharedMaterial=m;if(!collides) {g.layer=2;g.GetComponent<Collider>().enabled=false;}
        }
        static void Label(string value,Vector3 pos,float size)
        {
            var g=new GameObject(value);g.transform.position=pos;
            var t=g.AddComponent<TextMesh>();t.text=value;t.characterSize=size*.24f;t.fontSize=64;t.color=new Color(.86f,.9f,.9f);
        }
        void Update()
        {
            if(Active==null)return;
            if(Input.GetKeyDown(KeyCode.P))paused=!paused;
            if(Input.GetKeyDown(KeyCode.T))slow=!slow;
            if(Input.GetKeyDown(KeyCode.B))body=!body;
            if(Input.GetKeyDown(KeyCode.V))debug=!debug;
            if(Input.GetKeyDown(KeyCode.C))components=!components;
            if(Input.GetKeyDown(KeyCode.Tab))debugMode=(debugMode+1)%3;
            if(Input.GetKeyDown(KeyCode.N))NewSpecimen();
            if(Input.GetKeyDown(KeyCode.R))Active.Recover(new Vector3(0,1.1f,0));
            if(Input.GetKeyDown(KeyCode.Alpha1))Impact(0);
            if(Input.GetKeyDown(KeyCode.Alpha2))Impact(1);
            if(Input.GetKeyDown(KeyCode.Alpha3))Impact(2);
            if(Input.GetKeyDown(KeyCode.Alpha4))Impact(3);
            if(Input.GetKeyDown(KeyCode.LeftBracket))LaunchSpeed=Mathf.Max(6,LaunchSpeed-2);
            if(Input.GetKeyDown(KeyCode.RightBracket))LaunchSpeed=Mathf.Min(36,LaunchSpeed+2);
            if(Input.GetKeyDown(KeyCode.Period)&&paused)Simulate(.02f);
            orbit=Input.GetMouseButton(1);
            if(orbit) { yaw+=Input.GetAxis("Mouse X")*3;pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*3,5,80); }
            distance=Mathf.Clamp(distance-Input.mouseScrollDelta.y,4,22);
            Active.throttle=Input.GetAxisRaw("Vertical");Active.steering=Input.GetAxisRaw("Horizontal");Active.handbrake=Input.GetKey(KeyCode.Space);Active.brake=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.LeftControl)?1:0;
            foreach(var view in views) {view.bodyVisible=body;view.debugVisible=debug;view.debugMode=debugMode;view.componentsVisible=components;view.Refresh();}
            MoveCamera(false);
        }
        void FixedUpdate() { if(Active!=null&&!paused)Simulate(slow?.004f:.02f); }
        public void Simulate(float dt) { foreach(var c in cars)c.Tick(dt); }
        public void Impact(int direction)
        {
            paused=false;
            Active.Recover(direction==0?new Vector3(0,1.1f,14):direction==1?new Vector3(0,1.1f,-14):direction==2?new Vector3(14,1.1f,0):new Vector3(6.3f,1.1f,5));
            Active.Launch((direction==0||direction==3?Vector3.forward:direction==1?Vector3.back:Vector3.right)*LaunchSpeed);
            MoveCamera(true);
        }
        public void MoveCamera(bool snap)
        {
            if(LabCamera==null||Active==null)return;
            cameraTarget=snap?Active.Center:Vector3.Lerp(cameraTarget,Active.Center,1-Mathf.Exp(-Time.unscaledDeltaTime*9));
            Vector3 offset=Quaternion.Euler(pitch,yaw,0)*new Vector3(0,0,-distance);
            Vector3 lookAt=cameraTarget+Vector3.up*.3f;
            Vector3 desired=cameraTarget+offset;
            Vector3 path=desired-lookAt;
            if(Physics.SphereCast(lookAt,.2f,path.normalized,out RaycastHit obstruction,path.magnitude,1<<0,QueryTriggerInteraction.Ignore))
                desired=lookAt+path.normalized*Mathf.Max(.4f,obstruction.distance-.15f);
            LabCamera.transform.position=desired;LabCamera.transform.LookAt(lookAt);
        }
        void OnGUI()
        {
            if(Active==null)return;
            if(titleStyle==null) { titleStyle=new GUIStyle(GUI.skin.label){fontSize=25,fontStyle=FontStyle.Bold};titleStyle.normal.textColor=new Color(1,.69f,.22f);textStyle=new GUIStyle(GUI.skin.label){fontSize=15};smallStyle=new GUIStyle(GUI.skin.label){fontSize=12}; }
            float scale=Mathf.Clamp(Screen.height/900f,.8f,1.5f);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            GUI.Box(new Rect(18,18,360,480),GUIContent.none);
            GUILayout.BeginArea(new Rect(32,27,335,465));GUILayout.Label("TOTALLED",titleStyle);GUILayout.Label("CRASH LAB  /  SACRIFICIAL SEDAN  /  0.2",smallStyle);GUILayout.Space(12);
            GUILayout.Label($"{Mathf.Abs(Active.Speed)*3.6f:0} km/h    {(paused?"PAUSED":slow?"SLOW MOTION":"LIVE")}",textStyle);
            GUILayout.Label(Active.handbrake?"HANDBRAKE / REAR TIRES LOCKING":Active.brake>0?"BRAKE / FOUR-WHEEL STOP":"ROLLING / FULL TIRE GRIP",smallStyle);
            GUILayout.Label($"{Active.structure.nodes.Count} nodes  /  {Active.structure.beams.Count} beams",textStyle);
            GUILayout.Label($"Yielded {Active.structure.PlasticCount}   Broken {Active.structure.BrokenCount}",textStyle);
            GUILayout.Label($"Plastic travel {Active.structure.PlasticTotal:0.000} m",textStyle);
            GUILayout.Label($"Peak contact {Active.structure.PeakImpact/1000:0.0} kN",textStyle);
            GUILayout.Label($"Coolant {Active.temperature:0} °C   Fuel {Active.fuel:P0}",textStyle);
            GUILayout.Space(8);foreach(var p in Active.parts)GUILayout.Label($"{p.name}: {p.Condition:P0}",smallStyle);
            GUILayout.Space(5);foreach(var w in Active.wheels)GUILayout.Label($"{w.name}   {w.LiveLinks(Active.structure)}/4 links   {(w.punctured?"RIM / FLAT":"TIRE")}",smallStyle);
            GUILayout.EndArea();
            float h=Screen.height/scale;
            GUI.Box(new Rect(18,h-157,630,139),GUIContent.none);
            GUILayout.BeginArea(new Rect(32,h-150,605,128));
            GUILayout.Label("WASD drive  •  SPACE handbrake  •  SHIFT brake",textStyle);
            GUILayout.Label($"1 front / 2 rear / 3 side / 4 pole • [ ] speed {LaunchSpeed*3.6f:0} km/h",textStyle);
            GUILayout.Label("R recover, keep damage  •  N new car, keep wreck",smallStyle);
            GUILayout.Label("P pause  •  . step  •  T slow motion  •  B body  •  V skeleton",smallStyle);
            GUILayout.Label($"TAB stress / plastic / broken  •  C components  •  RMB orbit / scroll zoom",smallStyle);
            GUILayout.Label("Prototype: world collisions only; specimens do not yet collide with each other.",smallStyle);
            GUILayout.EndArea();
            if(debug) { GUI.Label(new Rect(395,25,500,30),"STRUCTURE / "+new[]{"STRESS","PLASTIC TRAVEL","BROKEN CONNECTIONS"}[debugMode],textStyle); }
        }
        void OnDestroy() { Time.timeScale=1; }
    }
}

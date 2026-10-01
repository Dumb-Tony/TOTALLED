using System;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace Totalled
{
    public sealed class DerbyRace:MonoBehaviour
    {
        public enum Phase {Ready,Countdown,Racing,Finished}
        public Phase State {get;private set;}
        public readonly SacrificialSedan[] Cars=new SacrificialSedan[4];
        public readonly RaceProgress[] Progress=new RaceProgress[4];
        readonly SedanView[] views=new SedanView[4];
        readonly DerbyDriver[] drivers={new DerbyDriver(1),new DerbyDriver(-1.8f),new DerbyDriver(1.8f),new DerbyDriver(-.4f)};
        readonly string[] names={"YOU","RUSTY","BULLDOG","SIDESWIPE"};
        readonly int[] finishes=new int[4];
        readonly VehicleWorld world=new VehicleWorld();
        public Camera Camera {get;private set;}
        public bool AutoDrive,Paused;
        public float Elapsed {get;private set;}
        public int FinishPosition {get;private set;}
        public int TotalContacts {get;private set;}
        public int CompletedRivals {get {int n=0;for(int i=1;i<4;i++)if(Progress[i].Finished)n++;return n;}}
        float countdown,resetCooldown,opponentVisualTime;
        int finishCount;
        Vector3 cameraTarget;
        GUIStyle title,heading,text,small,button;
        Texture2D map;
        public void Initialize()
        {
            if(Cars[0]!=null)return;
            Time.fixedDeltaTime=.02f;Time.maximumDeltaTime=.08f;Application.targetFrameRate=60;
            Physics.gravity=new Vector3(0,-9.81f,0);
            RenderSettings.ambientLight=new Color(.57f,.63f,.7f);
            RenderSettings.skybox=Resources.Load<Material>("TOTALLED/YardSky");
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.49f,.55f,.60f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.003f;
            YardReflections.Apply();
            var sun=new GameObject("Circuit afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.3f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(43,-35,0);
            QualitySettings.shadowDistance=70;
            Camera=new GameObject("Race camera").AddComponent<Camera>();Camera.tag="MainCamera";Camera.farClipPlane=230;Camera.fieldOfView=57;Camera.gameObject.AddComponent<AudioListener>();
            DerbyTrack.Build();
            var query=new GameObject("Race collision probe");query.layer=2;query.transform.position=Vector3.down*1000;
            var probe=query.AddComponent<SphereCollider>();probe.isTrigger=true;
            Color[] colors={new Color(.56f,.17f,.12f),new Color(.18f,.38f,.42f),new Color(.67f,.49f,.12f),new Color(.34f,.27f,.43f)};
            for(int i=0;i<4;i++){
                float a=-.25f+(i/2)*.14f,lane=i%2==0?-1.8f:1.8f;
                Cars[i]=new SacrificialSedan(Vector3.zero,probe);
                foreach(var n in Cars[i].structure.nodes){n.position=DerbyTrack.Point(a,lane)+DerbyTrack.Heading(a)*n.position;n.previous=n.position;}
                Progress[i]=new RaceProgress(Cars[i].Center);Cars[i].brake=1;
                views[i]=new SedanView(Cars[i],colors[i]);
            }
            Physics.SyncTransforms();
            for(int i=0;i<75;i++)world.Step(Cars,.02f);
            for(int i=0;i<4;i++)Progress[i].previous=Cars[i].Center;
            cameraTarget=Cars[0].Center;MoveCamera(true);
        }
        void Start(){
            Initialize();Resources.UnloadUnusedAssets();
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-race-smoke")>=0){AutoDrive=true;gameObject.AddComponent<DerbyRaceSmoke>();}
        }
        public void Begin(){if(State!=Phase.Ready)return;countdown=3;State=Phase.Countdown;Paused=false;}
        public void Restart(){AudioListener.pause=false;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
        void Update()
        {
            if(Cars[0]==null)return;
            if(Input.GetKeyDown(KeyCode.Return)){if(State==Phase.Ready)Begin();else if(State==Phase.Finished)Restart();}
            if(Input.GetKeyDown(KeyCode.Escape)&&State!=Phase.Ready&&State!=Phase.Finished)Paused=!Paused;
            if(Input.GetKeyDown(KeyCode.R))Restart();
            if(Input.GetKeyDown(KeyCode.F)&&State==Phase.Racing&&!Paused)Recover();
            if(Input.GetKeyDown(KeyCode.M))AudioListener.pause=!AudioListener.pause;
            if(State==Phase.Racing&&!Paused&&!AutoDrive){
                Cars[0].throttle=Input.GetAxisRaw("Vertical");Cars[0].steering=Input.GetAxisRaw("Horizontal");
                Cars[0].handbrake=Input.GetKey(KeyCode.Space);Cars[0].brake=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.LeftControl)?1:0;
            }
            views[0].Refresh();opponentVisualTime+=Time.unscaledDeltaTime;
            if(opponentVisualTime>=1f/30){opponentVisualTime=0;for(int i=1;i<4;i++)views[i].Refresh();}
            MoveCamera(false);
        }
        void FixedUpdate(){if(Cars[0]!=null&&!Paused)Simulate(.02f);}
        public void Simulate(float dt)
        {
            if(State==Phase.Ready||State==Phase.Finished)return;
            if(State==Phase.Countdown){
                countdown-=dt;
                if(countdown<=0){State=Phase.Racing;foreach(var car in Cars){car.brake=0;car.throttle=0;}}
                return;
            }
            Elapsed+=dt;resetCooldown=Mathf.Max(0,resetCooldown-dt);
            for(int i=0;i<4;i++){
                if(Progress[i].Finished){drivers[i].Drive(Cars[i],Cars,dt,false);continue;}
                if(i>0||AutoDrive)drivers[i].Drive(Cars[i],Cars,dt);
            }
            world.Step(Cars,dt);TotalContacts+=world.ContactCount;
            for(int i=0;i<4;i++){
                Progress[i].Advance(Cars[i].Center);
                if(Progress[i].Finished&&finishes[i]==0)finishes[i]=++finishCount;
            }
            if(Progress[0].Finished||Elapsed>=240){
                FinishPosition=finishes[0];State=Phase.Finished;
                foreach(var car in Cars){car.throttle=0;car.brake=1;}
            }
        }
        public void Recover()
        {
            if(resetCooldown>0)return;
            float a=Mathf.Max(0,Progress[0].Passed)*Mathf.PI*2/DerbyTrack.Gates-.08f;
            var car=Cars[0];Vector3 position=DerbyTrack.Point(a,-2)+Vector3.up*1.1f;
            car.structure.Relocate(position,DerbyTrack.Heading(a)*Quaternion.Inverse(car.Orientation),car.Center,SacrificialSedan.Index(1,0,3));
            Progress[0].previous=car.Center;resetCooldown=5;
        }
        public int Position()
        {
            if(finishes[0]>0)return finishes[0];
            int place=1;for(int i=1;i<4;i++)if(finishes[i]>0||Progress[i].Order(Cars[i].Center)>Progress[0].Order(Cars[0].Center))place++;
            return place;
        }
        public void RefreshViews(){foreach(var view in views)view.Refresh();}
        void MoveCamera(bool snap)
        {
            cameraTarget=snap?Cars[0].Center:Vector3.Lerp(cameraTarget,Cars[0].Center,1-Mathf.Exp(-Time.unscaledDeltaTime*8));
            Vector3 forward=Cars[0].Forward;forward.y=0;forward.Normalize();
            Vector3 desired=cameraTarget-forward*8+Vector3.up*4.3f;
            Camera.transform.position=snap?desired:Vector3.Lerp(Camera.transform.position,desired,1-Mathf.Exp(-Time.unscaledDeltaTime*5));
            Camera.transform.LookAt(cameraTarget+forward*3+Vector3.up*.4f);
        }
        void Styles()
        {
            if(title!=null)return;
            title=new GUIStyle(GUI.skin.label){fontSize=42,fontStyle=FontStyle.Bold};title.normal.textColor=new Color(1,.70f,.25f);
            heading=new GUIStyle(title){fontSize=24};
            text=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};
            small=new GUIStyle(text){fontSize=14};
            button=new GUIStyle(GUI.skin.button){fontSize=18,fontStyle=FontStyle.Bold};
            map=new Texture2D(160,160,TextureFormat.RGBA32,false);
            for(int y=0;y<160;y++)for(int x=0;x<160;x++){
                float wx=(x/159f-.5f)*90,wz=(y/159f-.5f)*120;
                float radial=Mathf.Sqrt(wx*wx/900+wz*wz/1936);
                map.SetPixel(x,y,Mathf.Abs(radial-1)<.14f?new Color(.37f,.39f,.37f):new Color(.07f,.09f,.09f));
            }
            map.Apply();
        }
        void OnGUI()
        {
            if(Cars[0]==null)return;Styles();
            float scale=Mathf.Min(Screen.width/1280f,Screen.height/720f);GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
            float width=Screen.width/scale,height=Screen.height/scale;
            Panel(new Rect(20,20,320,126));
            GUI.Label(new Rect(36,26,290,32),"MOTOR WORKS  /  01",heading);
            GUI.Label(new Rect(36,65,280,32),$"LAP {Mathf.Min(3,Progress[0].Lap+1)} / 3     POSITION {Position()} / 4",text);
            GUI.Label(new Rect(36,102,290,24),$"{Mathf.Abs(Cars[0].Speed)*3.6f:000} KM/H     {Elapsed:000.0}s / 240s",small);
            Panel(new Rect(20,height-57,width-40,38));
            GUI.Label(new Rect(35,height-50,width-70,28),"WASD / arrows: drive    SPACE: handbrake    SHIFT: brake    F: reset, keep damage    R: restart    ESC: pause    M: mute",small);
            Panel(new Rect(width-230,20,210,160));GUI.Label(new Rect(width-214,30,180,25),"THE FIELD",heading);
            for(int i=0;i<4;i++)GUI.Label(new Rect(width-214,65+i*24,190,25),names[i]+"   "+(Progress[i].Finished?"FINISHED":$"LAP {Progress[i].Lap+1}"),small);
            if(State==Phase.Racing&&!Paused){
                Rect mapRect=new Rect(width-190,height-240,160,160);GUI.DrawTexture(mapRect,map);
                for(int i=3;i>=0;i--){
                    Vector3 p=Cars[i].Center;GUI.color=i==0?new Color(1,.7f,.25f):Color.white;
                    GUI.DrawTexture(new Rect(mapRect.x+(p.x/90+.5f)*160-3,mapRect.y+(.5f-p.z/120)*160-3,7,7),Texture2D.whiteTexture);
                }
                GUI.color=Color.white;GUI.Label(new Rect(width-190,height-270,160,24),"CIRCUIT / YOU IN GOLD",small);
                int next=(Progress[0].Passed+1)%DerbyTrack.Gates;
                Vector3 target=DerbyTrack.Point(next*Mathf.PI*2/DerbyTrack.Gates);
                float angle=Vector3.SignedAngle(Cars[0].Forward,target-Cars[0].Center,Vector3.up);
                if(Mathf.Abs(angle)>100){Panel(new Rect(width*.5f-130,25,260,45));GUI.Label(new Rect(width*.5f-115,32,230,35),"TURN BACK TO THE COURSE",small);}
                if(Cars[0].parts[1].Condition<.35f||Cars[0].Up.y<.3f)GUI.Label(new Rect(36,158,420,30),"STRUGGLING? F resets position, not damage.",small);
            }
            if(State==Phase.Countdown){
                Panel(new Rect(width*.5f-80,height*.5f-70,160,140));
                GUI.Label(new Rect(width*.5f-24,height*.5f-45,100,100),Mathf.CeilToInt(countdown).ToString(),title);return;
            }
            if(State==Phase.Ready||State==Phase.Finished||Paused){
                float x=(width-620)*.5f,y=(height-380)*.5f;Panel(new Rect(x,y,620,380));
                GUI.Label(new Rect(x+30,y+20,570,60),State==Phase.Ready?"TOTALLED":Paused?"PAUSED":FinishPosition==1?"VICTORY":FinishPosition>0?"RACE FINISHED":"TIME'S UP",title);
                GUI.Label(new Rect(x+30,y+85,560,38),"01  /  MOTOR WORKS BANGER CIRCUIT",heading);
                string message=State==Phase.Ready?"Three laps. Four cars. First across the line wins.\n\nTrade paint, shove rivals into the walls, and keep your wheels turning. Every impact stays. Wrecks remain on the circuit.":Paused?"Take a breath. Your race and damage are preserved.":FinishPosition>0?$"Finished {FinishPosition} of 4 in {Elapsed:0.0} seconds.\n\nYour sedan survived with {Cars[0].structure.BrokenCount} broken connections. Try again for first place.":"The four-minute limit expired. A damaged car can still race while it can propel itself. Restart for a fresh field.";
                GUI.Label(new Rect(x+30,y+138,560,145),message,text);
                if(GUI.Button(new Rect(x+30,y+295,350,54),State==Phase.Ready?"START RACE  /  ENTER":Paused?"RESUME":"RACE AGAIN  /  ENTER",button)){
                    if(State==Phase.Ready)Begin();else if(Paused)Paused=false;else Restart();
                }
                if(Paused&&GUI.Button(new Rect(x+400,y+295,185,54),"RESTART",button))Restart();
            }
        }
        static void Panel(Rect rect){GUI.color=new Color(.035f,.05f,.06f,.94f);GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;}
    }
}

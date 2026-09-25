using System;
using System.Collections.Generic;
using System.IO;
using Totalled;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class CrashLabBuild
{
    const string ScenePath="Assets/TOTALLED/Scenes/CrashLab.unity";
    [MenuItem("TOTALLED/Create Crash Lab scene")]
    public static void CreateScene()
    {
        // Runtime-created meshes need explicit material assets so player stripping
        // cannot remove shaders that were only referenced by Shader.Find strings.
        Directory.CreateDirectory("Assets/TOTALLED/Resources/TOTALLED");
        AssetDatabase.Refresh();
        string surface="Assets/TOTALLED/Resources/TOTALLED/Surface.mat";
        string debug="Assets/TOTALLED/Resources/TOTALLED/DebugLines.mat";
        string skyPath="Assets/TOTALLED/Resources/TOTALLED/YardSky.mat";
        string textPath="Assets/TOTALLED/Resources/TOTALLED/YardText.mat";
        if(AssetDatabase.LoadAssetAtPath<Material>(textPath)==null)AssetDatabase.CreateAsset(new Material(Shader.Find("TOTALLED/DepthText")),textPath);
        var yardText=AssetDatabase.LoadAssetAtPath<Material>(textPath);yardText.shader=Shader.Find("TOTALLED/DepthText");EditorUtility.SetDirty(yardText);
        if(AssetDatabase.LoadAssetAtPath<Material>(skyPath)==null)
        {
            var sky=new Material(Shader.Find("Skybox/Procedural"));sky.SetColor("_SkyTint",new Color(.55f,.59f,.65f));
            sky.SetColor("_GroundColor",new Color(.42f,.40f,.37f));sky.SetFloat("_AtmosphereThickness",1.15f);sky.SetFloat("_Exposure",1.1f);
            AssetDatabase.CreateAsset(sky,skyPath);
        }
        var yardSky=AssetDatabase.LoadAssetAtPath<Material>(skyPath);yardSky.SetColor("_SkyTint",new Color(.5f,.5f,.5f));yardSky.SetFloat("_AtmosphereThickness",.85f);EditorUtility.SetDirty(yardSky);
        if(AssetDatabase.LoadAssetAtPath<Material>(surface)==null)AssetDatabase.CreateAsset(new Material(Shader.Find("Standard")),surface);
        if(AssetDatabase.LoadAssetAtPath<Material>(debug)==null)AssetDatabase.CreateAsset(new Material(Shader.Find("Hidden/Internal-Colored")),debug);
        Directory.CreateDirectory("Assets/TOTALLED/Scenes");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Crash Lab bootstrap").AddComponent<CrashLab>();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        PlayerSettings.bundleVersion="0.5.0";PlayerSettings.companyName="TOTALLED";PlayerSettings.productName="TOTALLED — Crash Lab";
        PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
        PlayerSettings.fullScreenMode=FullScreenMode.Windowed;
        PlayerSettings.runInBackground=true;
        AssetDatabase.SaveAssets();
    }
    [Serializable] public class Sample
    {
        public string phase;public int broken,plasticBeams;public float plasticTravel,speed,engine,temperature,centerY,driveDistance;
        public int attachedWheels,loosePanels,detachedPanels;public float maxWheelMisalignment;public float[] beamRest;
    }
    [Serializable] public class Report
    {
        public string utc,unity;public bool passed;public List<string> checks=new List<string>();public List<Sample> samples=new List<Sample>();
        public string scope="Single car versus static world. Numerical regression and rendered captures, not a handling-fun or BeamNG-fidelity certification.";
    }
    static Report report;
    static void Check(bool pass,string message)
    { report.checks.Add((pass?"PASS: ":"FAIL: ")+message);if(!pass)report.passed=false;Debug.Log(report.checks[report.checks.Count-1]); }
    static Sample Record(SacrificialSedan c,string phase)
    {
        var s=new Sample {phase=phase,broken=c.structure.BrokenCount,plasticBeams=c.structure.PlasticCount,plasticTravel=c.structure.PlasticTotal,speed=c.Speed,engine=c.parts[1].Condition,temperature=c.temperature,centerY=c.Center.y,beamRest=new float[c.structure.beams.Count]};
        foreach(var w in c.wheels) {if(w.LiveLinks(c.structure)>=2)s.attachedWheels++;s.maxWheelMisalignment=Mathf.Max(s.maxWheelMisalignment,Vector3.Angle(w.forward,c.Forward));}
        foreach(var p in c.panels) {if(p.LiveMounts(c.structure)<3)s.loosePanels++;if(p.LiveMounts(c.structure)==0)s.detachedPanels++;}
        for(int i=0;i<s.beamRest.Length;i++)s.beamRest[i]=c.structure.beams[i].rest;
        report.samples.Add(s);return s;
    }
    static void Run(CrashLab lab,int frames) { for(int i=0;i<frames;i++)lab.Simulate(.02f); }
    [MenuItem("TOTALLED/Run physics verification and captures")]
    public static void Verify()
    {
        Directory.CreateDirectory("Artifacts");CreateScene();
        var lab=UnityEngine.Object.FindFirstObjectByType<CrashLab>();lab.Initialize();
        report=new Report {utc=DateTime.UtcNow.ToString("O"),unity=Application.unityVersion,passed=true};
        var car=lab.Active;
        try
        {
            CrashLabRevisionChecks.Run(Check);
            CrashLabWheelChecks.Run(Check);
            CrashLabTargetChecks.Run(lab,Check);
            Run(lab,250);var settled=Record(car,"settled");Capture(lab,"01-pristine");
            Check(car.structure.Finite(),"Stationary structure stays finite after five seconds.");
            Check(settled.broken==0,"Gravity and static suspension load do not break pristine beams.");
            Check(settled.plasticTravel<.12f,"Stationary car does not crumple under its own weight.");
            Vector3 start=car.Center;car.throttle=.7f;Run(lab,100);car.throttle=0;
            Check(Vector3.Distance(start,car.Center)>2,"Pristine car propels itself using ground-contact wheel forces.");
            Record(car,"powered-drive");
            lab.Impact(0);Run(lab,110);var first=Record(car,"front-impact-1");Capture(lab,"02-first-impact");
            Check(car.structure.Finite(),"First wall impact remains finite.");
            Check(first.plasticTravel>settled.plasticTravel+.01f,"Wall collision produces permanent beam deformation.");
            float beforeRecovery=car.structure.PlasticTotal;int brokenBefore=car.structure.BrokenCount;
            car.Recover(new Vector3(0,1.1f,0));
            Check(Mathf.Abs(car.structure.PlasticTotal-beforeRecovery)<.00001f && car.structure.BrokenCount==brokenBefore,"Recovery preserves plastic deformation and broken connections.");
            bool unchanged=true;for(int i=0;i<first.beamRest.Length;i++)if(first.beamRest[i]!=car.structure.beams[i].rest)unchanged=false;
            Check(unchanged,"Recovery leaves every beam rest length bit-identical.");
            Run(lab,100);start=car.Center;car.throttle=1;Run(lab,100);car.throttle=0;
            float driven=Vector3.Distance(start,car.Center);Record(car,"damaged-powered-drive").driveDistance=driven;
            Check(driven>1,"Damaged car still propels itself after the first collision.");
            lab.Impact(0);Run(lab,110);var second=Record(car,"front-impact-2");Capture(lab,"03-repeat-impact");
            Check(second.plasticTravel>first.plasticTravel+.01f,"Second frontal hit adds permanent deformation to the same specimen.");
            Check(second.broken>=first.broken,"Broken connections cannot spontaneously heal.");
            lab.Impact(1);Run(lab,110);Record(car,"rear-impact");
            lab.Impact(2);Run(lab,220);var final=Record(car,"side-impact");Capture(lab,"04-multi-impact-wreck");
            Check(car.structure.Finite(),"Repeated front/rear/side impacts remain finite.");
            Check(final.plasticTravel>second.plasticTravel,"Multi-direction impacts continue to accumulate damage.");
            lab.View.bodyVisible=false;lab.View.debugVisible=true;lab.View.debugMode=1;Capture(lab,"05-plastic-skeleton");
            lab.View.debugMode=2;Capture(lab,"06-broken-connections");
            // Elastic response in isolation, well below yield, must recover.
            var elastic=new SoftStructure(null);int a=elastic.AddNode(Vector3.zero);int b=elastic.AddNode(Vector3.right);
            elastic.AddBeam(a,b,1e-8f,.5f,3);elastic.nodes[b].position+=Vector3.right*.001f;
            for(int i=0;i<20;i++)elastic.Step(.02f,null);
            Check(Mathf.Abs(Vector3.Distance(elastic.nodes[0].position,elastic.nodes[1].position)-1)<.001f && elastic.PlasticTotal==0,"Sub-yield elastic displacement recovers without plastic damage.");
            // Keep hammering the same car; breakage must emerge from collisions.
            lab.View.bodyVisible=true;lab.View.debugVisible=false;
            for(int hit=0;hit<6;hit++) {lab.Impact(hit%3);Run(lab,160);}
            var destroyed=Record(car,"ten-hit-endurance");Capture(lab,"07-ten-hit-wreck");
            Check(car.structure.Finite(),"Ten real wall impacts remain finite.");
            Check(destroyed.broken>0,"Real impacts break structural connections.");
            Check(destroyed.loosePanels>0,"Real impacts fail at least one panel mount.");
            lab.LaunchSpeed=28;
            for(int hit=0;hit<6;hit++) {lab.Impact(hit%4);Run(lab,180);}
            var severe=Record(car,"six-more-high-speed-hits");Capture(lab,"08-severe-wreck");
            Check(car.structure.Finite(),"High-speed endurance impacts remain finite.");
            Check(severe.maxWheelMisalignment>settled.maxWheelMisalignment+.5f,"Damaged mounting geometry changes wheel alignment.");
            Check(severe.detachedPanels>0,"High-speed impacts fully detach a panel after partial mount failure.");
            var detached=new List<int>();var oldPositions=new List<Vector3>();
            foreach(var p in car.panels)if(p.LiveMounts(car.structure)==0)foreach(int n in p.nodes){detached.Add(n);oldPositions.Add(car.structure.nodes[n].position);}
            car.Recover(new Vector3(0,1.1f,0));
            bool debrisStayed=true;for(int i=0;i<detached.Count;i++)if(car.structure.nodes[detached[i]].position!=oldPositions[i])debrisStayed=false;
            Check(debrisStayed&&detached.Count>0,"Recovery leaves fully detached panel debris in place.");
            Run(lab,150);Record(car,"severe-wreck-recovered");Capture(lab,"09-recovered-wreck");
        }
        catch(Exception e) {report.passed=false;report.checks.Add("EXCEPTION: "+e);Debug.LogException(e);}
        File.WriteAllText("Artifacts/verification.json",JsonUtility.ToJson(report,true));
        Debug.Log("CRASH LAB VERIFICATION "+(report.passed?"PASSED":"FAILED"));
        if(Application.isBatchMode)EditorApplication.Exit(report.passed?0:2);
    }
    static void Capture(CrashLab lab,string name)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)return;
        lab.View.Refresh();var camera=lab.LabCamera;Vector3 center=lab.Active.Center;
        camera.transform.position=center+new Vector3(-6,3.9f,6.8f);camera.transform.LookAt(center+Vector3.up*.25f);
        var target=new RenderTexture(1280,720,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
        var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
        File.WriteAllBytes("Artifacts/"+name+".png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;
        UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);
    }
    [MenuItem("TOTALLED/Build Windows prototype")]
    public static void BuildWindows()
    {
        CreateScene();Directory.CreateDirectory("Builds/Windows");
        var result=BuildPipeline.BuildPlayer(new[]{ScenePath},"Builds/Windows/TOTALLED-CrashLab.exe",BuildTarget.StandaloneWindows64,BuildOptions.Development);
        if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Windows build failed: "+result.summary.result);
    }
}






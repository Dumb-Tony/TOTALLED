using System;
using System.IO;
using Totalled;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class DerbyRaceBuild
{
    [Serializable] public class Result {public bool passed=true;public System.Collections.Generic.List<string> checks=new System.Collections.Generic.List<string>();public int[] gates;public int contacts;public float raceSeconds;public double meanTickMs;public string scope="Full deterministic four-car autonomous race, editor simulation; not browser FPS.";}
    static Result result;
    static void Check(bool pass,string text){result.checks.Add((pass?"PASS: ":"FAIL: ")+text);if(!pass)result.passed=false;Debug.Log(result.checks[result.checks.Count-1]);}
    public static void CreateScene()
    {
        CrashLabBuild.CreateScene();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        new GameObject("Motor Works race").AddComponent<DerbyRace>();
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/TOTALLED/Scenes/MotorWorks.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/TOTALLED/Scenes/MotorWorks.unity",true)};
        PlayerSettings.bundleVersion="0.9.0";AssetDatabase.SaveAssets();
    }
    public static void Verify()
    {
        result=new Result();Directory.CreateDirectory("Artifacts");
        try{
            CreateScene();var race=UnityEngine.Object.FindFirstObjectByType<DerbyRace>();race.Initialize();
            Check(race.State==DerbyRace.Phase.Ready&&race.Cars.Length==4,"Race opens on ready screen with player and three opponents.");
            var progress=new RaceProgress(new Vector3(30,0,-1));progress.Advance(new Vector3(30,0,1));
            progress.previous=new Vector3(30,0,-1);progress.Advance(new Vector3(30,0,1));
            Check(progress.Passed==0,"Repeated start-line crossing cannot award laps.");
            float a=5*Mathf.PI*2/DerbyTrack.Gates;progress.previous=DerbyTrack.Point(a)-DerbyTrack.Tangent(a);progress.Advance(DerbyTrack.Point(a)+DerbyTrack.Tangent(a));
            Check(progress.Passed==0,"Skipping ordered gates cannot shortcut a lap.");
            a=Mathf.PI*2/DerbyTrack.Gates;progress.previous=DerbyTrack.Point(a)+DerbyTrack.Tangent(a);progress.Advance(DerbyTrack.Point(a)-DerbyTrack.Tangent(a));
            Check(progress.Passed==0,"Reverse gate crossing does not count.");
            for(int i=1;i<=72;i++){a=(i%24)*Mathf.PI*2/24;progress.previous=DerbyTrack.Point(a)-DerbyTrack.Tangent(a);progress.Advance(DerbyTrack.Point(a)+DerbyTrack.Tangent(a));}
            Check(progress.Finished&&progress.Lap==3,"Three full ordered laps finish the race.");
            race.Begin();race.Simulate(1);
            Check(race.State==DerbyRace.Phase.Countdown&&race.Elapsed==0,"Countdown holds the field and does not consume race time.");
            race.AutoDrive=true;var watch=System.Diagnostics.Stopwatch.StartNew();int ticks=0;
            while(race.State!=DerbyRace.Phase.Finished&&ticks<12200){race.Simulate(.02f);ticks++;if(ticks%1500==0)Debug.Log("RACE PROGRESS "+ticks+": "+string.Join(",",Array.ConvertAll(race.Progress,p=>p.Passed.ToString())));}
            watch.Stop();result.meanTickMs=watch.Elapsed.TotalMilliseconds/ticks;result.raceSeconds=race.Elapsed;result.contacts=race.TotalContacts;
            result.gates=Array.ConvertAll(race.Progress,p=>p.Passed);
            bool finite=true;foreach(var car in race.Cars)finite&=car.structure.Finite();
            Check(finite,"All four cars remain finite through the event.");
            Check(race.State==DerbyRace.Phase.Finished,"Event always reaches results, including time-limit fallback.");
            Check(race.Progress[0].Finished,"Autonomous player can complete the full three-lap circuit.");
            int progressed=0;for(int i=1;i<4;i++)if(race.Progress[i].Passed>=24)progressed++;
            Check(progressed>=2,"At least two opponents complete a lap using wheel forces and steering.");
            Check(race.TotalContacts>0,"Racing field produces real two-way vehicle contacts.");
            float plastic=race.Cars[0].structure.PlasticTotal;int passed=race.Progress[0].Passed;race.Recover();
            Check(Mathf.Abs(race.Cars[0].structure.PlasticTotal-plastic)<.0001f&&race.Progress[0].Passed==passed,"Track recovery preserves damage and cannot advance race progress.");
            race.RefreshViews();Capture(race);
        }catch(Exception e){result.passed=false;Debug.LogException(e);result.checks.Add(e.ToString());}
        File.WriteAllText("Artifacts/race-verification.json",JsonUtility.ToJson(result,true));EditorApplication.Exit(result.passed?0:2);
    }
    static void Capture(DerbyRace race)
    {
        race.Camera.transform.position=new Vector3(80,75,-85);race.Camera.transform.LookAt(Vector3.zero);
        var rt=new RenderTexture(1600,900,24);race.Camera.targetTexture=rt;race.Camera.Render();RenderTexture.active=rt;
        var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes("Artifacts/race-overview.png",tex.EncodeToPNG());
        race.Camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
    }
}

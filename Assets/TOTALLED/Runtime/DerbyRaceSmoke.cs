using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Totalled
{
    public sealed class DerbyRaceSmoke:MonoBehaviour
    {
        [Serializable] public class Result {public bool passed,restartPassed;public int captures;public string scope="Packaged race startup/countdown, autonomous driving and scene restart; not interactive browser FPS.";public List<string> errors=new List<string>();}
        static Result result=new Result();
        static bool restarting;
        string folder;
        void OnEnable(){Application.logMessageReceived+=Log;}
        void OnDisable(){Application.logMessageReceived-=Log;}
        void Log(string text,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)result.errors.Add(text);}
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../..","Artifacts"));
            var race=GetComponent<DerbyRace>();
            if(restarting){
                yield return null;
                result.restartPassed=race.State==DerbyRace.Phase.Ready&&race.Elapsed==0&&race.Progress[0].Passed==-1&&FindObjectsByType<VehicleFeedback>(FindObjectsSortMode.None).Length==4;
                result.passed=result.passed&&result.restartPassed&&result.errors.Count==0;
                File.WriteAllText(Path.Combine(folder,"race-smoke.json"),JsonUtility.ToJson(result,true));Application.Quit(result.passed?0:2);yield break;
            }
            yield return new WaitForSeconds(1);Capture(race,"race-ready");
            bool ready=race.State==DerbyRace.Phase.Ready;race.Begin();
            bool countdown=race.State==DerbyRace.Phase.Countdown;
            yield return new WaitForSeconds(4);Capture(race,"race-start");
            yield return new WaitForSeconds(25);Capture(race,"race-pack");
            bool moving=true;foreach(var p in race.Progress)if(p.Passed<1)moving=false;
            foreach(var c in race.Cars)if(!c.structure.Finite())moving=false;
            result.passed=ready&&countdown&&moving&&race.State==DerbyRace.Phase.Racing&&result.errors.Count==0;
            restarting=true;race.Restart();
        }
        void Capture(DerbyRace race,string name)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+"-hud.png"));race.RefreshViews();var rt=new RenderTexture(1600,900,24);race.Camera.targetTexture=rt;race.Camera.Render();RenderTexture.active=rt;
            var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();
            File.WriteAllBytes(Path.Combine(folder,name+".png"),tex.EncodeToPNG());race.Camera.targetTexture=null;RenderTexture.active=null;Destroy(rt);Destroy(tex);result.captures++;
        }
    }
}

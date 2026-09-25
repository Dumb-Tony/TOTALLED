using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Totalled
{
    // Optional unattended standalone check; normal launches never run this sequence.
    public sealed class CrashLabSmoke : MonoBehaviour
    {
        [Serializable] public sealed class Result
        {
            public bool passed;
            public int offscreenCaptures, brokenBeams, activeEngineSources, skidVertices;
            public bool tireSoundActive;
            public float seconds, accumulatedPlasticTravel;
            public string scope="Hidden standalone player, explicit camera renders; excludes screen-space HUD and interactive framerate measurement.";
            public List<string> errors=new List<string>();
        }
        readonly Result result=new Result();
        string folder;
        float began;
        void OnEnable() {Application.logMessageReceived+=OnLog;}
        void OnDisable() {Application.logMessageReceived-=OnLog;}
        void OnLog(string message,string stack,LogType type)
        {if(type==LogType.Exception||type==LogType.Error||type==LogType.Assert)result.errors.Add(message);}
        IEnumerator Start()
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../..","Artifacts"));Directory.CreateDirectory(folder);
            var lab=GetComponent<CrashLab>();began=Time.realtimeSinceStartup;
            yield return new WaitForSeconds(2);
            foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if(source.name=="Engine"&&source.isPlaying&&source.volume>0)result.activeEngineSources++;
            Capture(lab,"runtime-pristine.png");
            yield return new WaitForSeconds(1);
            lab.Impact(0);yield return new WaitForSeconds(3);
            lab.Impact(0);yield return new WaitForSeconds(3);
            Capture(lab,"runtime-damaged.png");
            yield return new WaitForSeconds(1);
            result.brokenBeams=lab.Active.structure.BrokenCount;
            result.accumulatedPlasticTravel=lab.Active.structure.PlasticTotal;
            lab.NewSpecimen();lab.Active.Recover(new Vector3(0,1.1f,-16));
            yield return new WaitForSeconds(1);
            lab.Active.Launch(Vector3.forward*16);lab.Active.steering=.5f;lab.Active.handbrake=true;
            yield return new WaitForSeconds(.9f);
            foreach(var feedback in UnityEngine.Object.FindObjectsByType<VehicleFeedback>(FindObjectsSortMode.None))result.skidVertices+=feedback.SkidVertices;
            foreach(var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
                if(source.name=="Tire scrub"&&source.isPlaying&&source.volume>.01f)result.tireSoundActive=true;
            Capture(lab,"runtime-slide.png");result.seconds=Time.realtimeSinceStartup-began;
            result.passed=result.errors.Count==0&&result.offscreenCaptures==3&&result.activeEngineSources>=2&&result.skidVertices>0&&result.tireSoundActive&&lab.Active.structure.Finite()&&result.accumulatedPlasticTravel>.01f;
            File.WriteAllText(Path.Combine(folder,"runtime-smoke.json"),JsonUtility.ToJson(result,true));
            Application.Quit(result.passed?0:2);
        }
        void Capture(CrashLab lab,string file)
        {
            // A hidden Windows player may skip backbuffer rendering entirely. Explicit
            // offscreen rendering checks shader inclusion without inventing an FPS score.
            lab.View.Refresh();var camera=lab.LabCamera;
            var target=new RenderTexture(1600,900,24);camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var pixels=new Texture2D(1600,900,TextureFormat.RGB24,false);
            pixels.ReadPixels(new Rect(0,0,1600,900),0,0);pixels.Apply();
            File.WriteAllBytes(Path.Combine(folder,file),pixels.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;Destroy(pixels);Destroy(target);
            result.offscreenCaptures++;
        }
    }
}

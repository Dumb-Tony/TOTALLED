using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class CrashLabPublish
{
    [MenuItem("TOTALLED/Build browser playtest")]
    public static void BuildWeb()
    {
        CrashLabBuild.CreateScene();
        PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback=true;
        PlayerSettings.WebGL.dataCaching=true;
        PlayerSettings.WebGL.nameFilesAsHashes=true;
        PlayerSettings.WebGL.memorySize=256;
        Directory.CreateDirectory("Builds/WebGL");
        var result=BuildPipeline.BuildPlayer(new[]{"Assets/TOTALLED/Scenes/CrashLab.unity"},"Builds/WebGL",BuildTarget.WebGL,BuildOptions.None);
        if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Web build failed: "+result.summary.result);
        string build="Builds/WebGL/Build";
        string generated=File.ReadAllText("Builds/WebGL/index.html");
        Func<string,string> asset=pattern=>Path.GetFileName(Directory.GetFiles(build,pattern).Single(file=>generated.Contains(Path.GetFileName(file))));
        string page=File.ReadAllText("Tools/Playtest/index.html");
        page=page.Replace("__LOADER__",asset("*.loader.js"))
            .Replace("__DATA__",asset("*.data*"))
            .Replace("__FRAMEWORK__",asset("*.framework.js*"))
            .Replace("__WASM__",asset("*.wasm*"));
        File.WriteAllText("Builds/WebGL/index.html",page);
        File.WriteAllText("Builds/WebGL/.nojekyll","");
    }
}

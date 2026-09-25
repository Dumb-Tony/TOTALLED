using System;
using Totalled;
using UnityEngine;

public static class CrashLabWheelChecks
{
    public static void Verify()
    {
        CrashLabBuild.CreateScene();UnityEngine.Object.FindFirstObjectByType<CrashLab>().Initialize();
        Run((passed,message)=>{Debug.Log((passed?"PASS: ":"FAIL: ")+message);if(!passed)throw new Exception(message);});
    }
    public static void Run(Action<bool,string> check)
    {
        var probeObject=new GameObject("Wheel rendering probe");probeObject.layer=2;probeObject.transform.position=Vector3.down*1000;
        var probe=probeObject.AddComponent<SphereCollider>();probe.isTrigger=true;
        var car=new SacrificialSedan(new Vector3(-5,0,-15),probe);var view=new SedanView(car);
        for(int i=0;i<150;i++)car.Tick(.02f);
        car.throttle=.5f;for(int i=0;i<70;i++)car.Tick(.02f);view.Refresh();
        var tire=view.root.transform.Find("Rear L tire");var rim=view.root.transform.Find("Rear L rim");
        Quaternion tireBefore=tire.rotation,rimBefore=rim.rotation;Vector3 before=car.Center;
        car.Tick(.02f);car.Tick(.02f);view.Refresh();
        check(Vector3.Distance(before,car.Center)>.01f&&Quaternion.Angle(tireBefore,tire.rotation)>1&&Quaternion.Angle(rimBefore,rim.rotation)>1,
            "Powered driving visibly rotates both the tire mesh and detailed rim.");
        Vector3 axle=Vector3.Cross(car.wheels[0].up,car.wheels[0].forward).normalized;
        check(Vector3.Dot(tire.up,axle)>.99f,"Rolling tire stays aligned to its actual suspension axle.");
        Vector3 bottom=-car.Up;
        Vector3 motion=(tire.rotation*Quaternion.Inverse(tireBefore))*bottom-bottom;
        check(Vector3.Dot(motion,car.Forward)<-.001f,"Forward rolling moves the bottom tread backward relative to the hub.");
        car.throttle=0;car.Launch(-car.Forward*3);tireBefore=tire.rotation;
        car.Tick(.02f);car.Tick(.02f);view.Refresh();
        motion=(tire.rotation*Quaternion.Inverse(tireBefore))*bottom-bottom;
        check(Vector3.Dot(motion,car.Forward)>.001f,"Reverse rolling moves the bottom tread forward relative to the hub.");
        UnityEngine.Object.DestroyImmediate(view.root);UnityEngine.Object.DestroyImmediate(probeObject);
    }
}

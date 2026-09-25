using System;
using System.Collections.Generic;
using System.IO;
using Totalled;
using UnityEngine;

public static class CrashLabRevisionChecks
{
    [Serializable] public sealed class Maneuver
    {
        public string mode;public float speed, distance, yaw, slipAngle;public bool finite;
    }
    [Serializable] public sealed class Results
    {
        public float idleMotion, idleShapeError, drivenImpactPlasticTravel, drivenImpactNoseCrush;
        public List<Maneuver> maneuvers=new List<Maneuver>();
    }
    static void Step(SacrificialSedan car,int frames) {for(int i=0;i<frames;i++)car.Tick(.02f);}
    static float Nose(SacrificialSedan car)
    {return Vector3.Dot(car.structure.nodes[SacrificialSedan.Index(1,0,6)].position-car.structure.nodes[SacrificialSedan.Index(1,0,3)].position,car.Forward);}
    public static void Run(Action<bool,string> check)
    {
        var results=new Results();
        var g=new GameObject("Revision test collision probe");g.layer=2;g.transform.position=Vector3.down*1000;
        var probe=g.AddComponent<SphereCollider>();probe.isTrigger=true;
        try
        {
            var car=new SacrificialSedan(Vector3.zero,probe);Step(car,250);
            var baseline=new List<Vector3>();var samples=new List<int>();
            Vector3 originalCenter=Vector3.zero;for(int i=0;i<car.chassisCount;i++)originalCenter+=car.structure.nodes[i].original;originalCenter/=car.chassisCount;
            foreach(var panel in car.panels)foreach(int n in panel.nodes)
            {
                samples.Add(n);Vector3 local=Quaternion.Inverse(car.Orientation)*(car.structure.nodes[n].position-car.Center);baseline.Add(local);
                results.idleShapeError=Mathf.Max(results.idleShapeError,(local-(car.structure.nodes[n].original-originalCenter)).magnitude);
            }
            for(int frame=0;frame<150;frame++)
            {
                car.Tick(.02f);
                for(int i=0;i<samples.Count;i++)
                {
                    Vector3 local=Quaternion.Inverse(car.Orientation)*(car.structure.nodes[samples[i]].position-car.Center);
                    results.idleMotion=Mathf.Max(results.idleMotion,Vector3.Distance(baseline[i],local));
                }
            }
            check(results.idleMotion<.005f,$"Intact panels settle: maximum idle movement {results.idleMotion:0.0000} m (<0.005 m).");
            check(results.idleShapeError<.08f,$"Intact panel shape stays supported: error {results.idleShapeError:0.0000} m (<0.08 m).");
            check(car.structure.BrokenCount==0&&car.structure.PlasticTotal<.01f,"No damage accumulates during eight seconds at rest.");
            car.throttle=.65f;car.steering=.2f;Step(car,75);car.throttle=0;car.brake=.6f;Step(car,40);
            check(car.structure.BrokenCount==0&&car.structure.PlasticTotal<.03f,"Normal acceleration, steering and braking do not loosen or damage panels.");

            // Match initial speed, steering and timing. Compare measured trajectories,
            // not the tire model's internal coefficients.
            foreach(bool turning in new[]{false,true})foreach(string mode in new[]{"coast","foot brake","handbrake"})
            {
                var c=new SacrificialSedan(new Vector3(0,0,-16),probe);Step(c,150);
                Vector3 start=c.Center,forward=c.Forward;c.Launch(Vector3.forward*16);
                c.steering=turning?.35f:0;c.brake=mode=="foot brake"?1:0;c.handbrake=mode=="handbrake";
                Step(c,60);
                float slip=Mathf.Abs(Mathf.Atan2(Vector3.Dot(c.Velocity,c.Right),Vector3.Dot(c.Velocity,c.Forward))*Mathf.Rad2Deg);
                var m=new Maneuver {mode=(turning?"turn / ":"straight / ")+mode,speed=c.Velocity.magnitude,distance=Vector3.Distance(start,c.Center),yaw=Vector3.Angle(forward,c.Forward),slipAngle=slip,finite=c.structure.Finite()};
                results.maneuvers.Add(m);
            }
            var coast=results.maneuvers[0];var foot=results.maneuvers[1];var hand=results.maneuvers[2];
            check(foot.speed<hand.speed-1&&foot.distance<hand.distance,"Four-wheel brake slows the car more strongly than the rear handbrake.");
            check(foot.speed<coast.speed-3&&hand.speed<coast.speed-1,"Both brakes dissipate forward speed instead of accelerating the car.");
            var turnFoot=results.maneuvers[4];var turnHand=results.maneuvers[5];
            check(turnHand.slipAngle>turnFoot.slipAngle+3,$"Handbrake develops more lateral slip in the same turn ({turnHand.slipAngle:0.0}° vs {turnFoot.slipAngle:0.0}°).");
            check(turnHand.yaw>turnFoot.yaw+3,$"Rear locking permits more rotation ({turnHand.yaw:0.0}° vs {turnFoot.yaw:0.0}°).");
            bool finite=true;foreach(var m in results.maneuvers)finite&=m.finite;
            check(finite,"Brake and handbrake maneuvers stay finite.");

            var impact=new SacrificialSedan(Vector3.zero,probe);Step(impact,150);
            float pristineNose=Nose(impact);impact.throttle=1;Step(impact,350);impact.throttle=0;
            results.drivenImpactPlasticTravel=impact.structure.PlasticTotal;
            results.drivenImpactNoseCrush=pristineNose-Nose(impact);
            check(results.drivenImpactPlasticTravel>.05f&&results.drivenImpactNoseCrush>.08f,
                $"Driving into the wall normally causes a lasting visible dent ({results.drivenImpactNoseCrush:0.000} m nose crush), without launch shortcuts.");
        }
        finally
        {
            File.WriteAllText("Artifacts/revision-checks.json",JsonUtility.ToJson(results,true));UnityEngine.Object.DestroyImmediate(g);
        }
    }
}

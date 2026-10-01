using UnityEngine;
namespace Totalled
{
    public sealed class DerbyDriver
    {
        readonly float lane;
        float stuck,reverse;
        public DerbyDriver(float lane){this.lane=lane;}
        public void Drive(SacrificialSedan car,SacrificialSedan[] field,float dt,bool attack=true)
        {
            float angle=DerbyTrack.Angle(car.Center);
            float targetLane=lane;
            if(attack)foreach(var rival in field){
                if(rival==car)continue;Vector3 delta=rival.Center-car.Center;
                float ahead=Vector3.Dot(delta,car.Forward);
                if(ahead>1&&ahead<9&&delta.magnitude<10){
                    float radial=Mathf.Sqrt(rival.Center.x*rival.Center.x/(30*30)+rival.Center.z*rival.Center.z/(44*44));
                    targetLane=Mathf.Clamp((radial-1)*35,-3,3);break;
                }
            }
            float look=7+Mathf.Abs(car.Speed)*.45f;
            Vector3 target=DerbyTrack.Point(angle+look/38,targetLane);
            Vector3 local=Quaternion.Inverse(car.Orientation)*(target-car.Center);
            float steer=Mathf.Atan2(2*3.2f*local.x,Mathf.Max(1,local.x*local.x+local.z*local.z))*Mathf.Rad2Deg;
            car.steering=Mathf.Clamp(steer/24,-1,1);car.handbrake=false;
            float desired=Mathf.Lerp(12.5f,9,Mathf.Abs(Mathf.Sin(angle)));
            car.brake=car.Speed>desired+1?.35f:0;
            car.throttle=car.Speed<desired?1:.18f;
            if(Mathf.Abs(car.Speed)<.65f)stuck+=dt;else stuck=0;
            if(stuck>3.5f&&reverse<=0){reverse=2;stuck=0;}
            if(reverse>0){reverse-=dt;car.throttle=-.75f;car.steering=-car.steering;car.brake=0;}
        }
    }
}

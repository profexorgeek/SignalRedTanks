using FlatRedBall;
using FlatRedBall.Math;
using Microsoft.Xna.Framework;
using NarfoxGameTools.Extensions;
using SignalRed.Client;
using System;
using TankMp.Input;
using TankMp.Models;

namespace TankMp.Entities.Tanks
{
    public partial class TankBase
    {
        const float MaxSpeed = 200;
        const float MaxDrag = 1.5f;
        const float MaxAcceleration = MaxSpeed / MaxDrag;
        const float SecondsToLerpToState = 0.15f;
        const float ReloadSeconds = 0.2f;
        float timeToNextShot = 0;

        public bool LocallyOwned => SignalRedClient.Instance.ClientId == Controller.OwnerClientId;
        float FrameLerp => TimeManager.SecondDifference / SecondsToLerpToState;
        public ITankController Controller { get; set; }
        

        private void CustomInitialize()
        {
            Drag = MaxDrag;
            Turret.ParentRotationChangesRotation = false;
        }

        private void CustomActivity()
        {
            DoInput();
        }

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        void DoInput()
        {
            // EARLY OUT: no controller!
            if (Controller == null) return;

            Controller.Update();

            var throttle = Controller.MovementMagnitude.Clamp(-1f, 1f);
            var velocityAngle = (float)Math.Atan2(Velocity.Y, Velocity.X);
            var rotationToVelocityAngle = MathFunctions.AngleToAngle(RotationZ, velocityAngle);

            Acceleration.X = (float)(Math.Cos(Controller.MovementAngle) * (MaxAcceleration * throttle));
            Acceleration.Y = (float)(Math.Sin(Controller.MovementAngle) * (MaxAcceleration * throttle));
            RotationZ += MathHelper.Lerp(0, rotationToVelocityAngle, FrameLerp);
            Turret.RelativeRotationZ = Controller.AimAngle;

            if(LocallyOwned)
            {
                timeToNextShot -= TimeManager.SecondDifference;
                if(timeToNextShot <= 0 && Controller.Firing)
                {
                    SignalRedClient.Instance
                        .CreateEntity<BulletNetworkState>(new BulletNetworkState()
                    {
                            X = Turret.Muzzle.X,
                            Y = Turret.Muzzle.Y,
                            Angle = Turret.RotationZ,
                    });
                    timeToNextShot = ReloadSeconds;
                }
            }
        }

        
    }
}

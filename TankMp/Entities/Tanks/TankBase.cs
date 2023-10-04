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
        const float SecondsToLerpToState = 0.2f;
        const float ReloadSeconds = 0.2f;
        const float AnimationScaledSpeed = 10f;

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
            var rotationChangeThisFrame = MathHelper.Lerp(0, rotationToVelocityAngle, FrameLerp);
            var rotationPercentThisFrame = (float)(rotationChangeThisFrame / (Math.PI * 2f));
            var treadCircumference = (float)Math.Pow(TreadLeft.RelativePosition.Y, 2f);
            var linearVelocity = Velocity.Length();
            var velocityMagnitude = linearVelocity / MaxSpeed;

            Acceleration.X = (float)(Math.Cos(Controller.MovementAngle) * (MaxAcceleration * throttle));
            Acceleration.Y = (float)(Math.Sin(Controller.MovementAngle) * (MaxAcceleration * throttle));
            RotationZ += rotationChangeThisFrame;
            Turret.RelativeRotationZ = Controller.AimAngle;

            // base tread animation uses throttle, then scale speed down based on rotation direction
            FlatRedBall.Debugging.Debugger.CommandLineWrite("Tread left scale:" + rotationChangeThisFrame);
            TreadLeft.AnimationSpeed = velocityMagnitude * AnimationScaledSpeed;
            TreadLeft.AnimationSpeed *= Math.Sign(rotationChangeThisFrame) < 0 ? 1.35f : 0.75f;
            TreadRight.AnimationSpeed = velocityMagnitude * AnimationScaledSpeed;
            TreadRight.AnimationSpeed *= Math.Sign(rotationChangeThisFrame) > 0 ? 1.35f : 0.75f;

            if (LocallyOwned)
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

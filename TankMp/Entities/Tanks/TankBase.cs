using FlatRedBall.Math;
using TankMp.Input;
using NarfoxGameTools.Extensions;
using System;
using Microsoft.Xna.Framework;
using FlatRedBall;
using SignalRed.Common.Interfaces;
using TankMp.Models;

namespace TankMp.Entities.Tanks
{
    public partial class TankBase
    {
        const float MaxSpeed = 200;
        const float MaxDrag = 1.5f;
        const float MaxAcceleration = MaxSpeed / MaxDrag;
        const float SecondsToLerpToState = 0.15f;
        const float FireRatePerSecond = 2f;
        ITankController controller;

        float FrameLerp => TimeManager.SecondDifference / SecondsToLerpToState;
        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }


        public ITankController Controller { get; set; }

        

        private void CustomInitialize()
        {
            Drag = MaxDrag;
            TurretBaseInstance.ParentRotationChangesRotation = false;
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
            TurretBaseInstance.RelativeRotationZ = Controller.AimAngle;
        }

        
    }
}

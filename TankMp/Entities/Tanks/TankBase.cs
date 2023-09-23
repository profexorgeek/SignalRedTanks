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
    public partial class TankBase : INetworkEntity
    {
        const float MaxSpeed = 200;
        const float MaxDrag = 1.5f;
        const float MaxAcceleration = MaxSpeed / MaxDrag;
        const float RotationToTargetAngleSeconds = 0.15f;
        const float FireRatePerSecond = 2f;
        ITankController controller;

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

        public object GetState()
        {
            return new TankNetworkState
            {
                X = this.X,
                Y = this.Y,
                VelocityX = this.Velocity.X,
                VelocityY = this.Velocity.Y,
                AimAngle = this.TurretBaseInstance.RelativeRotationZ
            };
        }

        public void ApplyState(object networkState, bool isReckoning = false)
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
            var frameLerp = TimeManager.SecondDifference / RotationToTargetAngleSeconds;

            Acceleration.X = (float)(Math.Cos(Controller.MovementAngle) * (MaxAcceleration * throttle));
            Acceleration.Y = (float)(Math.Sin(Controller.MovementAngle) * (MaxAcceleration * throttle));
            RotationZ += MathHelper.Lerp(0, rotationToVelocityAngle, frameLerp);
            TurretBaseInstance.RelativeRotationZ = Controller.AimAngle;
        }

        
    }
}

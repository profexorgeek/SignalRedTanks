using FlatRedBall.Math;
using TankMp.Input;
using NarfoxGameTools.Extensions;
using System;
using Microsoft.Xna.Framework;
using FlatRedBall;

namespace TankMp.Entities.Tanks
{
    public partial class TankBase
    {
        const float MaxSpeed = 200;
        const float MaxDrag = 1.5f;
        const float MaxAcceleration = MaxSpeed / MaxDrag;
        const float RotationToTargetAngleSeconds = 0.15f;
        const float FireRatePerSecond = 2f;
        ITankController controller;


        public ITankController Controller
        {
            get
            {
                return controller;
            }
            set
            {
                controller = value;
                controller.Tank = this;
            }
        }

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
            var frameLerp = TimeManager.SecondDifference / RotationToTargetAngleSeconds;

            Acceleration.X = (float)(Math.Cos(Controller.MovementAngle) * (MaxAcceleration * throttle));
            Acceleration.Y = (float)(Math.Sin(Controller.MovementAngle) * (MaxAcceleration * throttle));
            RotationZ += MathHelper.Lerp(0, rotationToVelocityAngle, frameLerp);
            TurretBaseInstance.RelativeRotationZ = Controller.AimAngle;
        }
    }
}

using FlatRedBall;
using FlatRedBall.Math;
using Microsoft.Xna.Framework;
using NarfoxGameTools.Extensions;
using SignalRed.Client;
using System;
using System.Collections.Generic;
using TankMp.Entities.Bullets;
using TankMp.Input;
using TankMp.Models;

namespace TankMp.Entities.Tanks
{
    public partial class TankBase
    {

        float timeToNextShot = 0;
        bool destroyed;
        List<BulletBase> damageSources = new List<BulletBase>();


        public bool LocallyOwned => SignalRedClient.Instance.ClientId == Controller.OwnerClientId;
        float FrameLerp => TimeManager.SecondDifference / Globals.Tank_LerpSeconds;
        public ITankController Controller { get; set; }
        public float CurrentHealth { get; set; }
        public bool Destroyed
        {
            get
            {
                return destroyed;
            }
            set
            {
                destroyed = value;
                this.Visible = !destroyed;
            }
        }
        

        private void CustomInitialize()
        {
            Drag = Globals.Tank_Drag;
            Turret.ParentRotationChangesRotation = false;
            Destroyed = false;
        }

        private void CustomActivity()
        {
            DoInput();
        }

        public void Die()
        {
            Destroyed = true;
        }

        private void CustomDestroy()
        {
            damageSources.Clear();
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
            var linearVelocity = Velocity.Length();
            var velocityMagnitude = linearVelocity / Globals.Tank_Speed;

            Acceleration.X = (float)(Math.Cos(Controller.MovementAngle) * (Globals.Tank_Accel * throttle));
            Acceleration.Y = (float)(Math.Sin(Controller.MovementAngle) * (Globals.Tank_Accel * throttle));
            RotationZ += rotationChangeThisFrame;
            Turret.RelativeRotationZ = Controller.AimAngle;

            // base tread animation uses throttle, then scale speed down based on rotation direction
            FlatRedBall.Debugging.Debugger.CommandLineWrite("Tread left scale:" + rotationChangeThisFrame);
            TreadLeft.AnimationSpeed = velocityMagnitude * Globals.Tank_AnimationSpeedScale;
            TreadLeft.AnimationSpeed *= Math.Sign(rotationChangeThisFrame) < 0 ? 1.35f : 0.75f;
            TreadRight.AnimationSpeed = velocityMagnitude * Globals.Tank_AnimationSpeedScale;
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
                    timeToNextShot = Globals.Tank_ReloadSeconds;
                }
            }
        }

        
        public void TakeDamage(BulletBase bullet)
        {
            if(LocallyOwned && !damageSources.Contains(bullet))
            {
                CurrentHealth -= bullet.Damage;
                damageSources.Add(bullet);
            }
        }
    }
}

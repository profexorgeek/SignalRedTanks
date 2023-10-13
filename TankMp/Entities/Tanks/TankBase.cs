using FlatRedBall;
using FlatRedBall.Math;
using FlatRedBall.Screens;
using Microsoft.Xna.Framework;
using NarfoxGameTools.Extensions;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System;
using System.Collections.Generic;
using TankMp.Entities.Bullets;
using TankMp.Input;
using TankMp.Models;

namespace TankMp.Entities.Tanks
{
    public partial class TankBase : ISignalRedEntity<TankNetworkState>
    {

        float timeToNextShot = 0;
        List<BulletBase> damageSources = new List<BulletBase>();
        double lastStateUpdateTime;
        float timeToNextNetworkUpdate;

        public TankNetworkState CurrentState { get; private set; } = new TankNetworkState();
        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public bool LocallyOwned => SignalRedClient.Instance.ClientId == OwnerClientId;
        float FrameLerp => TimeManager.SecondDifference / Globals.Tank_LerpSeconds;
        public float CurrentHealth { get; set; }
        
        

        private void CustomInitialize()
        {
            Drag = Globals.Tank_Drag;
            Turret.ParentRotationChangesRotation = false;
        }

        private void CustomActivity()
        {
            DoInput();

            timeToNextNetworkUpdate -= TimeManager.SecondDifference;
            if(timeToNextNetworkUpdate <= 0)
            {
                SignalRedClient.Instance.UpdateEntity(this);
                timeToNextNetworkUpdate = Globals.Network_EntityUpdateSeconds;
            }
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
            // EARLY OUT: no state
            if (CurrentState == null) return;

            // first, interpolate to where we should be per the last state
            //var timeSinceLastState = TimeManager.CurrentTime - lastStateUpdateTime;
            //var targetX = CurrentState.X + (float)(CurrentState.VelocityX * timeSinceLastState);
            //var targetY = CurrentState.Y + (float)(CurrentState.VelocityY * timeSinceLastState);
            //X = MathHelper.Lerp(X, targetX, FrameLerp);
            //Y = MathHelper.Lerp(Y, targetY, FrameLerp);

            // now apply state input
            var throttle = CurrentState.MovementMagnitude.Clamp(-1f, 1f);
            var velocityAngle = (float)Math.Atan2(Velocity.Y, Velocity.X);
            var rotationToVelocityAngle = MathFunctions.AngleToAngle(RotationZ, velocityAngle);
            var rotationChangeThisFrame = MathHelper.Lerp(0, rotationToVelocityAngle, FrameLerp);
            var linearVelocity = Velocity.Length();
            var velocityMagnitude = linearVelocity / Globals.Tank_Speed;

            Acceleration.X = (float)(Math.Cos(CurrentState.MovementAngle) * (Globals.Tank_Accel * throttle));
            Acceleration.Y = (float)(Math.Sin(CurrentState.MovementAngle) * (Globals.Tank_Accel * throttle));
            RotationZ += rotationChangeThisFrame;
            Turret.RelativeRotationZ = CurrentState.AimAngle;

            // base tread animation uses throttle, then scale speed down based on rotation direction
            FlatRedBall.Debugging.Debugger.CommandLineWrite("Tread left scale:" + rotationChangeThisFrame);
            TreadLeft.AnimationSpeed = velocityMagnitude * Globals.Tank_AnimationSpeedScale;
            TreadLeft.AnimationSpeed *= Math.Sign(rotationChangeThisFrame) < 0 ? 1.35f : 0.75f;
            TreadRight.AnimationSpeed = velocityMagnitude * Globals.Tank_AnimationSpeedScale;
            TreadRight.AnimationSpeed *= Math.Sign(rotationChangeThisFrame) > 0 ? 1.35f : 0.75f;

            if (LocallyOwned)
            {
                timeToNextShot -= TimeManager.SecondDifference;
                if(timeToNextShot <= 0 && CurrentState.Firing)
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

        public void ApplyCreationState(TankNetworkState networkState, float deltaSeconds)
        {
            X = networkState.X;
            Y = networkState.Y;
            Velocity.X = networkState.VelocityX;
            Velocity.Y = networkState.VelocityY;
            CurrentHealth = networkState.CurrentHealth;
        }

        public void ApplyUpdateState(TankNetworkState networkState, float deltaSeconds, bool force = false)
        {
            // we only take state updates if this tank is NOT locally owned (a network controlled entity)
            // or the state is forced. The local controller will force local state updates
            if(LocallyOwned == false || force == true)
            {
                CurrentState = networkState;
                CurrentState.X = networkState.X + (networkState.VelocityX * deltaSeconds);
                CurrentState.Y = networkState.Y + (networkState.VelocityY * deltaSeconds);
                lastStateUpdateTime = TimeManager.CurrentTime - deltaSeconds;
            }
        }

        public void Destroy(TankNetworkState networkState, float deltaSeconds)
        {
            X = networkState.X;
            Y = networkState.Y;
            Destroy();
        }

        public TankNetworkState GetState()
        {
            return CurrentState;
        }
    }
}

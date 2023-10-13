using FlatRedBall;
using Microsoft.Xna.Framework;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System;
using TankMp.GumRuntimes;
using TankMp.Models;

namespace TankMp.Entities.Bullets
{
    public partial class BulletBase : ISignalRedEntity<BulletNetworkState>
    {
        float secondsUntilDeath;
        float secondsToNextUpdate;
        bool started = false;
        float FrameLerp = TimeManager.SecondDifference / Globals.Bullet_LerpSeconds;

        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public bool IsLocallyOwned => SignalRedClient.Instance.ClientId == OwnerClientId;
        public float Damage { get; set; } = 10f;


        private void CustomInitialize()
        {


        }

        private void CustomActivity()
        {
            if (started && IsLocallyOwned)
            {
                secondsUntilDeath -= TimeManager.SecondDifference;
                if(secondsUntilDeath <= 0)
                {
                    SignalRedClient.Instance.DeleteEntity(this);
                }

                // NOTE: bullets are only loosely synchronized based on
                // their start position. Normally we would send updates here
                // but all bullets move dumbly in a straight line and each client
                // only really cares if a bullet hits their locally-owned tank.
                // Therefore each client simulates bullets based on where it created
                // the bullet.
            }

            RotationZ = (float)Math.Atan2(Velocity.Y, Velocity.X);
        }

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }


        public void ApplyCreationState(BulletNetworkState networkState, float deltaSeconds)
        {
            var xSpeed = (float)(Math.Cos(networkState.Angle) * Globals.Bullet_Speed);
            var ySpeed = (float)(Math.Sin(networkState.Angle) * Globals.Bullet_Speed);
            X = networkState.X + (xSpeed * deltaSeconds);
            Y = networkState.Y + (ySpeed * deltaSeconds);
            Velocity.X = xSpeed;
            Velocity.Y = ySpeed;
            RotationZ = networkState.Angle;
            secondsUntilDeath = Globals.Bullet_LifeSeconds - deltaSeconds;
            started = true;
        }

        public void ApplyUpdateState(BulletNetworkState networkState, float deltaSeconds, bool force = false)
        {
            if(!IsLocallyOwned)
            {
                var xSpeed = (float)(Math.Cos(networkState.Angle) * Globals.Bullet_Speed);
                var ySpeed = (float)(Math.Sin(networkState.Angle) * Globals.Bullet_Speed);
                var xTarget = networkState.X + (xSpeed * deltaSeconds);
                var yTarget = networkState.Y + (ySpeed * deltaSeconds);
                X = MathHelper.Lerp(this.X, xTarget, FrameLerp);
                Y = MathHelper.Lerp(this.Y, yTarget, FrameLerp);
                RotationZ = networkState.Angle;
                Velocity.X = xSpeed;
                Velocity.Y = ySpeed;
            }
        }

        public BulletNetworkState GetState()
        {
            return new BulletNetworkState()
            {
                X = this.X,
                Y = this.Y,
                Angle = this.RotationZ,
            };
        }

        public void Destroy(BulletNetworkState networkState, float deltaSeconds)
        {
            // move this back to where the destroy occurred
            X = networkState.X;
            Y = networkState.Y;
            Destroy();
        }
    }
}

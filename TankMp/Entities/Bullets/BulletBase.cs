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
        bool started = false;

        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public float Damage { get; set; }


        private void CustomInitialize()
        {
            Damage = Globals.Bullet_Damage;
        }
        private void CustomActivity()
        {
            // NOTE: bullets are only loosely synchronized based on
            // their start position. Normally we would send updates here
            // but all bullets move dumbly in a straight line and each client
            // only really cares if a bullet hits their locally-owned tank.
            // Therefore each client simulates bullets based on where it created
            // the bullet.
            if (started)
            {
                RotationZ = (float)Math.Atan2(Velocity.Y, Velocity.X);
                secondsUntilDeath -= TimeManager.SecondDifference;
                if(secondsUntilDeath <= 0)
                {
                    Destroy();
                }

                
            }
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
            // NOOP: bullets do not take network updates, they are entirely locally simulated
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

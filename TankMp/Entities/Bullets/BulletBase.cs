using FlatRedBall;
using Microsoft.Xna.Framework;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System;
using TankMp.Models;

namespace TankMp.Entities.Bullets
{
    public partial class BulletBase : ISignalRedEntity<BulletNetworkState>
    {
        const float InterpolationSeconds = 0.15f;
        const float UpdateFreqSeconds = 0.5f;
        const float Speed = 300f;
        const float MaxLife = 4f;

        float secondsUntilDeath;
        float secondsToNextUpdate;
        bool started = false;
        float FrameLerp = TimeManager.SecondDifference / InterpolationSeconds;

        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public bool IsLocallyOwned => SignalRedClient.Instance.ClientId == OwnerClientId;

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

                secondsToNextUpdate -= TimeManager.SecondDifference;
                if(secondsToNextUpdate <= 0)
                {
                    SignalRedClient.Instance.UpdateEntity(this);
                    secondsToNextUpdate = UpdateFreqSeconds;
                }
                RotationZ = (float)Math.Atan2(Velocity.Y, Velocity.X);
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
            var xSpeed = (float)(Math.Cos(networkState.Angle) * Speed);
            var ySpeed = (float)(Math.Sin(networkState.Angle) * Speed);
            this.X = networkState.X + (xSpeed * deltaSeconds);
            this.Y = networkState.Y + (ySpeed * deltaSeconds);
            Velocity.X = xSpeed;
            Velocity.Y = ySpeed;
            secondsUntilDeath = MaxLife - deltaSeconds;
            started = true;
        }

        public void ApplyUpdateState(BulletNetworkState networkState, float deltaSeconds, bool force = false)
        {
            if(!IsLocallyOwned)
            {
                var xSpeed = (float)(Math.Cos(networkState.Angle) * Speed);
                var ySpeed = (float)(Math.Sin(networkState.Angle) * Speed);
                var xTarget = networkState.X + (xSpeed * deltaSeconds);
                var yTarget = networkState.Y + (ySpeed * deltaSeconds);
                this.X = MathHelper.Lerp(this.X, xTarget, FrameLerp);
                this.Y = MathHelper.Lerp(this.Y, yTarget, FrameLerp);
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
    }
}

using FlatRedBall;
using Microsoft.Xna.Framework;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System;
using System.Threading.Tasks;
using TankMp.Entities;
using TankMp.Entities.Tanks;
using TankMp.Models;

namespace TankMp.Input
{
    public class NetworkController : ITankController, ISignalRedEntity<TankNetworkState>
    {
        const float SecondsToLerpToState = 0.25f;

        float FrameLerp => TimeManager.SecondDifference / SecondsToLerpToState;
        TankNetworkState lastReceivedState = new TankNetworkState();
        double lastReceivedTime = TimeManager.CurrentTime;
        float lastReceivedDelta = float.MaxValue;
        TankBase tank;


        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public float MovementAngle => lastReceivedState.MovementAngle;
        public float MovementMagnitude => lastReceivedState.MovementMagnitude;
        public float AimAngle => lastReceivedState.AimAngle;
        public bool Firing => lastReceivedState.Firing;
        public bool IsDestroyed => Tank == null;
        public TankBase Tank
        {
            get
            {
                return tank;
            }
            set
            {
                tank = value;
                if (tank != null)
                {
                    tank.Controller = this;
                }
            }
        }

        

        public void Update()
        {
            // constantly interpolate towards where we think we should be according to the
            // last network update
            if(Tank != null)
            {
                var secondsSinceLastStateSent = (float)(TimeManager.CurrentTime - lastReceivedTime - lastReceivedDelta);
                var targetX = lastReceivedState.X + (lastReceivedState.VelocityX * secondsSinceLastStateSent);
                var targetY = lastReceivedState.Y + (lastReceivedState.VelocityY * secondsSinceLastStateSent);

                Tank.X = MathHelper.Lerp(Tank.X, targetX, FrameLerp);
                Tank.Y = MathHelper.Lerp(Tank.Y, targetY, FrameLerp);
            }
        }

        public void Destroy()
        {
            if (Tank != null)
            {
                Tank.Controller = null;
                Tank.Destroy();
                Tank = null;
            }
        }

        public void ApplyCreationState(TankNetworkState networkState, float deltaSeconds)
        {
            lastReceivedState = networkState;
            lastReceivedTime = TimeManager.CurrentTime;
            lastReceivedDelta = deltaSeconds;

            Tank.X = networkState.X;
            Tank.Y = networkState.Y;
        }

        public void ApplyUpdateState(TankNetworkState networkState, float deltaSeconds, bool force = false)
        {
            if (Tank != null)
            {
                lastReceivedState = networkState;
                lastReceivedTime = TimeManager.CurrentTime;
                lastReceivedDelta = deltaSeconds;
            }
        }

        public TankNetworkState GetState()
        {
            return new TankNetworkState()
            {
                MovementAngle = MovementAngle,
                MovementMagnitude = MovementMagnitude,
                AimAngle = AimAngle,
                Firing = Firing,
                X = Tank.X,
                Y = Tank.Y,
                VelocityX = Tank.Velocity.X,
                VelocityY = Tank.Velocity.Y,
            };
        }
    }
}

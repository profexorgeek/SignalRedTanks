using FlatRedBall;
using FlatRedBall.Gui;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework.Input;
using SignalRed.Client;
using System;
using System.Numerics;
using TankMp.Entities.Tanks;
using TankMp.Models;

namespace TankMp.Input
{
    public class KeyboardController : ITankController
    {
        const float NetworkUpdateFrequencySeconds = 0.5f;

        Vector2 movementVector = Vector2.Zero;
        TankNetworkState recentState = new TankNetworkState();
        float secondsToNextNetworkUpdate;
        TankBase tank;

        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }

        public float MovementAngle => (float)Math.Atan2(movementVector.Y, movementVector.X);
        public float MovementMagnitude => movementVector.Length().Clamp(-1f, 1f);
        public float AimAngle => (float)Math.Atan2(
            GuiManager.Cursor.WorldYAt(0) - Tank.Y,
            GuiManager.Cursor.WorldXAt(0) - Tank.X);
        public bool Firing => GuiManager.Cursor.PrimaryDown;
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
                if(tank != null)
                {
                    tank.Controller = this;
                }
            }
        }

        public object GetState()
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

        public void ApplyState(object networkState, bool force = false)
        {
            // NOOP:
            // keyboard controller is controlled by a local player and
            // the local player is authoritative over their own entity
            // so we do not apply incoming states unless they are a reckoning
            // or new creation state
            if(force && Tank != null)
            {
                var typedState = networkState as TankNetworkState;
                Tank.X = typedState.X;
                Tank.Y = typedState.Y;
                Tank.Velocity.X = typedState.VelocityX;
                Tank.Velocity.Y = typedState.VelocityY;
            }
        }

        public void Destroy()
        {
            if(Tank != null)
            {
                Tank.Controller = null;
                Tank.Destroy();
                Tank = null;
            }
        }

        public void Update()
        {
            var kb = InputManager.Keyboard;
            var cursor = GuiManager.Cursor;

            movementVector.X = kb.KeyDown(Keys.D) ? 1f : 0;
            movementVector.X = kb.KeyDown(Keys.A) ? -1f : movementVector.X;
            movementVector.Y = kb.KeyDown(Keys.W) ? 1f : 0;
            movementVector.Y = kb.KeyDown(Keys.S) ? -1f : movementVector.Y;

            SendNetworkUpdate();
        }

        public void SendNetworkUpdate()
        {
            if(SignalRedClient.Instance.Connected)
            {
                secondsToNextNetworkUpdate -= TimeManager.SecondDifference;
                if (secondsToNextNetworkUpdate < 0)
                {
                    SignalRedClient.Instance.UpdateEntity(this);
                    secondsToNextNetworkUpdate = NetworkUpdateFrequencySeconds;
                }
            }
        }
    }
}

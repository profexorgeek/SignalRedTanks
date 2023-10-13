using FlatRedBall;
using FlatRedBall.Gui;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework.Input;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System;
using System.Numerics;
using TankMp.Entities;
using TankMp.Entities.Tanks;
using TankMp.Factories;
using TankMp.Models;

namespace TankMp.Input
{
    public class KeyboardController : ITankController
    {
        public string OwnerClientId { get; set; }

        Vector2 movementVector = Vector2.Zero;
        TankNetworkState tankState = new TankNetworkState();

        public TankBase Tank { get; private set; }

        public void Update()
        {
            var kb = InputManager.Keyboard;
            var cursor = GuiManager.Cursor;

            // EARLY OUT: no tank target
            if(Tank == null) return;

            // get a vector based on keyboard input
            movementVector.X = kb.KeyDown(Keys.D) ? 1f : 0;
            movementVector.X = kb.KeyDown(Keys.A) ? -1f : movementVector.X;
            movementVector.Y = kb.KeyDown(Keys.W) ? 1f : 0;
            movementVector.Y = kb.KeyDown(Keys.S) ? -1f : movementVector.Y;

            // update the state based on our movement input
            tankState.MovementAngle = (float)Math.Atan2(movementVector.Y, movementVector.X);
            tankState.MovementMagnitude = movementVector.Length().Clamp(-1f, 1f);
            tankState.AimAngle = (float)Math.Atan2(
                GuiManager.Cursor.WorldYAt(0) - Tank.Y,
                GuiManager.Cursor.WorldXAt(0) - Tank.X);
            tankState.Firing = GuiManager.Cursor.PrimaryDown;

            // all other state properties are taken directly from the tank
            tankState.X = Tank.X;
            tankState.Y = Tank.Y;
            tankState.VelocityX = Tank.Velocity.X;
            tankState.VelocityY = Tank.Velocity.Y;

            // apply the state to the tank with zero time delta
            Tank.ApplyUpdateState(tankState, 0, true);
        }

        public void SetTargetTank(TankBase tank)
        {
            Tank = tank;
        }

        public void ClearTargetTank()
        {
            Tank = null;
        }
    }
}

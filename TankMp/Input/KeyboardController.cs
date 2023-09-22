using FlatRedBall.Gui;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using TankMp.Entities.Tanks;

namespace TankMp.Input
{
    public class KeyboardController : ITankController
    {
        Vector2 movementVector = Vector2.Zero;

        public float MovementAngle => (float)Math.Atan2(movementVector.Y, movementVector.X);
        public float MovementMagnitude => movementVector.Length().Clamp(-1f, 1f);
        public float AimAngle => (float)Math.Atan2(
            GuiManager.Cursor.WorldYAt(0) - Tank.Y,
            GuiManager.Cursor.WorldXAt(0) - Tank.X);
        public bool Firing => GuiManager.Cursor.PrimaryDown;
        public TankBase Tank { get; set; }

        public void Update()
        {
            var kb = InputManager.Keyboard;
            var cursor = GuiManager.Cursor;

            movementVector.X = kb.KeyDown(Keys.D) ? 1f : 0;
            movementVector.X = kb.KeyDown(Keys.A) ? -1f : movementVector.X;
            movementVector.Y = kb.KeyDown(Keys.W) ? 1f : 0;
            movementVector.Y = kb.KeyDown(Keys.S) ? -1f : movementVector.Y;
        }
    }
}

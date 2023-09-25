using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using TankMp.Entities.Bullets;
using TankMp.Entities.Tanks;
using TankMp.Screens;
using FlatRedBall.Math;

namespace TankMp.Screens
{
    public partial class GameScreen
    {
        void OnBulletsVsSolidCollided (Entities.Bullets.BulletBase bulletBase, FlatRedBall.TileCollisions.TileShapeCollection tileShapeCollection) 
        {
            bulletBase.RotationZ = (float)MathFunctions.RegulateAngle(bulletBase.RotationZ + Math.PI);
        }
        
    }
}

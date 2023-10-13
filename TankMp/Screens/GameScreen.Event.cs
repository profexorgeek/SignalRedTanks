using FlatRedBall.Math;
using SignalRed.Client;
using System;

namespace TankMp.Screens
{
    public partial class GameScreen
    {
        void OnBulletsVsSolidCollided (Entities.Bullets.BulletBase bulletBase, FlatRedBall.TileCollisions.TileShapeCollection tileShapeCollection) 
        {
            bulletBase.RotationZ = (float)MathFunctions.RegulateAngle(bulletBase.RotationZ + Math.PI);
        }
        void OnBulletsVsTanksCollided (Entities.Bullets.BulletBase bulletBase, Entities.Tanks.TankBase tankBase) 
        {
            if(bulletBase.OwnerClientId != tankBase.OwnerClientId)
            {
                tankBase.TakeDamage(bulletBase);
                bulletBase.Destroy();
            }
        }
        
    }
}

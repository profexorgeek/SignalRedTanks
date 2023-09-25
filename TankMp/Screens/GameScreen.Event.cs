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
            if(bulletBase.OwnerClientId != tankBase.Controller.OwnerClientId && tankBase.LocallyOwned)
            {
                // TODO: damage tank and mark that it has taken damage from this source
                // so it can't be damaged for more than a frame

                SignalRedClient.Instance.DeleteEntity(bulletBase);

                // destroy this bullet immediately because it hit our own tank
                bulletBase.Destroy();
            }
        }
        
    }
}

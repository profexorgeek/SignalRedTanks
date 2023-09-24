using SignalRed.Common.Interfaces;
using TankMp.Entities.Tanks;
using TankMp.Models;

namespace TankMp.Input
{
    public interface ITankController : ISignalRedEntity<TankNetworkState>
    {
        float MovementAngle { get;}
        float MovementMagnitude { get;}
        float AimAngle { get; }
        bool Firing { get;}
        bool IsDestroyed { get; }
        TankBase Tank { get; set; }

        void Update();
    }
}

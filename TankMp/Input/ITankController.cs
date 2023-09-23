using SignalRed.Common.Interfaces;
using TankMp.Entities.Tanks;

namespace TankMp.Input
{
    public interface ITankController : INetworkEntity
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

using SignalRed.Common.Interfaces;
using TankMp.Entities.Tanks;
using TankMp.Models;

namespace TankMp.Input
{
    public interface ITankController
    {
        TankBase Tank { get; }

        void Update();

        void SetTargetTank(TankBase tank);
        void ClearTargetTank();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TankMp.Entities.Tanks;

namespace TankMp.Input
{
    public interface ITankController
    {
        float MovementAngle { get;}
        float MovementMagnitude { get;}
        float AimAngle { get; }
        bool Firing { get;}
        TankBase Tank { get; set; }

        void Update();
    }
}

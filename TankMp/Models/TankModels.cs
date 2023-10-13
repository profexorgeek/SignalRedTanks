using TankMp.Entities.Tanks;
using TankMp.Input;

namespace TankMp.Models
{
    public class TankNetworkState
    {
        // current controls state
        public float MovementAngle { get; set; } = 0;
        public float MovementMagnitude { get; set; } = 0;
        public float AimAngle { get; set; } = 0;
        public bool Firing { get; set; } = false;

        // entity position state
        public float X { get; set; } = 0;
        public float Y { get; set; } = 0;
        public float VelocityX { get; set; } = 0;
        public float VelocityY { get; set; } = 0;

        // health state
        public float CurrentHealth { get; set; } = 0;
    }
}

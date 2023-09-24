using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System.Threading.Tasks;
using TankMp.Entities;
using TankMp.Entities.Tanks;
using TankMp.Models;

namespace TankMp.Input
{
    public class NetworkController : ITankController, ISignalRedEntity<TankNetworkState>
    {
        public TankNetworkState lastReceivedState = new TankNetworkState();
        TankBase tank;


        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public float MovementAngle => lastReceivedState.MovementAngle;
        public float MovementMagnitude => lastReceivedState.MovementMagnitude;
        public float AimAngle => lastReceivedState.AimAngle;
        public bool Firing => lastReceivedState.Firing;
        public bool IsDestroyed => Tank == null;
        public TankBase Tank
        {
            get
            {
                return tank;
            }
            set
            {
                tank = value;
                if (tank != null)
                {
                    tank.Controller = this;
                }
            }
        }

        

        public void Update()
        {
            // NOOP
        }

        public void Destroy()
        {
            if (Tank != null)
            {
                Tank.Controller = null;
                Tank.Destroy();
                Tank = null;
            }
        }

        public void ApplyCreationState(TankNetworkState networkState, float deltaSeconds)
        {
            ApplyUpdateState(networkState, deltaSeconds, true);
        }

        public void ApplyUpdateState(TankNetworkState networkState, float deltaSeconds, bool force = false)
        {
            if (Tank != null)
            {
                var typedState = networkState as TankNetworkState;
                lastReceivedState = typedState;

                Tank.X = typedState.X;
                Tank.Y = typedState.Y;
                Tank.Velocity.X = typedState.VelocityX;
                Tank.Velocity.Y = typedState.VelocityY;
            }
        }

        public TankNetworkState GetState()
        {
            return new TankNetworkState()
            {
                MovementAngle = MovementAngle,
                MovementMagnitude = MovementMagnitude,
                AimAngle = AimAngle,
                Firing = Firing,
                X = Tank.X,
                Y = Tank.Y,
                VelocityX = Tank.Velocity.X,
                VelocityY = Tank.Velocity.Y,
            };
        }
    }
}

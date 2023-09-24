using FlatRedBall.Forms.MVVM;
using SignalRed.Common.Interfaces;
using System;

namespace TankMp.Models
{
    public enum PlayerJoinStatus
    {
        Unknown = 0,
        Disconnected = 1,
        Connected = 2,
        Ready = 3,
        Playing = 4,
    }

    public class PlayerStatusViewModel : ViewModel, INetworkEntity
    {
        bool destroyed = false;

        public string Username { get => Get<string>(); set => Set(value); }
        public double Ping { get => Get<double>(); set => Set(value); }
        public PlayerJoinStatus CurrentStatus { get => Get<PlayerJoinStatus>(); set => Set(value); }
        public int Deaths { get => Get<int>(); set => Set(value); }
        public int Kills { get => Get<int>(); set => Set(value); }

        [DependsOn(nameof(CurrentStatus))]
        public bool IsReady => CurrentStatus == PlayerJoinStatus.Ready;
        [DependsOn(nameof(CurrentStatus))]
        public bool IsDisconnected => CurrentStatus == PlayerJoinStatus.Disconnected;
        [DependsOn(nameof(Ping))]
        [DependsOn(nameof(Username))]
        public string UsernameAndPing => $"{Username} ({Ping:F2})";

        public bool Destroyed => destroyed;

        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }
        public object GetState()
        {
            return new PlayerStatusNetworkState()
            {
                Username = Username,
                CurrentStatus = (int)CurrentStatus,
                Deaths = Deaths,
                Kills = Kills,
                Ping = Ping,
            };
        }
        public void ApplyState(object networkState, bool isReckoning = false)
        {
            var typedState = networkState as PlayerStatusNetworkState;
            if(typedState != null)
            {
                Username = typedState.Username;
                CurrentStatus = (PlayerJoinStatus)typedState.CurrentStatus;
                Deaths = typedState.Deaths;
                Kills = typedState.Kills;
                Ping = typedState.Ping;
            }
        }
        public void Destroy()
        {
            destroyed = true;
        }
    }

    public class PlayerStatusNetworkState
    {
        public string Username { get; set; }
        public int CurrentStatus { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
        public double Ping { get; set; }
    }
}

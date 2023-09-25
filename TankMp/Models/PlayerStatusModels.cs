using FlatRedBall.Forms.MVVM;
using SignalRed.Common.Interfaces;
using System;
using TankMp.Entities;

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

    public class PlayerStatusViewModel : ViewModel, ISignalRedEntity<PlayerStatusNetworkState>
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

        

        public void ApplyCreationState(PlayerStatusNetworkState networkState, float deltaSeconds)
        {
            ApplyUpdateState(networkState, deltaSeconds, true);
        }

        public void ApplyUpdateState(PlayerStatusNetworkState networkState, float deltaSeconds, bool force = false)
        {
            Username = networkState.Username;
            CurrentStatus = (PlayerJoinStatus)networkState.CurrentStatus;
            Deaths = networkState.Deaths;
            Kills = networkState.Kills;
            Ping = networkState.Ping;
        }

        public PlayerStatusNetworkState GetState()
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

        public void Destroy(PlayerStatusNetworkState networkState, float deltaSeconds)
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

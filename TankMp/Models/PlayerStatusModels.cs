using FlatRedBall.Forms.MVVM;
using SignalRed.Common.Messages;

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

    public class PlayerStatusViewModel : ViewModel
    {
        public string ClientId { get => Get<string>(); set => Set(value); }
        public string Name { get => Get<string>(); set => Set(value); }
        public PlayerJoinStatus CurrentStatus { get => Get<PlayerJoinStatus>(); set => Set(value); }
        public int Deaths { get => Get<int>(); set => Set(value); }
        public int Kills { get => Get<int>(); set => Set(value); }


        [DependsOn(nameof(CurrentStatus))]
        public bool IsReady => CurrentStatus == PlayerJoinStatus.Ready;

        [DependsOn(nameof(CurrentStatus))]
        public bool IsDisconnected => CurrentStatus == PlayerJoinStatus.Disconnected;
        

        public static PlayerStatusViewModel CreateFromUserMessage(UserMessage message)
        {
            return new PlayerStatusViewModel()
            {
                ClientId = message.ClientId,
                Name = message.UserName,
                CurrentStatus = PlayerJoinStatus.Connected,
                Deaths = 0,
                Kills = 0,
            };
        }
        public static PlayerStatusViewModel CreateFromEntityMessage(EntityMessage message)
        {
            var model = message.GetPayload<PlayerStatusNetworkModel>();
            var vm = new PlayerStatusViewModel()
            {
                ClientId = message.ClientId
            };
            vm.UpdateFromEntityMessage(message);
            return vm;
        }
        public void UpdateFromEntityMessage(EntityMessage message)
        {
            var model = message.GetPayload<PlayerStatusNetworkModel>();
            ClientId = model.ClientId;
            Name = model.Name;
            CurrentStatus = (PlayerJoinStatus)model.CurrentStatus;
            Deaths = model.Deaths;
            Kills = model.Kills;
        }
        public void UpdateFromUserMessage(UserMessage message)
        {
            Name = message.UserName;
        }
        public PlayerStatusNetworkModel ToNetworkModel()
        {
            return new PlayerStatusNetworkModel()
            {
                ClientId = ClientId,
                Name = Name,
                CurrentStatus = (int)CurrentStatus,
                Deaths = Deaths,
                Kills = Kills
            };
        }
    }

    public class PlayerStatusNetworkModel
    {
        public string ClientId { get; set; }
        public string Name { get; set; }
        public int CurrentStatus { get; set; }
        public int Deaths { get; set; }
        public int Kills { get; set; }
    }
}

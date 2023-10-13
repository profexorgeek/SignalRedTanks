using FlatRedBall.Forms.MVVM;
using SignalRed.Client;
using SignalRed.Common.Messages;
using System.Collections.ObjectModel;
using System.Linq;

namespace TankMp.Models
{
    public class GameStateViewModel : ViewModel
    {
        public ObservableCollection<PlayerStatusViewModel> Players { get; set; }
        public ObservableCollection<string> Messages { get; set; }

        public string LocalUsername { get => Get<string>(); set => Set(value); }
        public string CurrentChat { get => Get<string>(); set => Set(value); }
        public bool IsGameStartable { get => Get<bool>(); private set => Set(value); }
        public PlayerStatusViewModel Winner => Players.OrderByDescending(p => p.Kills).First();

        [DependsOn(nameof(Players))]
        public PlayerStatusViewModel LocalPlayer => Players.Where(p => p.OwnerClientId == SignalRedClient.Instance.ClientId).FirstOrDefault();

        public GameStateViewModel()
        {
            Players = new ObservableCollection<PlayerStatusViewModel>();
            Messages = new ObservableCollection<string>();

            UpdateStartableStatus();
        }

        public void UpdateStartableStatus()
        {
            IsGameStartable = Players.Count > 0 && Players.All(p => p.CurrentStatus == PlayerJoinStatus.Ready);
        }

        public void GrantKillCredit(string clientId)
        {
            var player = Players.FirstOrDefault(p => p.OwnerClientId == clientId);
            if(player != null)
            {
                player.Kills += 1;
            }
        }
    }
}

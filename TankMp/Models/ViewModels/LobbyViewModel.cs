using FlatRedBall.Forms.MVVM;
using SignalRed.Common.Messages;
using System.Collections.ObjectModel;
using System.Linq;

namespace TankMp.Models.ViewModels
{
    public class LobbyViewModel : ViewModel
    {
        public ObservableCollection<PlayerStatusViewModel> Players { get; set; }
        public ObservableCollection<string> Chats { get; set; }

        public string CurrentChat { get => Get<string>(); set => Set(value); }

        public bool IsGameStartable { get => Get<bool>(); private set => Set(value); }

        public LobbyViewModel()
        {
            Players = new ObservableCollection<PlayerStatusViewModel>();
            Chats = new ObservableCollection<string>();

            UpdateStartableStatus();
        }

        public void AddOrUpdatePlayerFromNetworkMessage(UserMessage message)
        {
            var existing = Players.Where(p => p.ClientId == message.ClientId).FirstOrDefault();
            if(existing == null)
            {
                var plyr = PlayerStatusViewModel.CreateFromUserMessage(message);
                Players.Add(plyr);
            }
            else
            {
                existing.UpdateFromUserMessage(message);
            }
        }

        public void SetPlayerReadyStatus(string clientId, bool isReady)
        {
            var plyr = Players.Where(p => p.ClientId == clientId).FirstOrDefault();
            if(plyr != null)
            {
                plyr.CurrentStatus = PlayerJoinStatus.Ready;
            }

            UpdateStartableStatus();
        }

        public void TryRemovePlayerWithClientId(string id)
        {
            var existing = Players.Where(p => p.ClientId == id).FirstOrDefault();
            if(existing != null)
            {
                Players.Remove(existing);
            }
        }

        public void UpdateStartableStatus()
        {
            IsGameStartable = Players.Count > 1 && Players.All(p => p.CurrentStatus == PlayerJoinStatus.Ready);
        }
    }
}

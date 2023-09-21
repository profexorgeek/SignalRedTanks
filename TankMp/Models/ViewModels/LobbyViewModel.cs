using FlatRedBall.Forms.MVVM;
using SignalRed.Common.Messages;
using System.Collections.ObjectModel;
using System.Linq;

namespace TankMp.Models.ViewModels
{
    public class PlayerStatusViewModel : ViewModel
    {
        public string Id { get => Get<string>(); set => Set(value); }
        public string Name { get => Get<string>(); set => Set(value); }
        public bool IsReady { get => Get<bool>(); set => Set(value); }
        public bool IsDisconnected { get => Get<bool>(); set => Set(value); }
    }

    public class LobbyViewModel : ViewModel
    {
        public ObservableCollection<PlayerStatusViewModel> Players { get; set; }
        public ObservableCollection<string> Chats { get; set; }

        public string CurrentChat { get => Get<string>(); set => Set(value); }

        public LobbyViewModel()
        {
            Players = new ObservableCollection<PlayerStatusViewModel>();
            Chats = new ObservableCollection<string>();
        }

        public void AddOrUpdatePlayerFromNetworkMessage(UserMessage message)
        {
            // NOTE: if we got a network message with a user, we know they are
            // connected so this always sets IsDisconnected to false

            var existing = Players.Where(p => p.Id == message.ClientId).FirstOrDefault();
            if(existing == null)
            {
                var plyr = new PlayerStatusViewModel
                {
                    Id = message.ClientId,
                    Name = message.UserName,
                    IsReady = false,
                    IsDisconnected = false,
                };
                Players.Add(plyr);
            }
            else
            {
                existing.Name = message.UserName;
                existing.IsDisconnected = false;
            }
        }

        public void SetPlayerReadyStatus(string clientId, bool isReady)
        {
            var plyr = Players.Where(p => p.Id == clientId).FirstOrDefault();
            if(plyr != null)
            {
                plyr.IsReady = isReady;
            }
        }

        public void TryRemovePlayerWithClientId(string id)
        {
            var existing = Players.Where(p => p.Id == id).FirstOrDefault();
            if(existing != null)
            {
                Players.Remove(existing);
            }
        }
    }
}

using FlatRedBall.Forms.MVVM;
using System.Collections.ObjectModel;

namespace TankMp.Models
{
    public class LobbyViewModel : ViewModel
    {
        public ObservableCollection<string> Players { get; set; }
        public ObservableCollection<string> Chats { get; set; }

        public string CurrentChat { get => Get<string>(); set => Set(value); }

        public LobbyViewModel()
        {
            Players = new ObservableCollection<string>();
            Chats = new ObservableCollection<string>();
        }
    }
}

using FlatRedBall.Forms.Controls;
using TankMp.GumRuntimes.Controls;
using TankMp.Models.ViewModels;

namespace TankMp.GumRuntimes.Menus
{
    public partial class ServerLobbyRuntime
    {
        public LobbyViewModel ViewModel => BindingContext as LobbyViewModel;
        partial void CustomInitialize () 
        {
            PlayersList.FormsControl.ListBoxItemGumType = typeof(ListBoxItemPlayerRuntime);
            PlayersList.FormsControl.SetBinding(nameof(ListBox.Items), nameof(ViewModel.Players));
            
            ChatsList.FormsControl.SetBinding(nameof(ListBox.Items), nameof(ViewModel.Chats));
            
            ChatEntryTextBox.FormsControl.SetBinding(nameof(TextBox.Text), nameof(ViewModel.CurrentChat));
        }
    }
}

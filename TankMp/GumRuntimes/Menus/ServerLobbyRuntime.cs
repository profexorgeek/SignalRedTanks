using FlatRedBall.Forms.Controls;
using TankMp.GumRuntimes.Controls;
using TankMp.Models;

namespace TankMp.GumRuntimes.Menus
{
    public partial class ServerLobbyRuntime
    {
        public GameStateViewModel ViewModel => BindingContext as GameStateViewModel;
        partial void CustomInitialize () 
        {
            PlayersList.FormsControl.ListBoxItemGumType = typeof(ListBoxItemPlayerRuntime);
            PlayersList.FormsControl.SetBinding(nameof(ListBox.Items), nameof(ViewModel.Players));
            ChatsList.FormsControl.SetBinding(nameof(ListBox.Items), nameof(ViewModel.Messages));
            ChatEntryTextBox.FormsControl.SetBinding(nameof(TextBox.Text), nameof(ViewModel.CurrentChat));
            StartButton.FormsControl.SetBinding(nameof(StartButton.FormsControl.IsEnabled), nameof(ViewModel.IsGameStartable));
        }
    }
}

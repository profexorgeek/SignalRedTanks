using TankMp.Models;

namespace TankMp.GumRuntimes.Controls
{
    public partial class ListBoxItemPlayerRuntime
    {
        public PlayerStatusViewModel ViewModel => BindingContext as PlayerStatusViewModel;
        partial void CustomInitialize ()
        {
            TextInstance.SetBinding(nameof(TextRuntime.Text), nameof(ViewModel.Name));
            ReadyIcon.SetBinding(nameof(ReadyIcon.Visible), nameof(ViewModel.IsReady));
            NetworkIcon.SetBinding(nameof(NetworkIcon.Visible), nameof(ViewModel.IsDisconnected));
        }
    }
}

using SignalRed.Client;
using System;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class ConnectToServer
    {
        void CustomInitialize()
        {
            JoinButton.FormsControl.Click += TryJoinServer;
            CancelButton.FormsControl.Click += CancelJoinServer;

            // TODO, DEBUG: remove this, just makes testing faster for now
            IpTextbox.FormsControl.Text = "http://localhost:5000";
        }
        void CustomDestroy() { }
        void CustomActivity(bool firstTimeCalled) { }
        static void CustomLoadStaticContent(string contentManagerName) { }

        void TryJoinServer(object sender, EventArgs e)
        {
            var url = IpTextbox.FormsControl.Text;
            var username = UsernameTextbox.FormsControl.Text;

            if(!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(url))
            {
                GameStateService.Instance.GameState.LocalUsername = username;
                SignalRedClient.Instance.Connect(url, () =>
                {
                    MoveToScreen(typeof(ServerLobby).FullName);
                });
            }
        }
        void CancelJoinServer(object sender, EventArgs e)
        {
            // NOOP yet!
        }
    }
}

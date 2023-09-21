using SignalRed.Client;
using System;

namespace TankMp.Screens
{
    public partial class ConnectToServer
    {
        void CustomInitialize()
        {
            JoinButton.FormsControl.Click += TryJoinServer;
            CancelButton.FormsControl.Click += CancelJoinServer;
            SignalRedClient.Instance.ConnectionOpened += Connected;

            // TODO: remove this, just makes testing faster for now
            IpTextbox.FormsControl.Text = "http://localhost:5000";
        }
        void CustomDestroy()
        {
            SignalRedClient.Instance.ConnectionOpened -= Connected;
        }
        void CustomActivity(bool firstTimeCalled) { }
        static void CustomLoadStaticContent(string contentManagerName) { }

        async void TryJoinServer(object sender, EventArgs e)
        {
            var url = IpTextbox.FormsControl.Text;
            var username = UsernameTextbox.FormsControl.Text;
            if(!string.IsNullOrEmpty(url))
            {
                await SignalRedClient.Instance.Connect(url, username);
            }
        }
        void Connected()
        {
            MoveToScreen(typeof(ServerLobby).FullName);
        }
        void CancelJoinServer(object sender, EventArgs e)
        {
            // NOOP yet!
        }

    }
}

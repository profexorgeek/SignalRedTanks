using SignalRed.Client;
using System;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class ConnectToServer
    {

        void CustomInitialize()
        {
            GameClientService.Instance.Initialize();


            JoinButton.FormsControl.Click += TryJoinServer;

            CancelButton.FormsControl.Click += (s, e) =>
            {

            };

        }

        void CustomActivity(bool firstTimeCalled)
        {


        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        async void TryJoinServer(object sender, EventArgs e)
        {
            var url = IpTextbox.FormsControl.Text;
            var username = UsernameTextbox.FormsControl.Text;
            if(!string.IsNullOrEmpty(url))
            {
                await SRClient.Instance.Connect(url, username);
                await SRClient.Instance.RequestScreenTransition(typeof(ServerLobby).FullName);
            }
        }

    }
}

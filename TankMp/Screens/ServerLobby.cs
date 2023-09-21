using FlatRedBall.Input;
using SignalRed.Client;
using System.Threading.Tasks;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class ServerLobby
    {

        void CustomInitialize()
        {
            ServerLobbyGum.ServerLobbyMenu.BindingContext = GameClientService.Instance.LobbyViewModel;
            ServerLobbyGum.FormsControl.ServerLobbyMenu.SendChatButton.Click += async (s, e) =>
            {
                await SendChat();
            };

            // request an updated list of users
            SRClient.Instance.ReckonUsers();
        }

        void CustomActivity(bool firstTimeCalled)
        {
            if(InputManager.Keyboard.KeyReleased(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                SendChat();
            }
        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        async Task SendChat()
        {
            var chat = GameClientService.Instance.LobbyViewModel.CurrentChat;
            await SRClient.Instance.SendChat(chat);

            GameClientService.Instance.LobbyViewModel.CurrentChat = "";
        }

    }
}

using SignalRed.Client;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class EndgameScreen
    {

        void CustomInitialize()
        {
            var winner = GameStateService.Instance.GameState.Winner;
            EndgameScreenGum.EndgameWinner.Text = winner != null ? winner.Username : "";
            EndgameScreenGum.ReturnToLobbyButton.FormsControl.Click += (s, e) => SignalRedClient.Instance.RequestScreenTransition(typeof(ServerLobby).FullName, true);
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

    }
}

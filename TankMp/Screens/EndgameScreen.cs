using TankMp.Services;

namespace TankMp.Screens
{
    public partial class EndgameScreen
    {

        void CustomInitialize()
        {
            EndgameScreenGum.EndgameWinner.Text = GameStateService.Instance.GameState.Winner.Username;
            EndgameScreenGum.ReturnToLobbyButton.FormsControl.Click += (s, e) => MoveToScreen(typeof(ServerLobby).FullName);
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

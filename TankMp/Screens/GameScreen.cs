using FlatRedBall;
using FlatRedBall.Entities;
using SignalRed.Client;
using System.Collections.Generic;
using TankMp.Entities.Tanks;
using TankMp.Factories;
using TankMp.Input;

namespace TankMp.Screens
{
    public partial class GameScreen
    {
        const float ReckonFrequencySeconds = 0.5f;

        public bool IsInNetworkedGame => SignalRedClient.Instance.Connected;
        public TankBase LocalPlayer { get; set; }



        void CustomInitialize()
        {
            if(IsInNetworkedGame)
            {
                InitializeInNetworkedMode();
            }
            else
            {
                InitializeInSoloTestMode();
            }
        }

        void CustomDestroy()
        {


        }

        void CustomActivity(bool firstTimeCalled)
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }


        void InitializeInSoloTestMode()
        {
            LocalPlayer = TankBaseFactory.CreateNew(Map.Width / 2f, Map.Height / -2f);
            CameraController.Camera = Camera.Main;
            CameraController.Target = LocalPlayer;
            LocalPlayer.Controller = new KeyboardController();
        }

        void DestroyInSoloTestMode()
        {

        }

        void InitializeInNetworkedMode()
        {
            
        }
    }
}

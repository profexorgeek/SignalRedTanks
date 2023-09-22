using TankMp.Models;

namespace TankMp.Services
{
    public class GameStateService
    {
        private static GameStateService instance;
        private bool initialized = false;

        public static GameStateService Instance => instance ?? (instance = new GameStateService());
        public bool Initialized => initialized;
        public GameStateViewModel GameState { get; set; }

        private GameStateService() { }

        public void Initialize()
        {
            initialized = true;
            GameState = new GameStateViewModel();
        }
    }
}

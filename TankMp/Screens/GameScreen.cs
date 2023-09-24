using FlatRedBall;
using FlatRedBall.Instructions;
using NarfoxGameTools.Extensions;
using SignalRed.Client;
using SignalRed.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using TankMp.Factories;
using TankMp.Input;
using TankMp.Models;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class GameScreen
    {
        const float ReckonFrequencySeconds = 0.5f;
        const float RespawnSeconds = 5;

        public bool IsInNetworkedGame => SignalRedClient.Instance.Connected;
        float timeToRespawn = 0;
        Random rand = new Random();
        List<ITankController> controllers = new List<ITankController>();
        ITankController localController;

        GameStateViewModel GameState => GameStateService.Instance.GameState;

        void CustomInitialize()
        {
            RequestTankSpawn();
        }

        

        void CustomDestroy()
        {


        }

        void CustomActivity(bool firstTimeCalled)
        {
            if(localController != null && localController.IsDestroyed)
            {
                timeToRespawn -= TimeManager.SecondDifference;
                if(timeToRespawn < 0)
                {
                    //RequestTankSpawn();
                }
            }
        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        void RequestTankSpawn()
        {
            var state = new TankNetworkState()
            {
                MovementAngle = 0,
                MovementMagnitude = 0,
                AimAngle = 0,
                Firing = false,
                X = Map.Left + rand.InRange(50, Map.Width - 50),
                Y = Map.Top - rand.InRange(50, Map.Height - 50),
                VelocityX = 0,
                VelocityY = 0,
            };
            SignalRedClient.Instance.CreateEntity(state);
        }


        protected override void CreateEntity(EntityStateMessage message)
        {
            if (message.StateType == typeof(TankNetworkState).FullName)
            {
                var state = message.GetState<TankNetworkState>();
                var controller = GetControllerForEntityId(message.EntityId);
                controller = controller ?? CreateController(message.OwnerClientId, message.EntityId);
                if (controller.IsDestroyed)
                {
                    _ = InstructionManager.DoOnMainThreadAsync(() =>
                    {
                        TrySpawnTankForController(controller);
                        controller.ApplyState(state, true);
                    });
                }
                else
                {
                    controller.ApplyState(state);
                }
            }
        }

        protected override void UpdateEntity(EntityStateMessage message, bool isReckonMessage = false)
        {
            if (message.StateType == typeof(TankNetworkState).FullName)
            {
                var state = message.GetState<TankNetworkState>();
                var controller = GetControllerForEntityId(message.EntityId);

                if (controller != null)
                {
                    controller.ApplyState(state);
                }
            }
        }

        protected override void DeleteEntity(EntityStateMessage message)
        {
            if(message.StateType == typeof(TankNetworkState).FullName)
            {
                var state = message.GetState<TankNetworkState>();
                var controller = GetControllerForEntityId(message.EntityId);

                if(controller != null)
                {
                    // NOTE: this doesn't actually destroy the controller, it tells the controller
                    // to destroy it's tank. The controller will stick around and respawn
                    controller.Destroy();

                    bool isLocal = controller.OwnerClientId == SignalRedClient.Instance.ClientId;
                    if (isLocal)
                    {
                        timeToRespawn = RespawnSeconds;
                    }
                }
            }
        }


        ITankController? GetControllerForEntityId(string id)
        {
            return controllers.Where(c => c.EntityId == id).FirstOrDefault();
        }

        /// <summary>
        /// Creates a new controller and adds it to the list. If the controller
        /// is owned by this client, it will be a local controller type such as
        /// Keyboard. Otherwise it'll be a NetworkController. This also sets
        /// the localController field
        /// </summary>
        /// <param name="ownerId">The ClientId of the entity owner</param>
        /// <param name="entityId">The unique ID of the entity on the network</param>
        /// <returns>A valid ITankController</returns>
        ITankController CreateController(string ownerId, string entityId)
        {
            ITankController ctrl;
            if(ownerId == GameState.LocalPlayer.OwnerClientId)
            {
                ctrl = new KeyboardController();
                localController = ctrl;
            }
            else
            {
                ctrl = new NetworkController();
            }

            ctrl.OwnerClientId = ownerId;
            ctrl.EntityId = entityId;
            controllers.Add(ctrl);

            return ctrl;
        }

        void TrySpawnTankForController(ITankController controller)
        {
            var tank = TankBaseFactory.CreateNew();
            controller.Tank = tank;

            if(controller == localController)
            {
                CameraController.Target = tank;
            }
        }

    }
}

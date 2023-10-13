using FlatRedBall;
using FlatRedBall.Instructions;
using FlatRedBall.Screens;
using NarfoxGameTools.Extensions;
using SignalRed.Client;
using SignalRed.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using TankMp.Entities.Bullets;
using TankMp.Entities.Tanks;
using TankMp.Factories;
using TankMp.Input;
using TankMp.Models;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class GameScreen
    {
        public bool IsInNetworkedGame => SignalRedClient.Instance.Connected;
        float timeToRespawn = 0;
        Random rand = new Random();
        ITankController localController;
        TankBase localTank;

        public static GameScreen Current => ScreenManager.CurrentScreen as GameScreen;

        GameStateViewModel GameState => GameStateService.Instance.GameState;

        void CustomInitialize()
        {
            localController = new KeyboardController();
        }
        void CustomDestroy() { }
        void CustomActivity(bool firstTimeCalled)
        {
            localController?.Update();

            if(localTank == null)
            {
                timeToRespawn -= TimeManager.SecondDifference;
                if(timeToRespawn < 0)
                {
                    var respawnTankState = new TankNetworkState
                    {
                        // TODO: get a respawn location!
                        X = 150,
                        Y = -50,
                        VelocityX = 0,
                        VelocityY = 0,
                        AimAngle = 0,
                        MovementAngle = 0,
                        MovementMagnitude = 0,
                        Firing = false,
                        CurrentHealth = Globals.Tank_Health,
                    };
                    SignalRedClient.Instance.CreateEntity(respawnTankState);
                    timeToRespawn = Globals.Tank_RespawnSeconds;
                }
            }
            else
            {
                if(localTank.CurrentHealth <= 0)
                {
                    SignalRedClient.Instance.DeleteEntity(localTank);
                    DeleteTank(localTank);
                }
            }

            if(GameStateService.Instance.GameState.LocalPlayer.Kills >= Globals.Game_KillsToWin)
            {
                SignalRedClient.Instance.RequestScreenTransition(typeof(EndgameScreen).FullName);
            }
        }
        static void CustomLoadStaticContent(string contentManagerName) { }

        protected override void ApplyGenericMessage(GenericMessage message)
        {
            if(message.MessageKey == Globals.Network_KillCreditKey)
            {
                GameStateService.Instance.GameState.GrantKillCredit(message.MessageValue);
            }
        }
        protected override void CreateEntity(EntityStateMessage message)
        {
            if (message.StateType == typeof(TankNetworkState).FullName) CreateTank(message);

            else if (message.StateType == typeof(BulletNetworkState).FullName) CreateBullet(message);
        }
        protected override void UpdateEntity(EntityStateMessage message, bool isReckonMessage = false)
        {
            if (message.StateType == typeof(TankNetworkState).FullName) UpdateTank(message);

            else if (message.StateType == typeof(BulletNetworkState).FullName) UpdateBullet(message);

            else if (message.StateType == typeof(PlayerStatusNetworkState).FullName) UpdatePlayer(message);
        }
        protected override void DeleteEntity(EntityStateMessage message)
        {
            if (message.StateType == typeof(TankNetworkState).FullName) DeleteTank(message);

            else if (message.StateType == typeof(BulletNetworkState).FullName) DeleteBullet(message);
        }


        void UpdatePlayer(EntityStateMessage message)
        {
            // EARLY OUT: we don't accept updates to our player from the network
            if (message.OwnerClientId != SignalRedClient.Instance.ClientId) return;

            var player = GameStateService.Instance.GameState.Players.FirstOrDefault(p => p.EntityId == message.EntityId);
            if(player != null)
            {
                var state = message.GetState<PlayerStatusNetworkState>();
                player.ApplyUpdateState(state, message.DeltaSeconds);
            }
        }

        TankBase CreateTank(EntityStateMessage message)
        {
            var state = message.GetState<TankNetworkState>();
            var tank = TankBaseFactory.CreateNew(state.X, state.Y);
            tank.OwnerClientId = message.OwnerClientId;
            tank.EntityId = message.EntityId;
            tank.ApplyCreationState(state, message.DeltaSeconds);

            if(tank.OwnerClientId == SignalRedClient.Instance.ClientId)
            {
                localController.SetTargetTank(tank);
                localTank = tank;
                CameraController.Target = localTank;
            }

            return tank;
        }
        void UpdateTank(EntityStateMessage message)
        {
            // EARLY OUT: we don't take network updates for entities we own
            if (message.OwnerClientId == SignalRedClient.Instance.ClientId)
            {
                return;
            }

            var existing = TankBaseList.FirstOrDefault(t => t.EntityId == message.EntityId);
            if (existing != null)
            {
                var state = message.GetState<TankNetworkState>();
                existing.ApplyUpdateState(state, message.DeltaSeconds);
            }
        }
        void DeleteTank(EntityStateMessage message)
        {
            var tank = TankBaseList.FirstOrDefault(t => t.EntityId == message.EntityId);
            if(tank != null)
            {
                DeleteTank(tank);
            }
        }
        void DeleteTank(TankBase tank)
        {
            if (tank == localTank)
            {
                localTank = null;
                localController.ClearTargetTank();
                CameraController.Target = null;

                var plyr = GameStateService.Instance.GameState.LocalPlayer;
                plyr.Deaths += 1;
                SignalRedClient.Instance.UpdateEntity(plyr);
            }
            tank.Destroy();
        }

        BulletBase CreateBullet(EntityStateMessage message)
        {
            var state = message.GetState<BulletNetworkState>();
            var bullet = BulletBaseFactory.CreateNew();
            bullet.EntityId = message.EntityId;
            bullet.OwnerClientId = message.OwnerClientId;
            bullet.ApplyCreationState(state, message.DeltaSeconds);
            return bullet;
        }
        void UpdateBullet(EntityStateMessage message)
        {
            // NOOP: we don't update bullets from the network because they are locally simulated
        }
        void DeleteBullet(EntityStateMessage message)
        {
            // NOOP: we don't destroy bullets from the network because they are locally simulated.
        }
    }
}

using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Screens;
using SignalRed.Client;
using SignalRed.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using TankMp.Models;
using TankMp.Services;

namespace TankMp.Screens
{
    public partial class ServerLobby
    {
        const string MessageKey = "LobbyMessage";
        const float ReckonFrequencySeconds = 5f;
        const float PingUpdateFrequency = 0.5f;

        float secondsToNextReckon = 0f;
        float secondsToNextPingUpdate = 0f;

        GameStateViewModel GameState => GameStateService.Instance.GameState;

        void CustomInitialize()
        {
            ServerLobbyGum.ServerLobbyMenu.BindingContext = GameState;
            ServerLobbyGum.FormsControl.ServerLobbyMenu.SendChatButton.Click += (s,e) =>  SendChat();
            ServerLobbyGum.FormsControl.ServerLobbyMenu.ReadyButton.Click += (s, e) => SendReadyStatus(true);
            ServerLobbyGum.FormsControl.ServerLobbyMenu.LeaveButton.Click += (s, e) => LeaveServer();
            ServerLobbyGum.FormsControl.ServerLobbyMenu.StartButton.Click += (s, e) => RequestStartGame();

            // clear any players and messages that existed before, we'll get a fresh list from the server
            GameState.Messages.Clear();
            GameState.Players.Clear();

            var state = new PlayerStatusNetworkState()
            {
                CurrentStatus = (int)PlayerJoinStatus.Connected,
                Username = GameState.LocalUsername,
                Kills = 0,
                Deaths = 0,
            };
            SignalRedClient.Instance.CreateEntity(state);
        }
        void CustomDestroy() { }
        void CustomActivity(bool firstTimeCalled)
        {
            if (InputManager.Keyboard.KeyReleased(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                SendChat();
            }

            secondsToNextReckon -= TimeManager.SecondDifference;
            if(secondsToNextReckon < 0)
            {
                SignalRedClient.Instance.ReckonEntities();
                secondsToNextReckon = ReckonFrequencySeconds;
            }

            secondsToNextPingUpdate -= TimeManager.SecondDifference;
            if(secondsToNextPingUpdate < 0)
            {
                UpdatePing();
                secondsToNextPingUpdate = PingUpdateFrequency;
            }
        }
        static void CustomLoadStaticContent(string contentManagerName) { }

        void SendChat()
        {
            if(!string.IsNullOrWhiteSpace(GameState.CurrentChat))
            {
                var chat = $"{GameState.LocalPlayer?.Username}: {GameState.CurrentChat}";
                SignalRedClient.Instance.CreateGenericMessage(MessageKey, chat);
                GameState.CurrentChat = "";
            }            
        }
        void SendReadyStatus(bool isReady)
        {
            var me = GameState.LocalPlayer;
            if(me != null)
            {
                me.CurrentStatus = isReady ? PlayerJoinStatus.Ready : PlayerJoinStatus.Connected;
                SignalRedClient.Instance.UpdateEntity(me);
            }
        }
        void UpdatePing()
        {
            var me = GameState.LocalPlayer;
            if (me != null)
            {
                me.Ping = SignalRedClient.Instance.Ping;
                SignalRedClient.Instance.UpdateEntity(me);
            }
        }
        void RequestStartGame()
        {
            SignalRedClient.Instance.RequestScreenTransition(typeof(Level1).FullName);
        }
        void LeaveServer()
        {
            SignalRedClient.Instance.DeleteEntity(GameState.LocalPlayer);
            SignalRedClient.Instance.Disconnect(() =>
            {
                MoveToScreen(typeof(ConnectToServer).FullName);
            });
        }


        protected override void ApplyGenericMessage(GenericMessage message)
        {
            if (message.MessageKey == MessageKey)
            {
                GameState.Messages.Add(message.MessageValue);
            }
        }

        protected override void CreateEntity(EntityStateMessage message)
        {
            CreateOrUpdateEntity(message, true);
        }

        protected override void UpdateEntity(EntityStateMessage message, bool isReckonMessage = false)
        {
            CreateOrUpdateEntity(message, isReckonMessage);
        }

        protected override void DeleteEntity(EntityStateMessage message)
        {
            var plyr = GameState.Players.Where(p => p.OwnerClientId == message.OwnerClientId).FirstOrDefault();
            if (plyr != null)
            {
                plyr.Destroy();
                GameState.Players.Remove(plyr);
            }
            GameState.UpdateStartableStatus();
        }

        void CreateOrUpdateEntity(EntityStateMessage message, bool force = false)
        {
            if (message.StateType == typeof(PlayerStatusNetworkState).FullName)
            {
                var state = message.GetState<PlayerStatusNetworkState>();
                var existing = GameState.Players
                    .Where(p => p.EntityId == message.EntityId).FirstOrDefault();
                if (existing != null)
                {
                    existing.ApplyCreationState(state, message.DeltaSeconds);
                }
                else
                {
                    var plyr = new PlayerStatusViewModel();
                    plyr.OwnerClientId = message.OwnerClientId;
                    plyr.EntityId = message.EntityId;
                    plyr.ApplyUpdateState(state, message.DeltaSeconds, force);
                    GameState.Players.Add(plyr);

                    // if a new player joined, set our state back to not ready
                    if (plyr != GameState.LocalPlayer)
                    {
                        SendReadyStatus(false);
                    }
                }
            }

            GameState.UpdateStartableStatus();
        }
    }
}

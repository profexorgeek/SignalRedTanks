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
        const float ReckonFrequencySeconds = 1f;

        float secondsToNextReckon = 0f;

        GameStateViewModel GameState => GameStateService.Instance.GameState;

        void CustomInitialize()
        {
            ServerLobbyGum.ServerLobbyMenu.BindingContext = GameState;
            ServerLobbyGum.FormsControl.ServerLobbyMenu.SendChatButton.Click += (s,e) =>  SendChat();
            ServerLobbyGum.FormsControl.ServerLobbyMenu.ReadyButton.Click += (s, e) => SendReadyStatus(true);
            ServerLobbyGum.FormsControl.ServerLobbyMenu.LeaveButton.Click += (s, e) => LeaveServer();
            ServerLobbyGum.FormsControl.ServerLobbyMenu.StartButton.Click += (s, e) => RequestStartGame();

            SignalRedClient.Instance.ConnectionClosed               += ConnectionClosed;
            SignalRedClient.Instance.ScreenTransitionReceived       += ScreenTransitionReceived;
            SignalRedClient.Instance.EntityCreateReceived           += EntityCreateOrUpdateReceived;
            SignalRedClient.Instance.EntityUpdateReceived           += EntityCreateOrUpdateReceived;
            SignalRedClient.Instance.EntityDeleteReceived           += EntityDeleteReceived;
            SignalRedClient.Instance.EntityReckonReceived           += EntityReckonReceived;
            SignalRedClient.Instance.GenericMessageReceived         += GenericMessageReceived;

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
        void CustomDestroy()
        {
            SignalRedClient.Instance.ConnectionClosed               -= ConnectionClosed;
            SignalRedClient.Instance.ScreenTransitionReceived       -= ScreenTransitionReceived;
            SignalRedClient.Instance.EntityCreateReceived           -= EntityCreateOrUpdateReceived;
            SignalRedClient.Instance.EntityUpdateReceived           -= EntityCreateOrUpdateReceived;
            SignalRedClient.Instance.EntityDeleteReceived           -= EntityDeleteReceived;
            SignalRedClient.Instance.EntityReckonReceived           -= EntityReckonReceived;
            SignalRedClient.Instance.GenericMessageReceived         -= GenericMessageReceived;
        }
        void CustomActivity(bool firstTimeCalled)
        {
            if (InputManager.Keyboard.KeyReleased(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                SendChat();
            }

            secondsToNextReckon -= TimeManager.SecondDifference;
            if(secondsToNextReckon < 0)
            {
                _ = SignalRedClient.Instance.ReckonEntities();
                secondsToNextReckon = ReckonFrequencySeconds;
            }
        }
        static void CustomLoadStaticContent(string contentManagerName) { }

        async void SendChat()
        {
            if(!string.IsNullOrWhiteSpace(GameState.CurrentChat))
            {
                var chat = $"{GameState.LocalPlayer?.Username}: {GameState.CurrentChat}";
                await SignalRedClient.Instance.CreateGenericMessage(MessageKey, chat);
                GameState.CurrentChat = "";
            }            
        }
        async void SendReadyStatus(bool isReady)
        {
            var me = GameState.LocalPlayer;
            if(me != null)
            {
                me.CurrentStatus = isReady ? PlayerJoinStatus.Ready : PlayerJoinStatus.Connected;
                await SignalRedClient.Instance.UpdateEntity(me);
            }
            
        }
        async void RequestStartGame()
        {
            await SignalRedClient.Instance.RequestScreenTransition(typeof(Level1).FullName);
        }
        async void LeaveServer()
        {
            await SignalRedClient.Instance.DeleteEntity(GameState.LocalPlayer);
            await SignalRedClient.Instance.DisconnectAsync();
        }


        #region Networking Handlers
        void ConnectionClosed(Exception message)
        {
            MoveToScreen(typeof(ConnectToServer).FullName);
        }

        void ScreenTransitionReceived(ScreenMessage message)
        {
            if (message.TargetScreen != this.GetType().FullName)
            {
                ScreenManager.MoveToScreen(message.TargetScreen);
            }
        }

        void EntityCreateOrUpdateReceived(EntityStateMessage message)
        {
            if (message.StateType == typeof(PlayerStatusNetworkState).FullName)
            {
                var state = message.GetState<PlayerStatusNetworkState>();
                var existing = GameState.Players
                    .Where(p => p.EntityId == message.EntityId).FirstOrDefault();
                if(existing != null)
                {
                    existing.ApplyState(state);
                }
                else
                {
                    var plyr = new PlayerStatusViewModel();
                    plyr.OwnerClientId = message.OwnerClientId;
                    plyr.EntityId = message.EntityId;
                    plyr.ApplyState(state);
                    GameState.Players.Add(plyr);

                    // if a new player joined, set our state back to not ready
                    if(plyr != GameState.LocalPlayer)
                    {
                        SendReadyStatus(false);
                    }
                }
            }

            GameState.UpdateStartableStatus();
        }

        void EntityDeleteReceived(EntityStateMessage message)
        {
            var plyr = GameState.Players.Where(p => p.OwnerClientId == message.OwnerClientId).FirstOrDefault();
            if (plyr != null)
            {
                GameState.Players.Remove(plyr);
            }
            GameState.UpdateStartableStatus();
        }

        void EntityReckonReceived(List<EntityStateMessage> message)
        {
            var incomingPlayers = message.Where(m => m.StateType == typeof(PlayerStatusNetworkState).FullName);

            // set all known players to disconnected
            foreach(var p in GameState.Players)
            {
                p.CurrentStatus = PlayerJoinStatus.Disconnected;
            }

            // now update from our incoming list
            foreach (var msg in incomingPlayers)
            {
                EntityCreateOrUpdateReceived(msg);
            }

            // finally remove players that aren't connected
            for (var i = GameState.Players.Count - 1; i > -1; i--)
            {
                if (GameState.Players[i].CurrentStatus == PlayerJoinStatus.Disconnected)
                {
                    GameState.Players.RemoveAt(i);
                }
            }

            GameState.UpdateStartableStatus();
        }

        private void GenericMessageReceived(GenericMessage message)
        {
            if(message.MessageKey == MessageKey)
            {
                GameState.Messages.Add(message.MessageValue);
            }
        }

        #endregion
    }
}

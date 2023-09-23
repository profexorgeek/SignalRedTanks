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
            DoNetworkMessages();

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

        void DoNetworkMessages()
        {
            DoNetworkScreenMessages();
            DoNetworkEntityMessages();
            DoNetworkGenericMessages();
        }

        void DoNetworkScreenMessages()
        {
            var message = SignalRedClient.Instance.GetCurrentScreen();
            if (!string.IsNullOrEmpty(message.TargetScreen) &&
                message.TargetScreen != this.GetType().FullName)
            {
                ScreenManager.MoveToScreen(message.TargetScreen);
            }
        }

        void DoNetworkEntityMessages()
        {
            var messageTuples = SignalRedClient.Instance.GetEntityMessages();
            for(var i = 0; i < messageTuples.Count; i++)
            {
                var message = messageTuples[i].Item1;
                var messageType = messageTuples[i].Item2;
                switch(messageType)
                {
                    case SignalRedMessageType.Reckon:
                        CreateOrUpdateEntity(message, true);
                        break;
                    case SignalRedMessageType.Create:
                        CreateOrUpdateEntity(message);
                        break;
                    case SignalRedMessageType.Update:
                        CreateOrUpdateEntity(message);
                        break;
                    case SignalRedMessageType.Delete:
                        DeleteEntity(message);
                        break;
                    default:
                        throw new Exception($"Unexpected entity message type: {messageType} {message.StateType}");
                        break;
                }
            }
        }

        void DoNetworkGenericMessages()
        {
            var messages = SignalRedClient.Instance.GetGenericMessages();
            for(var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.MessageKey == MessageKey)
                {
                    GameState.Messages.Add(message.MessageValue);
                }
            }
        }



        #region Networking Handlers
        void ConnectionClosed(Exception message)
        {
            MoveToScreen(typeof(ConnectToServer).FullName);
        }

        void CreateOrUpdateEntity(EntityStateMessage message, bool force = false)
        {
            if (message.StateType == typeof(PlayerStatusNetworkState).FullName)
            {
                var state = message.GetState<PlayerStatusNetworkState>();
                var existing = GameState.Players
                    .Where(p => p.EntityId == message.EntityId).FirstOrDefault();
                if(existing != null)
                {
                    existing.ApplyState(state, force);
                }
                else
                {
                    var plyr = new PlayerStatusViewModel();
                    plyr.OwnerClientId = message.OwnerClientId;
                    plyr.EntityId = message.EntityId;
                    plyr.ApplyState(state, force);
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

        void DeleteEntity(EntityStateMessage message)
        {
            var plyr = GameState.Players.Where(p => p.OwnerClientId == message.OwnerClientId).FirstOrDefault();
            if (plyr != null)
            {
                GameState.Players.Remove(plyr);
            }
            GameState.UpdateStartableStatus();
        }

        //void EntityReckonReceived(List<EntityStateMessage> message)
        //{
        //    var incomingPlayers = message.Where(m => m.StateType == typeof(PlayerStatusNetworkState).FullName);

        //    // set all known players to disconnected
        //    foreach(var p in GameState.Players)
        //    {
        //        p.CurrentStatus = PlayerJoinStatus.Disconnected;
        //    }

        //    // now update from our incoming list
        //    foreach (var msg in incomingPlayers)
        //    {
        //        EntityCreateOrUpdateReceived(msg);
        //    }

        //    // finally remove players that aren't connected
        //    for (var i = GameState.Players.Count - 1; i > -1; i--)
        //    {
        //        if (GameState.Players[i].CurrentStatus == PlayerJoinStatus.Disconnected)
        //        {
        //            GameState.Players.RemoveAt(i);
        //        }
        //    }

        //    GameState.UpdateStartableStatus();
        //}
        #endregion
    }
}

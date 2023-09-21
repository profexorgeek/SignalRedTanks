using FlatRedBall.Input;
using SignalRed.Client;
using SignalRed.Common.Messages;
using System;
using System.Collections.Generic;
using TankMp.Models.ViewModels;

namespace TankMp.Screens
{
    public partial class ServerLobby
    {
        const string ReadyMessageKey = "ReadyStatus";

        LobbyViewModel lobbyViewModel = new LobbyViewModel();


        void CustomInitialize()
        {
            ServerLobbyGum.ServerLobbyMenu.BindingContext = lobbyViewModel;
            ServerLobbyGum.FormsControl.ServerLobbyMenu.SendChatButton.Click += (s,e) =>  SendChat();
            ServerLobbyGum.FormsControl.ServerLobbyMenu.ReadyButton.Click += (s, e) => SendReadyStatus(true);

            SignalRedClient.Instance.ChatReceived += ChatReceived;
            SignalRedClient.Instance.UserUpdateReceived += UserUpdateReceived;
            SignalRedClient.Instance.UserDeleteReceived += UserDeleteReceived;
            SignalRedClient.Instance.UserReckonReceived += UserReckonReceived;
            SignalRedClient.Instance.GenericMessageReceived += GenericMessageReceived;

            // request all users that may have joined before us
            SignalRedClient.Instance.ReckonUsers();

            // request all chats that may have happened before we joined
            SignalRedClient.Instance.RequestAllChats();
        }

        

        void CustomDestroy()
        {
            SignalRedClient.Instance.ChatReceived -= ChatReceived;
            SignalRedClient.Instance.UserUpdateReceived -= UserUpdateReceived;
            SignalRedClient.Instance.UserDeleteReceived -= UserDeleteReceived;
            SignalRedClient.Instance.UserReckonReceived -= UserReckonReceived;
            SignalRedClient.Instance.GenericMessageReceived -= GenericMessageReceived;
        }
        void CustomActivity(bool firstTimeCalled)
        {
            if (InputManager.Keyboard.KeyReleased(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
                SendChat();
            }
        }
        static void CustomLoadStaticContent(string contentManagerName) { }

        private void UserReckonReceived(List<UserMessage> message)
        {
            // first set all players to disconnected
            foreach (var plyr in lobbyViewModel.Players)
            {
                plyr.IsDisconnected = true;
            }

            // now add or update any new players, which will update
            // their IsDisconnected to false
            foreach (var user in message)
            {
                lobbyViewModel.AddOrUpdatePlayerFromNetworkMessage(user);
            }
        }
        private void UserDeleteReceived(UserMessage message)
        {
            lobbyViewModel.TryRemovePlayerWithClientId(message.ClientId);
        }
        private void UserUpdateReceived(UserMessage message)
        {
            lobbyViewModel.AddOrUpdatePlayerFromNetworkMessage(message);

            // any new user must reset the ready status
            SendReadyStatus(false);
        }
        private void ChatReceived(ChatMessage message)
        {
            lobbyViewModel.Chats.Add(message.ToString());
        }
        private void GenericMessageReceived(GenericMessage message)
        {
            if (message.MessageKey == ReadyMessageKey)
            {
                bool isReady = Convert.ToBoolean(message.MessageValue);
                lobbyViewModel.SetPlayerReadyStatus(message.ClientId, isReady);
            }
        }

        async void SendChat()
        {
            var chat = lobbyViewModel.CurrentChat;
            await SignalRedClient.Instance.SendChat(chat);
            lobbyViewModel.CurrentChat = "";
        }
        async void SendReadyStatus(bool isReady)
        {
            await SignalRedClient.Instance.SendGenericMessage(ReadyMessageKey, isReady.ToString());
        }
    }
}

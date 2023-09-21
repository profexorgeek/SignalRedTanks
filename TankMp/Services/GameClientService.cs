using FlatRedBall.Screens;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using SignalRed.Common.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TankMp.Models.ViewModels;

namespace TankMp.Services
{
    public class GameClientService : IGameClient
    {
        private static GameClientService instance;
        bool initialized = false;


        public static GameClientService Instance => instance ?? (instance = new GameClientService());
        public bool Initialized => initialized;
        public LobbyViewModel LobbyViewModel { get; set; }


        private GameClientService() { }

        public void Initialize()
        {
            SignalRedClient.Instance.Initialize(this);
            initialized = true;
            LobbyViewModel = new LobbyViewModel();
        }


        #region IGameClient
        public Task FailConnection(Exception exception)
        {
            throw new NotImplementedException();
        }

        public Task MoveToScreen(ScreenMessage message)
        {
            var screen = message.NewScreen;
            if(ScreenManager.CurrentScreen.GetType().FullName != screen)
            {
                ScreenManager.MoveToScreen(screen);
            }
            return Task.CompletedTask;
        }

        public Task CreateEntity(EntityMessage message)
        {
            throw new NotImplementedException();
        }
        public Task UpdateEntity(EntityMessage message)
        {
            throw new NotImplementedException();
        }
        public Task DeleteEntity(EntityMessage message)
        {
            throw new NotImplementedException();
        }
        public Task ReckonEntities(List<EntityMessage> entities)
        {
            throw new NotImplementedException();
        }

        public Task ReceiveChat(ChatMessage message)
        {
            LobbyViewModel.Chats.Add(message.ToString());
            return Task.CompletedTask;
        }
        public Task DeleteAllChats()
        {
            throw new NotImplementedException();
        }


        public Task RegisterUser(UserMessage message)
        {
            LobbyViewModel.AddOrUpdatePlayerFromNetworkMessage(message);
            return Task.CompletedTask;
        }
        public Task DeleteUser(UserMessage message)
        {
            LobbyViewModel.TryRemovePlayerWithClientId(message.ClientId);
            return Task.CompletedTask;
        }
        public Task ReckonUsers(List<UserMessage> users)
        {
            // first set all players to disconnected
            foreach (var plyr in LobbyViewModel.Players)
            {
                plyr.IsDisconnected = true;
            }

            // now add or update any new players, which will update
            // their IsDisconnected to false
            foreach (var user in users)
            {
                LobbyViewModel.AddOrUpdatePlayerFromNetworkMessage(user);
            }
            
            return Task.CompletedTask;
        }
        #endregion
    }
}

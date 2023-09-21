﻿using FlatRedBall.Screens;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using SignalRed.Common.Messages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TankMp.Models;

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
            SRClient.Instance.Initialize(this);
            initialized = true;
            LobbyViewModel = new LobbyViewModel();
        }

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
            LobbyViewModel.Players.Add(message.UserName);
            return Task.CompletedTask;
        }
        public Task DeleteUser(UserMessage message)
        {
            throw new NotImplementedException();
        }
        public Task ReckonUsers(List<UserMessage> users)
        {
            LobbyViewModel.Players.Clear();
            foreach (var user in users)
            {
                LobbyViewModel.Players.Add(user.UserName);
            }
            return Task.CompletedTask;
        }
    }
}

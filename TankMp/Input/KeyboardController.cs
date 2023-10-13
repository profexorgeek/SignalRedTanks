﻿using FlatRedBall;
using FlatRedBall.Gui;
using FlatRedBall.Input;
using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework.Input;
using SignalRed.Client;
using SignalRed.Common.Interfaces;
using System;
using System.Numerics;
using TankMp.Entities;
using TankMp.Entities.Tanks;
using TankMp.Models;

namespace TankMp.Input
{
    public class KeyboardController : ITankController, ISignalRedEntity<TankNetworkState>
    {
        const float NetworkUpdateFrequencySeconds = 0.15f;

        Vector2 movementVector = Vector2.Zero;
        TankNetworkState recentState = new TankNetworkState();
        float secondsToNextNetworkUpdate;
        float secondsToTankRespawn;
        TankBase tank;

        public string OwnerClientId { get; set; }
        public string EntityId { get; set; }

        public float MovementAngle => (float)Math.Atan2(movementVector.Y, movementVector.X);
        public float MovementMagnitude => movementVector.Length().Clamp(-1f, 1f);
        public float AimAngle => (float)Math.Atan2(
            GuiManager.Cursor.WorldYAt(0) - Tank.Y,
            GuiManager.Cursor.WorldXAt(0) - Tank.X);
        public bool Firing => GuiManager.Cursor.PrimaryDown;
        public bool TankDestroyed => Tank == null || Tank.Destroyed;
        public TankBase Tank
        {
            get
            {
                return tank;
            }
            set
            {
                tank = value;
                if(tank != null)
                {
                    tank.Controller = this;
                }
            }
        }

        public void Update()
        {
            // early out if tank is dead:
            if(TankDestroyed)
            {
                return;
            }

            var kb = InputManager.Keyboard;
            var cursor = GuiManager.Cursor;

            movementVector.X = kb.KeyDown(Keys.D) ? 1f : 0;
            movementVector.X = kb.KeyDown(Keys.A) ? -1f : movementVector.X;
            movementVector.Y = kb.KeyDown(Keys.W) ? 1f : 0;
            movementVector.Y = kb.KeyDown(Keys.S) ? -1f : movementVector.Y;

            if(Tank.CurrentHealth <= 0)
            {
                Tank.Die();
            }

            TrySendNetworkUpdate();
        }

        public void TrySendNetworkUpdate()
        {
            if(SignalRedClient.Instance.Connected)
            {
                secondsToNextNetworkUpdate -= TimeManager.SecondDifference;
                if (secondsToNextNetworkUpdate < 0)
                {
                    SignalRedClient.Instance.UpdateEntity(this);
                    secondsToNextNetworkUpdate = NetworkUpdateFrequencySeconds;
                }
            }
        }

        public void ApplyCreationState(TankNetworkState networkState, float deltaSeconds)
        {
            if (Tank != null)
            {
                Tank.X = networkState.X;
                Tank.Y = networkState.Y;
                Tank.Velocity.X = networkState.VelocityX;
                Tank.Velocity.Y = networkState.VelocityY;
                Tank.CurrentHealth = networkState.CurrentHealth;

                if(Tank.CurrentHealth > 0)
                {
                    Tank.Destroyed = false;
                }
            }
        }

        public void ApplyUpdateState(TankNetworkState networkState, float deltaSeconds, bool force = false)
        {
            // NOOP:
            // keyboard controller is controlled by a local player and
            // the local player is authoritative over their own entity
            // so we do not apply incoming states unless they are a new
            // creation state.
        }

        public void Destroy(TankNetworkState networkState, float deltaSeconds)
        {
            if (Tank != null)
            {
                Tank.Destroyed = true;
            }
        }

        public TankNetworkState GetState()
        {
            return new TankNetworkState()
            {
                MovementAngle = MovementAngle,
                MovementMagnitude = MovementMagnitude,
                AimAngle = AimAngle,
                Firing = Firing,
                X = Tank.X,
                Y = Tank.Y,
                VelocityX = Tank.Velocity.X,
                VelocityY = Tank.Velocity.Y,
                CurrentHealth = Tank.CurrentHealth,
            };
        }

        
    }
}

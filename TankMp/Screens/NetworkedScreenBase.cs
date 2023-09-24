using SignalRed.Client;
using SignalRed.Common.Messages;
using System;

namespace TankMp.Screens
{
    public partial class NetworkedScreenBase
    {

        void CustomInitialize()
        {
            // EARLY OUT: connection lost
            if (SignalRedClient.Instance.Connected == false)
            {
                MoveToScreen(typeof(ConnectToServer).FullName);
                return;
            }
        }

        void CustomActivity(bool firstTimeCalled)
        {
            DoNetworkStatus();

        }

        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        /// <summary>
        /// Processes all incoming messages from the server and
        /// calls overridable methods to handle the different message
        /// types
        /// </summary>
        /// <exception cref="Exception">Bad message type</exception>
        void DoNetworkStatus()
        {
            // EARLY OUT: connection lost
            if(SignalRedClient.Instance.Connected == false)
            {
                MoveToScreen(typeof(ConnectToServer).FullName);
                return;
            }


            // EARLY OUT: we're on the wrong screen
            var screen = SignalRedClient.Instance.GetCurrentScreen().TargetScreen;
            if(!string.IsNullOrEmpty(screen) && 
                !screen.Equals(this.GetType().FullName))
            {
                MoveToScreen(screen);
                return;
            }

            var genericMessages = SignalRedClient.Instance.GetGenericMessages();
            for(var i = 0; i < genericMessages.Count; i++)
            {
                ApplyGenericMessage(genericMessages[i]);
            }

            var entityMessages = SignalRedClient.Instance.GetEntityMessages();
            for(var i = 0; i < entityMessages.Count; i++)
            {
                var message = entityMessages[i].Item1;
                var type = entityMessages[i].Item2;
                switch(type)
                {
                    case SignalRedMessageType.Create:
                        CreateEntity(message);
                        break;
                    case SignalRedMessageType.Update:
                        UpdateEntity(message);
                        break;
                    case SignalRedMessageType.Reckon:
                        UpdateEntity(message, true);
                        break;
                    case SignalRedMessageType.Delete:
                        DeleteEntity(message);
                        break;
                    default:
                        throw new Exception($"Unexpected entity message type: {type} {message.StateType}");
                }
            }
        }

        /// <summary>
        /// Handle incoming generic messages
        /// </summary>
        /// <param name="message">A generic message</param>
        protected virtual void ApplyGenericMessage(GenericMessage message) { }

        /// <summary>
        /// Create an entity from an incoming message
        /// </summary>
        /// <param name="message">A message with the initial entity state</param>
        protected virtual void CreateEntity(EntityStateMessage message) { }

        /// <summary>
        /// Update an entity from an incoming message
        /// </summary>
        /// <param name="message">A message with an updated entity state</param>
        /// <param name="isReckonMessage">Whether the message is a reckoning message that should be forced</param>
        protected virtual void UpdateEntity(EntityStateMessage message, bool isReckonMessage = false) { }

        /// <summary>
        /// Delete an entity from an incoming message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void DeleteEntity(EntityStateMessage message) { }

    }
}

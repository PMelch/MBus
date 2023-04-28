using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MBus
{
    /// <summary>
    /// The main message box object.
    /// There is no built in way on how to get the MBus instance.
    ///
    /// The recommended way is to use some kind of dependency injection framework
    /// (Zenject or one of the alternatives <see href="https://www.libhunt.com/r/Zenject"/>)
    ///
    /// You could of course just create some DontDestroyOnLoad object which holds the instance or
    /// use a singleton. 
    /// </summary>
    public class MBus
    {
        private readonly List<Action<IMBusMessage>> _handlers = new();
        private readonly Dictionary<(Type, object), Action<IMBusMessage>> _baseHandlerMap = new();
        private bool _sendingInProgress;
        private readonly Queue<IMBusMessage> _pendingMessages = new();

        /// <summary>
        /// Add a listener for a type of message.
        /// This can either be very specific message or the root message type (IMbusMessage).
        /// If you use this method, you'll have to manually unsubscribe the handler later on.
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus AddListener<T>(Action<T> handler) where T : IMBusMessage
        {
            // create the wrapper handler that will take care of the type checking 
            void BaseHandler(IMBusMessage message)
            {
                if (message is T tMessage)
                {
                    handler.Invoke(tMessage);
                }
            }

            // add the mapping to determine the actual used handler derived from the user type and action. 
            _baseHandlerMap.Add((typeof(T), handler), BaseHandler);
            // remember the wrapped handler
            _handlers.Add(BaseHandler);
            
            return this;
        }

        /// <summary>
        /// Add a listener to a specific message type.
        /// The listener will be unsubscribed when the game object gets destroyed.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="holderComponent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus AddListenerUntilDestroyed<T>(Action<T> handler, Component holderComponent) where T : IMBusMessage
        {
            AddListener(handler);
            
            // dynamically create a MBusOnDestroyUnSubscriber component which will take care of the unsubscription
            var component = holderComponent.gameObject.AddComponent<MBusOnDestroyUnSubscriber>();
            component.Bus = this;
            component.Handler = handler;
            component.Type = typeof(T);
            return this;
        }

        /// <summary>
        /// Add a listener to a specific message type.
        /// The listener will be unsubscribed when the game object gets disabled.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="holderComponent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus AddListenerUntilDisabled<T>(Action<T> handler, Component holderComponent) where T : IMBusMessage
        {
            AddListener(handler);

            // dynamically create a MBusOnDisableUnSubscriber component which will take care of the unsubscription
            var component = holderComponent.gameObject.AddComponent<MBusOnDisableUnSubscriber>();
            component.Bus = this;
            component.Handler = handler;
            component.Type = typeof(T);
            return this;
        }


        /// <summary>
        /// This method is mainly used by the MBusOnDestroyUnSubscriber and MBusOnDisableUnSubscriber components.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public MBus RemoveListener(Type type, object handler)
        {
            var baseHandlerKey = (type, handler);
            if (_baseHandlerMap.TryGetValue(baseHandlerKey, out var baseHandler))
            {
                _baseHandlerMap.Remove(baseHandlerKey);
                _handlers.Remove(baseHandler);
            }

            return this;
        } 
        
        /// <summary>
        /// If you have used <see cref="AddListener{T}"/> to register a listener, you
        /// need to manually remove the listener using this function.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public MBus RemoveListener<T>(Action<T> handler) where T : IMBusMessage
        {
            RemoveListener(typeof(T), handler);
            return this;
        }
        
        /// <summary>
        /// Send a message to the bus which will notify all handlers.
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void SendMessage<T>(T message) where T : IMBusMessage
        {
            // In case we're already in the process of sending a message, let's queue this message for afterwards.
            // The reason for that is we want to keep the order of messages consistent for all message handlers.
            if (_sendingInProgress)
            {
                _pendingMessages.Enqueue(message);
            }
            else
            {
                _sendingInProgress = true;
                for (var t = 0; t < _handlers.Count; t++)
                {
                    var handler = _handlers[t];
                    try
                    {
                        handler.Invoke(message);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Got an error while invoking message handler for {typeof(T)}:{message}");
                        Debug.Log(e.Message);
                    }
                }

                _sendingInProgress = false;
                
                if (_pendingMessages.Count>0)
                {
                    var queuedMessage = _pendingMessages.Dequeue();
                    SendMessage(queuedMessage);
                }
            }
        }
    }
}
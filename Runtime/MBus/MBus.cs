using System;
using System.Collections.Generic;
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
        private readonly List<Action<object>> _handlers = new();
        private readonly Dictionary<(Type, object), Action<object>> _baseHandlerMap = new();
        private bool _sendingInProgress;
        private readonly Queue<object> _pendingMessages = new();

        /// <summary>
        /// Add a listener for a type of message.
        /// This can either be very specific message or just object (which could receive any type of message).
        /// If you use this method, you'll have to manually unsubscribe the handler later on.
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus Subscribe<T>(Action<T> handler)
        {
            // create the wrapper handler that will take care of the type checking 
            void BaseHandler(object message)
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
        /// Add a listener for a specific type and value.
        /// <example><code>
        ///
        /// public void OnMessage100()
        /// {
        /// }
        ///
        /// // trigger callback only if message type is Type1
        /// mBus.Subscribe(OnMessage100, 100);
        /// 
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus Subscribe<T>(Action handler, T value) 
        {
            // create the wrapper handler that will take care of the type checking 
            void BaseHandler(object message)
            {
                if (message is T tMessage)
                {
                    if (tMessage.Equals(value))
                    {
                        handler.Invoke();
                    }
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
        public MBus SubscribeUntilDestroyed<T>(Action<T> handler, Component holderComponent)
        {
            Subscribe(handler);
            AddOnDestroyedUnSubscriber<T>(handler, holderComponent);
            return this;
        }


        /// <summary>
        /// Add a listener to a specific message type and value.
        /// The listener will be unsubscribed when the game object gets destroyed.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="holderComponent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus SubscribeUntilDestroyed<T>(Action handler, T value, Component holderComponent) 
        {
            Subscribe(handler, value);
            AddOnDestroyedUnSubscriber<T>(handler, holderComponent);
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
        public MBus SubscribeUntilDisabled<T>(Action<T> handler, Component holderComponent)
        {
            Subscribe(handler);
            AddOnDisabledUnSubscriber<T>(handler, holderComponent);
            return this;
        }



        /// <summary>
        /// Add a listener to a specific message type and value.
        /// The listener will be unsubscribed when the game object gets disabled.
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="holderComponent"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MBus SubscribeUntilDisabled<T>(Action handler, T value, Component holderComponent) 
        {
            Subscribe(handler, value);
            AddOnDisabledUnSubscriber<T>(handler, holderComponent);
            return this;
        }


        /// <summary>
        /// This method is mainly used by the MBusOnDestroyUnSubscriber and MBusOnDisableUnSubscriber components.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public MBus Unsubscribe(Type type, object handler)
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
        /// If you have used <see cref="Subscribe{T}"/> to register a listener, you
        /// need to manually remove the listener using this function.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public MBus Unsubscribe<T>(Action<T> handler)
        {
            Unsubscribe(typeof(T), handler);
            return this;
        }
        
        /// <summary>
        /// Send a message to the bus which will notify all handlers.
        /// </summary>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        public void SendMessage<T>(T message)
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
        
        
        private void AddOnDestroyedUnSubscriber<T>(object handler, Component holderComponent)
        {
            // dynamically create a MBusOnDestroyUnSubscriber component which will take care of the unsubscription
            var component = holderComponent.gameObject.AddComponent<MBusOnDestroyUnSubscriber>();
            component.Bus = this;
            component.Handler = handler;
            component.Type = typeof(T);
        }

        private void AddOnDisabledUnSubscriber<T>(object handler, Component holderComponent)
        {
            // dynamically create a MBusOnDisableUnSubscriber component which will take care of the unsubscription
            var component = holderComponent.gameObject.AddComponent<MBusOnDisableUnSubscriber>();
            component.Bus = this;
            component.Handler = handler;
            component.Type = typeof(T);
        }
    }
}
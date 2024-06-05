using System;
using System.Collections.Generic;
using MBus.components;
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
        private readonly Dictionary<Type, List<Action<object>>> _handlers = new();
        private readonly Dictionary<(Type, object), Action<object>> _baseTypeHandlerMap = new();
        private readonly Dictionary<(Type, object), List<Action>> _valueHandlers = new();
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
                handler.Invoke((T)message);
            }

            // add the mapping to determine the actual used handler derived from the user type and action. 
            var messageType = typeof(T);
            _baseTypeHandlerMap.Add((messageType, handler), BaseHandler);
            
            // remember the wrapped handler
            var typeHandlers = _handlers.GetValueOrDefault(messageType, new List<Action<object>>());;
            typeHandlers.Add(BaseHandler);
            _handlers[messageType] = typeHandlers;
            
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
            // add the mapping to determine the actual used handler derived from the user type and action. 
            var messageType = typeof(T);
            
            // remember the wrapped handler
            var keyTuple = (messageType, value);
            var typeHandlers = _valueHandlers.GetValueOrDefault(keyTuple, new List<Action>());;
            typeHandlers.Add(handler);
            _valueHandlers[keyTuple] = typeHandlers;
            
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
            var component = AddOnDestroyedUnSubscriber<T>(handler, holderComponent);
            component.IsValueSubscription = true;
            component.Value = value;
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
            var component = AddOnDisabledUnSubscriber<T>(handler, holderComponent);
            component.IsValueSubscription = true;
            component.Value = value;
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
            if (_baseTypeHandlerMap.TryGetValue(baseHandlerKey, out var baseHandler))
            {
                _baseTypeHandlerMap.Remove(baseHandlerKey);
                if (_handlers.TryGetValue(type, out var list))
                {
                    list.Remove(baseHandler);
                }
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
        /// If you have used <see cref="Subscribe{T}(System.Action,T)("/> to register a listener, you
        /// need to manually remove the listener using this function.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public MBus Unsubscribe<T>(Action handler, T value)
        {
            var keyTuple = (value.GetType(), value);
            if (_valueHandlers.TryGetValue(keyTuple, out var handlers))
            {
                handlers.Remove(handler);
            }

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
                // invoke all type handlers
                var typesCopy = new List<Type>(_handlers.Keys);
                foreach (var key in typesCopy)
                {
                    if (key.IsInstanceOfType(message))
                    {
                        var handlers = new List<Action<object>>(_handlers[key]);
                        foreach (var handler in handlers)
                        {
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
                    }                    
                }
                
                // invoke all type/value handlers
                var valuesCopy = new List<(Type, object)>(_valueHandlers.Keys);
                foreach (var keyTuple in valuesCopy)
                {
                    if (keyTuple.Item1.IsInstanceOfType(message) && (keyTuple.Item2?.Equals(message) ?? false))
                    {
                        var handlers = new List<Action>(_valueHandlers[keyTuple]);
                        foreach (var handler in handlers)
                        {
                            try
                            {
                                handler.Invoke();
                            }
                            catch (Exception e)
                            {
                                Debug.Log($"Got an error while invoking message handler for {typeof(T)}:{message}");
                                Debug.Log(e.Message);
                            }
                        }
                    }                    
                }
                

                _sendingInProgress = false;
                
                // now, get the first pending message, if there was any, and process it
                if (_pendingMessages.Count>0)
                {
                    var queuedMessage = _pendingMessages.Dequeue();
                    SendMessage(queuedMessage);
                }
            }
        }
        
        
        private MBusOnDestroyUnSubscriber AddOnDestroyedUnSubscriber<T>(object handler, Component holderComponent)
        {
            // dynamically create a MBusOnDestroyUnSubscriber component which will take care of the unsubscription
            var component = holderComponent.gameObject.AddComponent<MBusOnDestroyUnSubscriber>();
            component.Bus = this;
            component.Handler = handler;
            component.Type = typeof(T);
            return component;
        }

        private MBusOnDisableUnSubscriber AddOnDisabledUnSubscriber<T>(object handler, Component holderComponent)
        {
            // dynamically create a MBusOnDisableUnSubscriber component which will take care of the unsubscription
            var component = holderComponent.gameObject.AddComponent<MBusOnDisableUnSubscriber>();
            component.Bus = this;
            component.Handler = handler;
            component.Type = typeof(T);
            return component;
        }
    }
}

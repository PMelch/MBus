using System;
using UnityEngine;

namespace MBus
{
    /// <summary>
    /// While it is generally favorable to use dependency injection to acquire the MBus instance,
    /// this is a helper class to store and use a global MBus instance.
    /// </summary>
    public static class MBusInstance
    {
        private static MBus _instance;

        /// <summary>
        /// Sets the global MBus instance used by the static methods in this class.
        /// </summary>
        /// <param name="bus">The MBus instance to use.</param>
        public static void SetInstance(MBus bus)
        {
            _instance = bus;
        }

        /// <summary>
        /// Subscribes a handler to messages of type T on the global instance.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="handler">The action to invoke when a message of type T is received.</param>
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        /// <returns>The global MBus instance, or null if not set.</returns>
        public static MBus Subscribe<T>(Action<T> handler)
        {
            _instance?.Subscribe(handler);
            return _instance;
        }

        /// <summary>
        /// Subscribes a handler to messages of type T on the global instance, automatically unsubscribing when the specified component is destroyed.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="handler">The action to invoke when a message is received.</param>
        /// <param name="holderComponent">The component whose lifecycle determines the subscription duration.</param>
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        /// <returns>The global MBus instance, or null if not set.</returns>
        public static MBus SubscribeUntilDestroyed<T>(Action<T> handler, Component holderComponent)
        {
            _instance?.SubscribeUntilDestroyed(handler, holderComponent);
            return _instance;
        }
        
        /// <summary>
        /// Subscribes a handler to a specific value of type T on the global instance, automatically unsubscribing when the specified component is destroyed.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="handler">The action to invoke when the value is received.</param>
        /// <param name="value">The specific value to listen for.</param>
        /// <param name="holderComponent">The component whose lifecycle determines the subscription duration.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The global MBus instance, or null if not set.</returns>
        public static MBus SubscribeUntilDestroyed<T>(Action handler, T value, Component holderComponent)
        {
            _instance?.SubscribeUntilDestroyed(handler, value, holderComponent);
            return _instance;
        }
        
        /// <summary>
        /// Subscribes a handler to messages of type T on the global instance, automatically unsubscribing when the specified component is disabled.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="handler">The action to invoke when a message is received.</param>
        /// <param name="holderComponent">The component whose lifecycle determines the subscription duration.</param>
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        /// <returns>The global MBus instance, or null if not set.</returns>
        public static MBus SubscribeUntilDisabled<T>(Action<T> handler, Component holderComponent)
        {
            _instance?.SubscribeUntilDisabled(handler, holderComponent);
            return _instance;
        }

        /// <summary>
        /// Subscribes a handler to a specific value of type T on the global instance, automatically unsubscribing when the specified component is disabled.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="handler">The action to invoke when the value is received.</param>
        /// <param name="value">The specific value to listen for.</param>
        /// <param name="holderComponent">The component whose lifecycle determines the subscription duration.</param>
        /// <typeparam name="T">The type of the value.</typeparam>
        /// <returns>The global MBus instance, or null if not set.</returns>
        public static MBus SubscribeUntilDisabled<T>(Action handler, T value, Component holderComponent)
        {
            _instance?.SubscribeUntilDisabled(handler, value, holderComponent);
            return _instance;
        }

        /// <summary>
        /// Manually unsubscribes a previously registered handler from the global instance.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        /// <typeparam name="T">The type of message the handler was subscribed to.</typeparam>
        /// <returns>The global MBus instance, or null if not set.</returns>
        public static MBus Unsubscribe<T>(Action<T> handler)
        {
            _instance?.Unsubscribe(handler);
            return _instance;
        }
        
        /// <summary>
        /// Sends a message to all subscribers on the global instance.
        /// Does nothing if the global instance is null.
        /// </summary>
        /// <param name="message">The message object to send.</param>
        /// <typeparam name="T">The type of the message.</typeparam>
        public static void SendMessage<T>(T message)
        {
            _instance?.SendMessage(message);
        }
    }
}
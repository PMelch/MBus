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

        public static void SetInstance(MBus bus)
        {
            _instance = bus;
        }

        public static MBus Subscribe<T>(Action<T> handler) where T : IMBusMessage
        {
            _instance?.Subscribe(handler);
            return _instance;
        }
        public static MBus SubscribeUntilDestroyed<T>(Action<T> handler, Component holderComponent) where T : IMBusMessage
        {
            _instance?.SubscribeUntilDestroyed(handler, holderComponent);
            return _instance;
        }
        
        public static MBus SubscribeUntilDisabled<T>(Action<T> handler, Component holderComponent) where T : IMBusMessage
        {
            _instance?.SubscribeUntilDisabled(handler, holderComponent);
            return _instance;
        }

        public static MBus Unsubscribe<T>(Action<T> handler) where T : IMBusMessage
        {
            _instance?.Unsubscribe(handler);
            return _instance;
        }
    }
}
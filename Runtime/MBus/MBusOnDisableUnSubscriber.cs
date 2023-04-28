using System;
using UnityEngine;

namespace MBus
{
    /// <summary>
    /// Helper component that will react to Unity's OnDestroy event to unregister a specific message handler.
    /// Do not manually add that to one of your GameObjects. It will be automatically added
    /// if you use MBus like this:
    /// <example>
    /// ...
    /// mbus.SubscribeUntilDisabled<IMbusMessage>(OnMessage, this)
    /// ...
    /// </example>
    /// </summary>
    public class MBusOnDisableUnSubscriber : MonoBehaviour 
    {
        internal object Handler;
        internal MBus Bus;
        internal Type Type;

        private void OnDisable()
        {
            Bus.Unsubscribe(Type, Handler);
            Handler = null;
            Destroy(this);
        }
    }
}
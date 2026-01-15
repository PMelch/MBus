using System;
using UnityEngine;

namespace MBus.components
{
    /// <summary>
    /// Internal base class for components that manage automatic unsubscription from the message bus.
    /// Used by <see cref="MBusOnDestroyUnSubscriber"/> and <see cref="MBusOnDisableUnSubscriber"/>.
    /// </summary>
    public class MBusUnSubscriberBase : MonoBehaviour
    {
        internal object Handler;
        internal MBus Bus;
        internal Type Type;
        internal object Value;
        internal bool IsValueSubscription;
    }
}
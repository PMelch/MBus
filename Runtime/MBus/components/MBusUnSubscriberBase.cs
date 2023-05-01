using System;
using UnityEngine;

namespace MBus.components
{
    public class MBusUnSubscriberBase : MonoBehaviour
    {
        internal object Handler;
        internal MBus Bus;
        internal Type Type;
        internal object Value;
        internal bool IsValueSubscription;
    }
}
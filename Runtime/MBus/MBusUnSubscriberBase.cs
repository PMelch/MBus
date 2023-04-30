using System;
using UnityEngine;

namespace MBus
{
    public class MBusUnSubscriberBase : MonoBehaviour
    {
        internal object Handler;
        internal MBus Bus;
        internal Type Type;
    }
}
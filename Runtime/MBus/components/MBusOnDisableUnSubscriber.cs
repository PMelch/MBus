namespace MBus.components
{
    /// <summary>
    /// Helper component that will react to Unity's OnDestroy event to unregister a specific message handler.
    /// Do not manually add that to one of your GameObjects. It will be automatically added
    /// if you use MBus like this:
    /// <example>
    /// ...
    /// mbus.SubscribeUntilDisabled<object>(OnMessage, this)
    /// ...
    /// </example>
    /// </summary>
    public class MBusOnDisableUnSubscriber : MBusUnSubscriberBase 
    {
        private void OnDisable()
        {
            PerformUnsubscription();
            Destroy(this);
        }

        private void PerformUnsubscription()
        {
            if (IsValueSubscription)
            {
                Bus.Unsubscribe((System.Action)Handler, Value);
            }
            else
            {
                Bus.Unsubscribe(Type, Handler);
            }

            Handler = null;
        }
    }
}
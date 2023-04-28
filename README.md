# MBus
MBus is a message bus system for Unity that allows you to communicate between different components of your game using messages that are classes or structs. You can use MBus to send and subscribe to messages of any type, and easily manage the lifetime of your subscriptions. You can either manually subscribe and unsubscribe to messages, or use the scope feature to automatically unsubscribe when a MonoBehaviour is destroyed or disabled. MBus is lightweight, fast and easy to use, and it helps you keep your code clean and decoupled.


### Examples

- declare a new message type
~~~
class MyMessage : IMBusMessage 
{
    public int IntVal;
}
~~~    

- subscribe to messages of a specific type
~~~  
// subscribe to messages of type MyMessage
mBus.AddListener<MyMessage>(OnMyMessage);

// react to a received message of a specific type
void OnMyMessage(MyMessage message) 
{
    Debug.Log($"{message.IntVal}");
}

// send a custom message
mBus.SendMessage(new MyMessage(){ IntVal = 42; });
~~~      

- subscribe to all types of messages
~~~  
// subscribe to messages of any kind 
// and automatically unsubscribe if the object is disabled
mBus.AddListenerUntilDisabled<IMBusMessage>(OnMessage, this);

// generic handler receiving all message types
void OnMessage(IMBusMessage message) 
{
    Debug.Log($"{message.GetType()}");
}
~~~  
### Automatic vs manual unsubscribing
If you use AddListener(Action) you need 
to unsubscribe the listener manually using
~~~
mBus.RemoveListener(Action);
~~~
However, you can have MBus do that for you by specifying
when the unsubscription should happen, using one of the following methods instead:
~~~
// unsubscribe when the gameobject (this) gets disabled
mBus.AddListenerUntilDisabled<MessageType>(Action, this);

// ubsubscribe when the gameobject (this) gets destroyed
mBus.AddListenerUntilDestroyed<MessageType>(Action, this);
~~~



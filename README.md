# MBus
MBus is a message bus system for Unity that allows you to communicate between different components of your game using messages that are classes or structs. You can use MBus to send and subscribe to messages of any type, and easily manage the lifetime of your subscriptions. You can either manually subscribe and unsubscribe to messages, or use the scope feature to automatically unsubscribe when a MonoBehaviour is destroyed or disabled. MBus is lightweight, fast and easy to use, and it helps you keep your code clean and decoupled.


### Examples

- declare a new message type
~~~
class MyMessage 
{
    public int IntVal;
}
~~~    

- subscribe to messages of a specific type
~~~  
// subscribe to messages of type MyMessage
mBus.Subscribe<MyMessage>(OnMyMessage);

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
mBus.SubscribeUntilDisabled<object>(OnMessage, this);

// generic handler receiving all message types
void OnMessage(object message) 
{
    Debug.Log($"{message.GetType()}");
}

// send messages of any type
mBus.SendMessage("Foo");
mBus.SendMessage(42);
~~~  
### Automatic vs manual unsubscribing

If you use Subscribe(Action) you need 
to unsubscribe the listener manually using
~~~
mBus.Unsubscribe(Action);
~~~
However, you can let MBus do this for you by specifying when to unsubscribe, using one of the following methods instead:
~~~
// unsubscribe when the gameobject (this) gets disabled
mBus.SubscribeUntilDisabled<MessageType>(Action, this);

// ubsubscribe when the gameobject (this) gets destroyed
mBus.SubscribeUntilDestroyed<MessageType>(Action, this);
~~~

### Acquiring a MBus instance
The recommended way is to use some kind of dependency injection framework
([Zenject](https://github.com/modesttree/Zenject) or one of the [alternatives](href="https://www.libhunt.com/r/Zenject"/>)).

#### Example when using Zenject
In your installer code, create the MBus singleton
~~~
Container.Bind<MBus>().AsSingle();
~~~

Access the instance in an game object
~~~
class Player : MonoBehaviour 
{
    [Inject] private MBus _bus;
    
    public void OnTriggerEvent() 
    {
        _bus.SendMessage(new PlayerMessage());
    }
}
~~~


However, you can use different means to manage your MBus instance(s).
If you have only one global instance - for example in an DontDestroyOnLoad object - you
can use `MBusInstance` to hold and access it.

~~~
public class MBusObject : MonoBehaviour 
{
    private static GameObject _gameObject;
    private MBus _bus;
    
    private void Awake()
    {
        var go = gameObject;
        
        if (_gameObject != null)
        {
            Destroy(gameObject);
            return;
        }

        _gameObject = this;
        DontDestroyOnLoad(go);
        
        // set the global instance
        _bus = new MBus();
        MBusInstance.SetInstance(_bus);
    }
    
    // when your holder object gets destroyed, also remove the global MBus instance
    private void OnDestroy() 
    {
        MBusInstance.SetInstance(null);
    }
}


~~~

## Installation
Install MBus using Unity's package manager. Select "Add package from git URL" and enter
~~~
https://github.com/PMelch/MBus.git#<version>
~~~

Specify the version you want to install - for example 1.0.0.






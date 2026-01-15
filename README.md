![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/PMelch/MBus?logo=GitHub&style=flat-square)
![GitHub issues](https://img.shields.io/github/issues-raw/PMelch/MBus?style=flat-square)
![GitHub](https://img.shields.io/github/license/PMelch/MBus?color=%23cccc00&style=flat-square)
[![openupm](https://img.shields.io/npm/v/com.melchart.mbus?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.melchart.mbus)


# MBus - the Unity message bus

## MBus: Seamless Communication for Your Unity Project

Simplify inter-component communication in your Unity game with MBus, a lightweight and efficient message bus system. This powerful tool enables seamless exchange of data between different parts of your game using structured messages, whether they're classes or structs.

## Effortless Message Handling

MBus streamlines the process of sending and receiving messages of any type, ensuring seamless integration into your game's architecture. Customize your message handling by leveraging the flexible subscription mechanism, allowing you to manage subscriptions manually or leverage the scope feature for automatic unsubscription when a component is destroyed or disabled.

## Clean and Decoupled Code

MBus promotes clean and decoupled code, enhancing modularity and maintainability in your Unity projects. With MBus, you can focus on the core gameplay logic while the message bus handles the intricate communication between components.

## Key Features:

- Send and subscribe to messages of any type
- Manage subscriptions manually or with the scope feature
- Lightweight and efficient design with minimal overhead
- Simplifies communication between different Unity components
- Promotes clean and decoupled code structure
- Embrace Seamless Communication with MBus

Integrate MBus into your Unity game and experience the power of streamlined communication between components. MBus's lightweight and efficient design ensures a smooth and performant experience, enhancing your development workflow and game's overall structure.

## Examples

#### Subscribe to messages of a specific type
```C#
class MyMessage 
{
    public int IntVal;
}

// subscribe to messages of type MyMessage
mBus.Subscribe<MyMessage>(OnMyMessage);

// react to a received message of a specific type
void OnMyMessage(MyMessage message) 
{
    Debug.Log($"{message.IntVal}");
}

// send a custom message
mBus.SendMessage(new MyMessage(){ IntVal = 42; });
```      

##### Subscribe to all types of messages
```C# 
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
``` 

##### Value subscriptions

MBus can not only be used to subscribe to types, but also to specific values. 

```C#
void OnMessage(){}

mBus.Subscribe(OnMessage, 100);
mBus.Subscribe(OnMessage, "foo");
``` 
Use value subscription, if you prefer to use enum based message handling:
```C#
public enum MessageType 
{
   Type1,
   Type2,
   Type3
}

mBus.Subscribe(OnMessage, MessageType.Type1);
``` 

### Automatic vs manual unsubscribing

MBus offers two ways to manage message subscriptions: manual and automatic. When you use the `Subscribe(Action)` method, you are responsible for explicitly unsubscribing from the message using the `mBus.Unsubscribe(Action)` method.

However, if you want MBus to handle unsubscription for you, you can use the `SubscribeUntilDisabled<MessageType>(Action, Component)` or `SubscribeUntilDestroyed<MessageType>(Action, Component)` methods. These methods ensure that the listener is automatically unsubscribed when the GameObject to which it belongs is disabled or destroyed, respectively. This approach eliminates the need for manual unsubscription, making it more convenient and less error-prone.

In summary, manual unsubscription gives you more control over when a listener unsubscribes, while automatic unsubscription simplifies the process and ensures that listeners are automatically removed when the associated GameObject is no longer relevant.

```C#
// auto-unsubscribe when the gameobject (this) gets disabled
mBus.SubscribeUntilDisabled<MessageType>(Action, this);

// auto-unsubscribe when the gameobject (this) gets destroyed
mBus.SubscribeUntilDestroyed<MessageType>(Action, this);
``` 

### Obtaining a MBus instance
The preferred method for acquiring a MBus instance is to utilize a dependency injection (DI) framework like
([Zenject](https://github.com/modesttree/Zenject) or its [counterparts](href="https://www.libhunt.com/r/Zenject"/>)).
This approach streamlines dependency management and promotes code modularity.

#### Example using Zenject
In your Unity project's installer script, create a singleton instance of MBus using Zenject's DI syntax:
```C#
Container.Bind<MBus>().AsSingle();
``` 

Once the singleton instance is established, your MonoBehaviour classes can access it through dependency injection:
```C#
class Player : MonoBehaviour 
{
    [Inject] private MBus _bus;
    
    public void OnTriggerEvent() 
    {
        _bus.SendMessage(new PlayerMessage());
    }
}
``` 
By injecting the MBus instance through DI, you achieve loose coupling and promote code reusability, making your project more maintainable and adaptable to future changes.

#### Via game object
While dependency injection is the recommended approach for managing MBus instances, you can also employ other methods, such as utilizing the `MBusInstance` class. This class acts as a central repository for accessing the global MBus instance, particularly if you have a single instance across your project.

To leverage `MBusInstance`, you can instantiate it within a DontDestroyOnLoad MonoBehaviour. This ensures that the instance persists throughout the game's lifecycle. 

```C#
public class MBusObject : MonoBehaviour 
{
    private static GameObject _instance;
    
    private void Awake()
    {
        var go = gameObject;
        
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(go);
        
        // set the global instance
        MBusInstance.SetInstance(new MBus());
    }
    
    // when your holder object gets destroyed, also remove the global MBus instance
    private void OnDestroy() 
    {
        MBusInstance.SetInstance(null);
    }
}

``` 

Once created, you can access the global MBus instance using the `MBusInstance.SendMessage()` method:
```C#
MBusInstance.SendMessage("foo");
``` 
By utilizing `MBusInstance`, you can manage the global MBus instance more directly, particularly if you have a centralized communication hub.

## Installation
### OpenUPM

The package is available on the [openupm registry](https://openupm.com). It's recommended to install it via [openupm-cli](https://github.com/openupm/openupm-cli).

~~~bash
openupm add com.melchart.mbus
~~~

### Git URL
You can also install MBus using Unity's package manager. Select "Add package from git URL" and enter
~~~
https://github.com/PMelch/MBus.git#v<version>
~~~

Specify the version you want to install - for example for version 1.0.0 you would use https://github.com/PMelch/MBus.git#v1.0.0






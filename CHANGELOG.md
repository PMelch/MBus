Changelog
=========

### 1.1.0-beta.1
- added value subscriptions: it is possible now to subscribe to a specific value instead of just a type.
~~~
void OnValue() 
{
}

bus.Subscribe(OnValue, 100);

// will trigger
bus.SendMessage(100);

// won't trigger
bus.SendMessage(101);

// unsubscripe
bus.Unsubscribe(OnValue, 100);
~~~
Value subscriptions will not receive a parameter.

### 1.0.0 
- Initial release
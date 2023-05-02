Changelog
=========

### 1.1.0
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

// unsubscribe
bus.Unsubscribe(OnValue, 100);
~~~
Value subscriptions will not receive a parameter.

One callback method can be used to subscribe to multiple explicit values.


### 1.0.0 
- Initial release
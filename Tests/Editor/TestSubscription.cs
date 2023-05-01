using System;
using System.Collections.Generic;
using System.Reflection;
using MBus.components;
using NUnit.Framework;
using UnityEngine;

namespace Tests.Editor
{
    public class TestSubscription
    {
        [Test]
        public void TestSubscriptionForType()
        {
            var bus = new MBus.MBus();

            var stringCalled = 0;
            var intCalled = 0;

            void OnMessageString(string value)
            {
                stringCalled++;
            }

            void OnMessageInt(int value)
            {
                intCalled++;
            }

            bus.Subscribe<string>(OnMessageString);
            bus.Subscribe<int>(OnMessageInt);
            bus.SendMessage("foo");
            Assert.AreEqual(1, stringCalled);
            Assert.AreEqual(0, intCalled);
        }

        [Test]
        public void TestSubscriptionForTypePlusForObject()
        {
            var bus = new MBus.MBus();

            var called = 0;

            void OnMessage(object value)
            {
                called++;
            }

            bus.Subscribe<object>(OnMessage);
            bus.Subscribe<string>(OnMessage);
            bus.SendMessage("foo");
            Assert.AreEqual(2, called);
        }

        [Test]
        public void TestValueSubscription()
        {
            var bus = new MBus.MBus();

            var calledFoo = 0;
            var calledBar = 0;

            void OnMessageFoo()
            {
                calledFoo++;
            }
            void OnMessageBar()
            {
                calledBar++;
            }

            bus.Subscribe(OnMessageFoo, "foo");
            bus.Subscribe(OnMessageBar, "bar");
            bus.SendMessage("foo");
            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(2, calledFoo);
            Assert.AreEqual(1, calledBar);
        }

        [Test]
        public void TestSendingWhileReceiving()
        {
            var bus = new MBus.MBus();

            var valuesReceived = new List<string>();

            void OnMessage1(string value)
            {
                valuesReceived.Add(value);
                if (value == "foo")
                {
                    bus.SendMessage("bar");
                }
            }

            void OnMessage2(string value)
            {
                valuesReceived.Add(value);
            }

            bus.Subscribe<string>(OnMessage1);
            bus.Subscribe<string>(OnMessage2);
            bus.SendMessage("foo");
            Assert.AreEqual(new[] { "foo", "foo", "bar", "bar" }, valuesReceived);
        }

        private class Base
        {
        }

        private class Sub1 : Base
        {
        }

        private class Sub2 : Base
        {
        }

        private class Sub3 : Sub2
        {
        }

        [Test]
        public void TestSendingSubtypes()
        {
            var bus = new MBus.MBus();
            var baseReceived = 0;
            var sub1Received = 0;
            var sub2Received = 0;
            var sub3Received = 0;
        
        
            void OnMessageSub2(Sub2 message)
            {
                ++sub2Received;
            }

            void OnMessageSub1(Sub1 message)
            {
                ++sub1Received;
            }

            void OnMessageSub3(Sub3 message)
            {
                ++sub3Received;
            }

            void OnMessageBase(Base message)
            {
                ++baseReceived;
            }


            bus.Subscribe<Base>(OnMessageBase);
            bus.Subscribe<Sub1>(OnMessageSub1);
            bus.Subscribe<Sub2>(OnMessageSub2);
            bus.Subscribe<Sub3>(OnMessageSub3);
            bus.SendMessage(new Sub3());
            Assert.AreEqual(1, sub3Received);
            Assert.AreEqual(1, sub2Received);
            Assert.AreEqual(1, baseReceived);
            Assert.AreEqual(0, sub1Received);
        }

        [Test]
        public void TestUnsubscription()
        {
            var bus = new MBus.MBus();

            var sCalled = 0;
            void OnString(string value)
            {
                ++sCalled;
            }
            var vCalled = 0;
            void OnValue()
            {
                ++vCalled;
            }

            bus.Subscribe<string>(OnString);
            bus.Subscribe(OnValue, "foo");
            bus.SendMessage("foo");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(1, vCalled);
            
            bus.Unsubscribe<string>(OnString);
            bus.SendMessage("foo");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(2, vCalled);
            
            bus.Unsubscribe(OnValue, "foo");
            bus.SendMessage("foo");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(2, vCalled);
        }
        
        [Test]
        public void TestMultipleValueSubscriptionWithSameCallback()
        {
            var bus = new MBus.MBus();

            var vCalled = 0;
            void OnValue()
            {
                ++vCalled;
            }

            bus.Subscribe(OnValue, "foo");
            bus.Subscribe(OnValue, "bar");
            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(2, vCalled);
            
            bus.Unsubscribe(OnValue, "foo");
            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(3, vCalled);
            
            bus.Unsubscribe(OnValue, "bar");
            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(3, vCalled);
        }

        [Test]
        public void TestAutoUnsubscribeOnDisable()
        {
            var bus = new MBus.MBus();

            var vCalled = 0;
            void OnValue()
            {
                ++vCalled;
            }

            var sCalled = 0;
            void OnString(string value)
            {
                ++sCalled;
            }

            var gameObject = new GameObject();
            var transform = gameObject.AddComponent<RectTransform>();
            bus.SubscribeUntilDisabled(OnValue, "foo", transform);
            bus.SubscribeUntilDisabled<string>(OnString, transform);

            var autoUnSubscriberComponents = gameObject.GetComponents<MBusOnDisableUnSubscriber>();
            Assert.AreEqual(2, autoUnSubscriberComponents.Length);
            
            bus.SendMessage("foo");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(1, vCalled);

            Type type = typeof(MBusOnDisableUnSubscriber);
            var methodInfo = type.GetMethod("PerformUnsubscription", BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(methodInfo);
            foreach (var component in autoUnSubscriberComponents)
            {
                methodInfo.Invoke(component,Array.Empty<object>());
            }

            bus.SendMessage("foo");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(1, vCalled);
        }
        
        [Test]
        public void TestAutoUnsubscribeOnDestroy()
        {
            var bus = new MBus.MBus();

            var vCalled = 0;
            void OnValue()
            {
                ++vCalled;
            }

            var sCalled = 0;
            void OnString(string value)
            {
                ++sCalled;
            }

            var gameObject = new GameObject();
            var transform = gameObject.AddComponent<RectTransform>();
            bus.SubscribeUntilDestroyed(OnValue, "foo", transform);
            bus.Subscribe(OnValue, "bar");
            bus.SubscribeUntilDestroyed<string>(OnString, transform);

            var autoUnSubscriberComponents = gameObject.GetComponents<MBusOnDestroyUnSubscriber>();
            Assert.AreEqual(2, autoUnSubscriberComponents.Length);
            
            bus.SendMessage("foo");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(1, vCalled);

            Type type = typeof(MBusOnDestroyUnSubscriber);
            var methodInfo = type.GetMethod("PerformUnsubscription", BindingFlags.NonPublic | BindingFlags.Instance);
            
            Assert.NotNull(methodInfo);
            foreach (var component in autoUnSubscriberComponents)
            {
                methodInfo.Invoke(component,Array.Empty<object>());
            }

            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(1, sCalled);
            Assert.AreEqual(2, vCalled);
        }
    }
}
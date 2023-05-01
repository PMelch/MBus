using System.Collections.Generic;
using NUnit.Framework;

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
            Assert.AreEqual(stringCalled, 1);
            Assert.AreEqual(intCalled, 0);
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
            Assert.AreEqual(called, 2);
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
            Assert.AreEqual(calledFoo, 2);
            Assert.AreEqual(calledBar, 1);
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
            Assert.AreEqual(valuesReceived, new[] { "foo", "foo", "bar", "bar" });
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
            Assert.AreEqual(sub3Received, 1);
            Assert.AreEqual(sub2Received, 1);
            Assert.AreEqual(baseReceived, 1);
            Assert.AreEqual(sub1Received, 0);
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
            Assert.AreEqual(sCalled, 1);
            Assert.AreEqual(vCalled, 1);
            
            bus.Unsubscribe<string>(OnString);
            bus.SendMessage("foo");
            Assert.AreEqual(sCalled, 1);
            Assert.AreEqual(vCalled, 2);
            
            bus.Unsubscribe(OnValue, "foo");
            bus.SendMessage("foo");
            Assert.AreEqual(sCalled, 1);
            Assert.AreEqual(vCalled, 2);
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
            Assert.AreEqual(vCalled, 2);
            
            bus.Unsubscribe(OnValue, "foo");
            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(vCalled, 3);
            
            bus.Unsubscribe(OnValue, "bar");
            bus.SendMessage("foo");
            bus.SendMessage("bar");
            Assert.AreEqual(vCalled, 3);
        }
    }
}
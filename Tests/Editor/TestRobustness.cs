using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests.Editor
{
    public class TestRobustness
    {
        [Test]
        public void TestExceptionIsolation()
        {
            var bus = new MBus.MBus();
            var handler1Called = false;
            var handler2Called = false;

            void FailingHandler(string msg)
            {
                handler1Called = true;
                throw new Exception("This handler failed intentionally.");
            }

            void WorkingHandler(string msg)
            {
                handler2Called = true;
            }

            bus.Subscribe<string>(FailingHandler);
            bus.Subscribe<string>(WorkingHandler);

            // We expect the log to catch the exception, but the test shouldn't crash
            // and the second handler MUST be called.
            LogAssert.Expect(LogType.Log, "Got an error while invoking message handler for System.String:test");
            LogAssert.Expect(LogType.Log, "This handler failed intentionally.");
            
            bus.SendMessage("test");

            Assert.IsTrue(handler1Called, "Failing handler should have been called.");
            Assert.IsTrue(handler2Called, "Working handler should have been called despite previous failure.");
        }

        [Test]
        public void TestUnsubscribeInHandler()
        {
            var bus = new MBus.MBus();
            var handlerCalledCount = 0;

            void SelfUnsubscribingHandler(string msg)
            {
                handlerCalledCount++;
                // Unsubscribe self
                bus.Unsubscribe<string>(SelfUnsubscribingHandler);
            }

            bus.Subscribe<string>(SelfUnsubscribingHandler);
            
            bus.SendMessage("first");
            Assert.AreEqual(1, handlerCalledCount, "Handler should be called once.");

            bus.SendMessage("second");
            Assert.AreEqual(1, handlerCalledCount, "Handler should NOT be called again after unsubscription.");
        }
        
        [Test]
        public void TestUnsubscribeNonExistent()
        {
            var bus = new MBus.MBus();
            
            Action<string> someHandler = (msg) => {};
            Action someValueHandler = () => {};

            // Should not throw
            Assert.DoesNotThrow(() => bus.Unsubscribe<string>(someHandler));
            Assert.DoesNotThrow(() => bus.Unsubscribe<string>(someValueHandler, "someValue"));
        }

        [Test]
        public void TestMBusInstance()
        {
            var bus = new MBus.MBus();
            MBus.MBusInstance.SetInstance(bus);

            var received = false;
            void Handler(string msg)
            {
                received = true;
            }

            MBus.MBusInstance.Subscribe<string>(Handler);
            MBus.MBusInstance.SendMessage("test");

            Assert.IsTrue(received);

            received = false;
            MBus.MBusInstance.Unsubscribe<string>(Handler);
            MBus.MBusInstance.SendMessage("test");
            Assert.IsFalse(received);

            // Cleanup
            MBus.MBusInstance.SetInstance(null);
        }
    }
}

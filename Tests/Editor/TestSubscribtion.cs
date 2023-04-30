using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TestSubscribtion
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

        var called = 0;
        void OnMessage()
        {
            called++;
        }

        bus.Subscribe(OnMessage, "foo");
        bus.SendMessage("foo");
        Assert.AreEqual(called, 1);
    }
}

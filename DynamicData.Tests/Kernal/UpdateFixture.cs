﻿using System;
using DynamicData.Kernel;
using DynamicData.Tests.Domain;
using NUnit.Framework;

namespace DynamicData.Tests.Kernal
{
    [TestFixture]
    public class UpdateFixture
    {
        [Test]
        public void Add()
        {
            var person = new Person("Person", 10);
            var update = new Change<Person, string>(ChangeReason.Add, "Person", person);

            Assert.AreEqual("Person", update.Key);
            Assert.AreEqual(ChangeReason.Add, update.Reason);
            Assert.AreEqual(person, update.Current);
            Assert.AreEqual(Optional.None<Person>(), update.Previous);
        }

        [Test]
        public void Remove()
        {
            var person = new Person("Person", 10);
            var update = new Change<Person, string>(ChangeReason.Remove, "Person", person);

            Assert.AreEqual("Person", update.Key);
            Assert.AreEqual(ChangeReason.Remove, update.Reason);
            Assert.AreEqual(person, update.Current);
            Assert.AreEqual(Optional.None<Person>(), update.Previous);
        }

        [Test]
        public void Update()
        {
            var current = new Person("Person", 10);
            var previous = new Person("Person", 9);
            var update = new Change<Person, string>(ChangeReason.Update, "Person", current, previous);

            Assert.AreEqual("Person", update.Key);
            Assert.AreEqual(ChangeReason.Update, update.Reason);
            Assert.AreEqual(current, update.Current);
            Assert.IsTrue(update.Previous.HasValue);
            Assert.AreEqual(previous, update.Previous.Value);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void UpdateWillThrowIfNoPreviousValueIsSupplied()
        {
            var current = new Person("Person", 10);
            var update = new Change<Person, string>(ChangeReason.Update, "Person", current);
        }
    }
}
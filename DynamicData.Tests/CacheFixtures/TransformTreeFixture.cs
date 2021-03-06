﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace DynamicData.Tests.CacheFixtures
{
    public class TransformTreeFixture
    {
        private ISourceCache<EmployeeDto, int> _sourceCache;
        private IObservableCache<Node<EmployeeDto, int>, int> _result;

        [SetUp]
        public void SetUp()
        {
            _sourceCache =  new SourceCache<EmployeeDto, int>(e => e.Id);

            _result = _sourceCache.Connect()
                            .TransformToTree(e => e.BossId)
                            .AsObservableCache();
        }

        [TearDown]
        public void TearDown()
        {
            _sourceCache.Dispose();
            _result.Dispose();
        }

        [Test]
        public void BuildTreeFromMixedData()
        {
            _sourceCache.AddOrUpdate(CreateEmployees());
            Assert.AreEqual(2, _result.Count);

            var firstNode = _result.Items.First();
            Assert.AreEqual(3, firstNode.Children.Count);
            
            var secondNode = _result.Items.Skip(1).First();
            Assert.AreEqual(0, secondNode.Children.Count);
        }

        [Test]
        public void UpdateAParentNode()
        {
            _sourceCache.AddOrUpdate(CreateEmployees());

            var changed = new EmployeeDto(1)
            {
                BossId = 0,
                Name = "Employee 1 (with name change)"
            };

            _sourceCache.AddOrUpdate(changed);
            Assert.AreEqual(2, _result.Count);

            var firstNode = _result.Items.First();
            Assert.AreEqual(3, firstNode.Children.Count);
            Assert.AreEqual(changed.Name, firstNode.Item.Name);

        }

        [Test]
        public void UpdateChildNode()
        {
            _sourceCache.AddOrUpdate(CreateEmployees());


            var changed = new EmployeeDto(2)
            {
                BossId =1,
                Name = "Employee 2 (with name change)"
            };

            _sourceCache.AddOrUpdate(changed);
            Assert.AreEqual(2, _result.Count);

            var changedNode = _result.Items.First().Children.Items.First();

            Assert.AreEqual(1, changedNode.Parent.Value.Item.Id);
            Assert.AreEqual(1, changedNode.Children.Count);
            Assert.AreEqual(changed.Name, changed.Name);
        }

        [Test]
        public void RemoveARootNodeWillPushOrphansUpTheHierachy()
        {
            _sourceCache.AddOrUpdate(CreateEmployees());
            _sourceCache.Remove(1);

            //we expect the original children nodes to be pushed up become new roots
            Assert.AreEqual(4, _result.Count);
        }

        [Test]
        public void RemoveAChildNodeWillPushOrphansUpTheHierachy()
        {
            _sourceCache.AddOrUpdate(CreateEmployees());
            _sourceCache.Remove(4);

            //we expect the children of node 4  to be pushed up become new roots
            Assert.AreEqual(3, _result.Count);

            var thirdNode = _result.Items.Skip(2).First();
            Assert.AreEqual(5, thirdNode.Key);
        }

        [Test]
        public void AddMissingChild()
        {
            var boss = new EmployeeDto(2) {BossId = 0, Name = "Boss"};
            var minion = new EmployeeDto(1) { BossId = 2, Name = "DogsBody" };
            _sourceCache.AddOrUpdate(boss);
            _sourceCache.AddOrUpdate(minion);

            Assert.AreEqual(1, _result.Count);

            var firstNode = _result.Items.First();
            Assert.AreEqual(boss, firstNode.Item);

            var childNode = firstNode.Children.Items.First();
            Assert.AreEqual(minion, childNode.Item);
        }

        [Test]
        public void AddMissingParent()
        {
            var minion = new EmployeeDto(1) { BossId = 2, Name = "DogsBody" };
            var boss = new EmployeeDto(2) { BossId = 0, Name = "Boss" };

            _sourceCache.AddOrUpdate(boss);
            _sourceCache.AddOrUpdate(minion);

            Assert.AreEqual(1, _result.Count);

            var firstNode = _result.Items.First();
            Assert.AreEqual(boss, firstNode.Item);

            var childNode = firstNode.Children.Items.First();
            Assert.AreEqual(minion, childNode.Item);
        }

        [Test]
        public void ChangeParent()
        {
            _sourceCache.AddOrUpdate(CreateEmployees());

            _sourceCache.AddOrUpdate(new EmployeeDto(4)
            {
                BossId = 1,
                Name = "Employee4"
            });


            //if this throws, then employee 4 is no a child of boss 1
            var emp4 = _result.Lookup(1).Value.Children.Lookup(4).Value;

            //check boss is = 1
            Assert.AreEqual(1, emp4.Parent.Value.Item.Id);

            //lookup previous boss (emp 4 should no longet be a child)
            var emp3 = _result.Lookup(1).Value.Children.Lookup(3).Value;


            //emp 4 must be removed from previous boss's child collection
            Assert.IsFalse( emp3.Children.Lookup(4).HasValue);
        }

        [Test]
        public void AddParent()
        {
            _sourceCache.AddOrUpdate(new EmployeeDto(1) { BossId = 2, Name = "E1" });
            _sourceCache.AddOrUpdate(new EmployeeDto(2) { BossId = 1, Name = "E2" });


            //we expect the children of node 4  to be pushed up become new roots
            Assert.AreEqual(1, _result.Count);

        }

        #region Employees
        private IEnumerable<EmployeeDto> CreateEmployees()
        {
            yield return new EmployeeDto(1)
            {
                BossId = 0,
                Name = "Employee1"
            };

            yield return new EmployeeDto(2)
            {
                BossId = 1,
                Name = "Employee2"
            };

            yield return new EmployeeDto(3)
            {
                BossId = 1,
                Name = "Employee3"
            };

            yield return new EmployeeDto(4)
            {
                BossId = 3,
                Name = "Employee4"
            };

            yield return new EmployeeDto(5)
            {
                BossId = 4,
                Name = "Employee5"
            };

            yield return new EmployeeDto(6)
            {
                BossId = 2,
                Name = "Employee6"
            };

            yield return new EmployeeDto(7)
            {
                BossId = 0,
                Name = "Employee7"
            };

            yield return new EmployeeDto(8)
            {
                BossId = 1,
                Name = "Employee8"
            };


        }

        public class EmployeeDto : IEquatable<EmployeeDto>
        {

            public EmployeeDto(int id)
            {
                Id = id;
            }

            public int Id { get; set; }
            public int BossId { get; set; }
            public string Name { get; set; }

            #region Equality Members

            public bool Equals(EmployeeDto other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((EmployeeDto)obj);
            }

            public override int GetHashCode()
            {
                return Id;
            }

            public static bool operator ==(EmployeeDto left, EmployeeDto right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(EmployeeDto left, EmployeeDto right)
            {
                return !Equals(left, right);
            }

            #endregion



            public override string ToString()
            {
                return $"Name: {Name}, Id: {Id}, BossId: {BossId}";
            }
        }


        #endregion


    }
}

using System;
using System.Collections.Generic;

namespace PhilApprovalFlow.Tests.Entities
{
    public class TestEntity : IApprovalFlow<TestEntityTransition>
    {
        public TestEntity()
        {
            Transitions = new HashSet<TestEntityTransition>();
            TestEntityID = Guid.NewGuid();

        }
        public Guid TestEntityID { get; set; }
        public ICollection<TestEntityTransition> Transitions { get; set; }

        public object GetID()
        {
            return TestEntityID;
        }

        public string GetLongDescription()
        {
            return "Test Long Description";
        }

        public string GetShortDescription()
        {
            return "Test Short Description";
        }
    }
}
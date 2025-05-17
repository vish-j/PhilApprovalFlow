using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Models;
using PhilApprovalFlow.Tests.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow.Tests
{
    [TestClass]
    public class EdgeCaseTests
    {
        [TestMethod]
        public void EmptyTransitionsTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            
            // Test various methods with empty transitions
            Assert.IsFalse(entity.Transitions.IsApproved());
            Assert.IsFalse(entity.Transitions.IsAnyApproved());
            Assert.IsFalse(entity.Transitions.IsAnyDecisionPending());
            Assert.IsFalse(entity.Transitions.IsInTransitions("User1"));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NoExistingTransitionTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            
            // Try to approve a non-existent transition
            workflow.Approve();
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullApproverTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            string approver = null;
            // Try to request approval with null approver
            workflow.RequestApproval(approver, "Reviewer");
        }
        
        [TestMethod]
        public void MetadataKeyExistsTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            
            // Set metadata with the same key twice
            workflow.SetMetadata("TestKey", "Value1");
            workflow.SetMetadata("TestKey", "Value2");
            
            // Verify the value was updated, not duplicated
            Assert.AreEqual("Value2", workflow.GetMetadata("TestKey"));
        }
        
        [TestMethod]
        [ExpectedException(typeof(KeyNotFoundException))]
        public void MetadataKeyNotFoundTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            
            // Try to get non-existent metadata
            workflow.GetMetadata("NonExistentKey");
        }
    }
}
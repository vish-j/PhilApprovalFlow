using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhilApprovalFlow.Tests.Entities;
using System;
using System.Diagnostics;
using System.Linq;

namespace PhilApprovalFlow.Tests
{
    [TestClass]
    public class PerformanceTests
    {
        [TestMethod]
        public void LargeWorkflowTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            
            // Add many transitions
            for (int i = 0; i < 100; i++)
            {
                workflow.RequestApproval($"User{i+2}", "Reviewer");
            }
            
            // Verify performance remains acceptable
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            var isAnyPending = entity.Transitions.IsAnyDecisionPending();
            
            stopwatch.Stop();
            
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100, $"Operation took too long for large workflow: {stopwatch.ElapsedMilliseconds}ms");
            Assert.IsTrue(isAnyPending);
        }
        
        [TestMethod]
        public void MultipleOperationsTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow();
            
            // Add 50 transitions
            for (int i = 0; i < 50; i++)
            {
                workflow.SetUserName("User1").RequestApproval($"User{i+2}", "Reviewer");
            }
            
            // Approve first 25
            for (int i = 0; i < 25; i++)
            {
                workflow.SetUserName($"User{i+2}").Approve();
            }
            
            // Measure performance of checking all approved transitions
            Stopwatch stopwatch = new();
            stopwatch.Start();
            
            var approvedCount = entity.Transitions.Count(t => t.ApproverDecision == Enum.DecisionType.Approved);
            
            stopwatch.Stop();
            
            Assert.AreEqual(25, approvedCount);
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50, $"Count operation took too long: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
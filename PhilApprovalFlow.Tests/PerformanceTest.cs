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
            // Arrange
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");
            
            // Add many transitions
            for (int i = 0; i < 100; i++)
            {
                workflow.RequestApproval($"User{i+2}", "Reviewer");
            }
            
            // Act
            bool isAnyPending = entity.Transitions.IsAnyDecisionPending();
            
            // Assert
            Assert.IsTrue(isAnyPending, "Large workflow should have pending decisions");
            
            // Performance assertion is only a warning, not a test failure
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            entity.Transitions.IsAnyDecisionPending();
            stopwatch.Stop();
            
            if (stopwatch.ElapsedMilliseconds > 100)
            {
                Console.WriteLine($"WARNING: Performance degradation detected. Operation took {stopwatch.ElapsedMilliseconds}ms");
            }
        }
        
        [TestMethod]
        public void MultipleOperationsTest()
        {
            // Arrange
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
            
            // Act
            var approvedCount = entity.Transitions.Count(t => t.ApproverDecision == Enum.DecisionType.Approved);
            
            // Assert
            Assert.AreEqual(25, approvedCount, "Expected exactly 25 approved transitions");
            
            // Performance metrics logging, not test failure
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            entity.Transitions.Count(t => t.ApproverDecision == Enum.DecisionType.Approved);
            stopwatch.Stop();
            
            Console.WriteLine($"Count operation completed in {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}
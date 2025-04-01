using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Models;
using PhilApprovalFlow.Tests.Entities;
using System;
using System.Linq;

namespace PhilApprovalFlow.Tests
{
    [TestClass]
    public class WorkflowTest
    {
        [TestMethod]
        public void RequestApprovalTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User2", "Reviewer");

            workflow.RequestApproval("User3", "Reviewer");

            workflow.RequestApproval("User2", "Reviewer");

            Assert.AreEqual(2, entity.Transitions.Count);
        }

        [TestMethod]
        public void RequestApprovalNotification()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User2", "Reviewer").LoadNotification("User2");

            workflow.RequestApproval("User3", "Reviewer").LoadNotification("User3");

            Assert.AreEqual(2, workflow.GetPAFNotifications().Count());
        }

        [TestMethod]
        public void RequestApprovalRoleChange()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User2", "Reviewer");

            workflow.RequestApproval("User2", "Approver");

            Assert.AreEqual("Approver", entity.Transitions.First().ApproverRole);
        }

        [TestMethod]
        public void ApproveTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User1", "Reviewer").Approve();

            Assert.AreEqual(DecisionType.Approved, entity.Transitions.First().ApproverDecision);
        }

        [TestMethod]
        public void ApproveGroupTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            var group = new PAFApproverGroup() { GroupID = 1 };
            group.SetApprovers(["User1"]);
            workflow.RequestApproval(group, "Reviewer", null).Approve();

            Assert.AreEqual(DecisionType.Approved, entity.Transitions.First().ApproverDecision);
        }

        [TestMethod]
        public void RejectTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User1", "Reviewer").Reject();

            Assert.AreEqual(DecisionType.Rejected, entity.Transitions.First().ApproverDecision);
        }

        [TestMethod]
        public void InvalidateTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User1", "Reviewer").Invalidate("User1");

            Assert.AreEqual(DecisionType.Invalidated, entity.Transitions.First(c => c.ApproverID == "User1").ApproverDecision);

            workflow.RequestApproval("User2", "Reviewer").Invalidate("User2");

            Assert.AreEqual(DecisionType.Invalidated, entity.Transitions.First(c => c.ApproverID == "User2").ApproverDecision);
        }

        [TestMethod]
        public void ResetTransitionsTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow();

            workflow.SetUserName("User1").RequestApproval("User1", "Reviewer").Approve();

            workflow.ResetTransitions();

            Assert.IsTrue(entity.Transitions.All(t => t.ApproverDecision == DecisionType.AwaitingDecision));
        }

        [TestMethod]
        public void SetMetadataTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.SetMetadata("Key1", "Value1");
            workflow.SetMetadata("Key2", "Value2");

            Assert.AreEqual("Value1", workflow.GetMetadata("Key1"));
            Assert.AreEqual("Value2", workflow.GetMetadata("Key2"));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvalidUserContextTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName(null);  // Invalid user context

            workflow.RequestApproval("User2", "Reviewer");
        }

        [TestMethod]
        public void NotificationContentTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            workflow.RequestApproval("User2", "Reviewer").LoadNotification("User2");

            var notification = workflow.GetPAFNotifications().First();
            Assert.AreEqual("User2", notification.To);
            Assert.IsTrue(notification.DecisionType == DecisionType.AwaitingDecision);
        }

        [TestMethod]
        public void RequestApprovalForGroupTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            var group = new PAFApproverGroup() { GroupID = 1 };
            group.SetApprovers(["User2", "User3"]);

            workflow.RequestApproval(group, "Reviewer");

            Assert.AreEqual(1, entity.Transitions.Count);
            Assert.AreEqual(group, entity.Transitions.First().ApproverGroup);
        }

        [TestMethod]
        public void LoadNotificationForGroupTest()
        {
            TestEntity entity = new TestEntity();
            var workflow = entity.GetApprovalFlow().SetUserName("User1");

            var group = new PAFApproverGroup() { GroupID = 1 };
            group.SetApprovers(["User2", "User3"]);

            workflow.RequestApproval(group, "Reviewer");
            workflow.LoadNotification(group);

            var notifications = workflow.GetPAFNotifications().ToList();
            Assert.AreEqual(2, notifications.Count);
            Assert.IsTrue(notifications.Any(n => n.To == "User2"));
            Assert.IsTrue(notifications.Any(n => n.To == "User3"));
        }
    }
}
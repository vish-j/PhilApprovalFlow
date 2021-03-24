using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Tests.Entities;
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

            Assert.AreEqual(DecisionType.Invalidated, entity.Transitions.First().ApproverDecision);
        }
    }
}
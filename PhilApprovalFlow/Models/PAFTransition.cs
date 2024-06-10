using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Interfaces;
using System;

namespace PhilApprovalFlow.Models
{
    public abstract class PAFTransition : IPAFTransition
    {
        public PAFTransition()
        {
        }

        public PAFTransition(PAFTransition t)
        {
            TransitionID = t.TransitionID;
            Order = t.Order;
            RequesterID = t.RequesterID;
            RequestedDate = t.RequestedDate;
            ApproverID = t.ApproverID;
            ApproverRole = t.ApproverRole;
            ApproverDecision = t.ApproverDecision;
            RequesterComments = t.RequesterComments;
            ApproverComments = t.ApproverComments;
            ApproverCheckInDate = t.ApproverCheckInDate;
            AcknowledgementDate = t.AcknowledgementDate;
        }

        public Guid TransitionID { get; set; }

        public int Order { get; set; }

        public string ApproverID { get; set; }

        public string ApproverRole { get; set; }

        public PAFApproverGroup ApproverGroup { get; set; }

        public DateTime? ApproverCheckInDate { get; set; }
        public DateTime? AcknowledgementDate { get; set; }

        public DecisionType ApproverDecision { get; set; }

        public string ApproverComments { get; set; }

        public string RequesterID { get; set; }
        public DateTime RequestedDate { get; set; }

        public string RequesterComments { get; set; }

        public bool IsCheckedIn => ApproverCheckInDate != null;

        public void Initalize(int order, string requester, string approver, string role, string comments)
        {
            Order = order;
            RequesterID = requester;
            RequestedDate = DateTime.Now;
            ApproverID = approver;
            ApproverRole = role;
            ApproverDecision = DecisionType.AwaitingDecision;
            RequesterComments = comments;
        }

        public void Initalize(int order, string requester, IPAFGroup group, string role, string comments)
        {
            Order = order;
            RequesterID = requester;
            RequestedDate = DateTime.Now;
            ApproverGroup = group as PAFApproverGroup;
            ApproverRole = role;
            ApproverDecision = DecisionType.AwaitingDecision;
            RequesterComments = comments;
        }
    }
}
using PhilApprovalFlow.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace PhilApprovalFlow.Models
{
    public abstract class PAFTransition : ITransition
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
            ApproverDecision = t.ApproverDecision;
            RequesterComments = t.RequesterComments;
            ApproverComments = t.ApproverComments;
            AcknowledgementDate = t.AcknowledgementDate;
        }

        [Key]
        public Guid TransitionID { get; set; }

        public int Order { get; set; }

        public string ApproverID { get; set; }
        public DateTime? AcknowledgementDate { get; set; }

        public DecisionType ApproverDecision { get; set; }

        public string ApproverComments { get; set; }

        public string RequesterID { get; set; }
        public DateTime RequestedDate { get; set; }

        public string RequesterComments { get; set; }

        public void Initalize(int order, string requester, string approver, string comments)
        {
            Order = order;
            RequesterID = requester;
            RequestedDate = DateTime.Now;
            ApproverID = approver;
            ApproverDecision = DecisionType.AwaitingDecision;
            RequesterComments = comments;
        }
    }
}
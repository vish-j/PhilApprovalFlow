using PhilApprovalFlow.Enum;
using System;
using System.ComponentModel.DataAnnotations;

namespace PhilApprovalFlow.Models
{
    public class Transition : ITransition
    {
        public Transition()
        {
        }

        public Transition(Transition t)
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
    }
}
using PhilApprovalFlow.Enum;
using System;

namespace PhilApprovalFlow
{
    public interface ITransition
    {
        Guid TransitionID { get; set; }
        int Order { get; set; }
        string ApproverID { get; set; }
        DecisionType ApproverDecision { get; set; }
        DateTime? AcknowledgementDate { get; set; }
        string ApproverComments { get; set; }
        string RequesterID { get; set; }
        DateTime RequestedDate { get; set; }
        string RequesterComments { get; set; }
        void Initalize(int order, string requester, string approver, string comments);
    }
}
using System.ComponentModel.DataAnnotations;

namespace PhilApprovalFlow.Enum
{
    public enum DecisionType
    {
        [Display(Name= "Awaiting Decision")]
        AwaitingDecision,
        Approved,
        Rejected,
        Invalidated
    }
}
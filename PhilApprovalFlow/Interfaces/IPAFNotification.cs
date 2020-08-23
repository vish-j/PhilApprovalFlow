using PhilApprovalFlow.Enum;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface IPAFNotification
    {
        string Comments { get; set; }
        DecisionType DecisionType { get; set; }
        string From { get; set; }
        string To { get; set; }
        IEnumerable<string> UsersToCC { get; set; }
    }
}
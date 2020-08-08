using PhilApprovalFlow.Enum;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public class PAFNotification
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Comments { get; set; }
        public DecisionType DecisionType { get; set; }
        public IEnumerable<string> UsersToCC { get; set; }
    }
}
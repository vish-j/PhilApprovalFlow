using PhilApprovalFlow.Enum;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow
{
    public static class ApprovalEngineExtensions
    {
        public static bool IsApprovedEnabled(this IEnumerable<IPAFTransition> transitions, string username)
        {
            return getApproverTransition(transitions, username)
               .Any(t => t.ApproverDecision != DecisionType.Approved) || transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        public static bool IsRejectEnabled(this IEnumerable<IPAFTransition> transitions, string username)
        {
            return getApproverTransition(transitions, username)
               .All(t => t.ApproverDecision != DecisionType.Approved) || transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        public static bool IsTakenDecision(this IEnumerable<IPAFTransition> transitions, string username)
        {
            var currentT = getApproverTransition(transitions, username).OrderByDescending(o => o.Order).FirstOrDefault();
            return !(currentT == null || currentT.ApproverDecision == DecisionType.AwaitingDecision);
        }

        public static bool IsApproved(this IEnumerable<IPAFTransition> transitions) => transitions.Any() && transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).All(t => t.ApproverDecision == DecisionType.Approved);

        public static bool IsAnyApproved(this IEnumerable<IPAFTransition> transitions) => transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).Any(t => t.ApproverDecision == DecisionType.Approved);

        public static bool IsAnyDecisionPending(this IEnumerable<IPAFTransition> transitions) => transitions.Any() && transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).Any(t => t.ApproverDecision == DecisionType.AwaitingDecision);

        private static IEnumerable<IPAFTransition> getApproverTransition(IEnumerable<IPAFTransition> transitions, string username)
        {
            return transitions?.Where(t => t.ApproverID == username) ?? new List<IPAFTransition>();
        }
    }
}
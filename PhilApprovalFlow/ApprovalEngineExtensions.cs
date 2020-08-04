using PhilApprovalFlow.Enum;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow
{
    public static class ApprovalEngineExtensions
    {
        public static bool IsApprovedEnabled(this IEnumerable<ITransition> transitions, string username)
        {
            return getApproverTransition(transitions, username)
               .Any(t => t.ApproverDecision != DecisionType.Approved) || transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        public static bool IsRejectEnabled(this IEnumerable<ITransition> transitions, string username)
        {
            return getApproverTransition(transitions, username)
               .All(t => t.ApproverDecision != DecisionType.Approved) || transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        public static bool IsReapprovalNeeded(this IEnumerable<ITransition> transitions, string username)
        {
            var currentT = getApproverTransition(transitions, username).OrderByDescending(o => o.Order).FirstOrDefault();

            if (currentT == null || currentT.ApproverDecision == DecisionType.Approved)
                return false;

            var order = currentT.Order + 1;

            var tr = transitions.Where(t => t.Order == order).FirstOrDefault();

            return tr != null && (tr.ApproverDecision == DecisionType.Rejected);
        }

        public static bool IsTakenDecision(this IEnumerable<ITransition> transitions, string username)
        {
            var currentT = getApproverTransition(transitions, username).OrderByDescending(o => o.Order).FirstOrDefault();
            return !(currentT == null || currentT.ApproverDecision == DecisionType.AwaitingDecision);
        }

        public static bool IsApproved(this IEnumerable<ITransition> transitions) => transitions.Any() && transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).All(t => t.ApproverDecision == DecisionType.Approved);

        public static bool IsAnyApproved(this IEnumerable<ITransition> transitions) => transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).Any(t => t.ApproverDecision == DecisionType.Approved);

        public static bool IsAnyDecisionPending(this IEnumerable<ITransition> transitions) => transitions.Any() && transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).Any(t => t.ApproverDecision == DecisionType.AwaitingDecision);

        private static IEnumerable<ITransition> getApproverTransition(IEnumerable<ITransition> transitions, string username)
        {
            return transitions?.Where(t => t.ApproverID == username) ?? new List<ITransition>();
        }
    }
}
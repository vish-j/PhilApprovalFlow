using PhilApprovalFlow.Enum;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow
{
    public static class ApprovalEngineExtensions
    {
        /// <summary>
        /// Check if approver exists in workflow
        /// </summary>
        /// <param name="username">Approver's username</param>
        /// <param name="includeInvalidated">Include Invalidated Transitions</param>
        public static bool IsInTransitions(this IEnumerable<IPAFTransition> transitions, string username, bool includeInvalidated = false)
        {
            if (!transitions.Any())
            {
                return false;
            }

            var list = transitions;
            if (!includeInvalidated)
            {
                list = transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated);
            }

            return list.Any(t => t.ApproverID == username || (((t.ApproverGroup?.GroupID ?? 0) != 0) &&
                     t.ApproverGroup.Contains(username) &&
                     t.ApproverGroup.IsActive()));
        }

        /// <summary>
        /// Check if approver can approve
        /// </summary>
        /// <param name="username">Approver's username</param>
        public static bool IsApprovedEnabled(this IEnumerable<IPAFTransition> transitions, string username)
        {
            return GetApproverTransitions(transitions, username)
               .Any(t => t.ApproverDecision != DecisionType.Approved) || transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        /// <summary>
        /// Check if approver can reject
        /// </summary>
        /// <param name="username">Approver's username</param>
        public static bool IsRejectEnabled(this IEnumerable<IPAFTransition> transitions, string username)
        {
            return GetApproverTransitions(transitions, username)
               .All(t => t.ApproverDecision != DecisionType.Approved) || transitions.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        /// <summary>
        /// Check if approver has made a decision
        /// </summary>
        /// <param name="username">Approver's username</param>
        public static bool IsTakenDecision(this IEnumerable<IPAFTransition> transitions, string username)
        {
            var currentT = GetApproverTransitions(transitions, username).OrderByDescending(o => o.Order).FirstOrDefault();
            return !(currentT == null || currentT.ApproverDecision == DecisionType.AwaitingDecision);
        }

        /// <summary>Check if approver has made a decision</summary>
        public static bool IsCheckedIn(this IEnumerable<IPAFTransition> transitions, string username)
        {
            return transitions.Any() && transitions.Any(t => (t.ApproverID == username || (((t.ApproverGroup?.GroupID ?? 0) != 0) &&
                     t.ApproverID == null &&
                     t.ApproverGroup.Contains(username) &&
                     t.ApproverGroup.IsActive())) && t.IsCheckedIn);
        }

        /// <summary>Check if all approvers have approved</summary>
        public static bool IsApproved(this IEnumerable<IPAFTransition> transitions)
        {
            if (!transitions.Any())
            {
                return false;
            }
            
            var validTransitions = transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).ToList();
            return validTransitions.Any() && validTransitions.All(t => t.ApproverDecision == DecisionType.Approved);
        }

        /// <summary>Check if any approver has approved</summary>
        public static bool IsAnyApproved(this IEnumerable<IPAFTransition> transitions)
        {
            if (!transitions.Any())
            {
                return false;
            }

            var validTransitions = transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).ToList();
            return validTransitions.Any(t => t.ApproverDecision == DecisionType.Approved);
        }

        /// <summary>Check if any approver has yet to make a decision</summary>
        public static bool IsAnyDecisionPending(this IEnumerable<IPAFTransition> transitions)
        {
            if (!transitions.Any())
            {
                return false;
            }
            
            var validTransitions = transitions.Where(t => t.ApproverDecision != DecisionType.Invalidated).ToList();
            return validTransitions.Any(t => t.ApproverDecision == DecisionType.AwaitingDecision);
        }

        private static IEnumerable<IPAFTransition> GetApproverTransitions(IEnumerable<IPAFTransition> transitions, string username)
        {
            return transitions?.Where(t => t.ApproverID == username || (((t.ApproverGroup?.GroupID ?? 0) != 0) &&
                     t.ApproverID == null &&
                     t.ApproverGroup.Contains(username) &&
                     t.ApproverGroup.IsActive())) ?? new List<IPAFTransition>();
        }
    }
}
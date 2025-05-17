using PhilApprovalFlow.Enum;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow
{
    public static class ApprovalEngineExtensions
    {
        /// <summary>
        /// Check if approver exists in workflow
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <param name="username">Approver's username to find in the transitions</param>
        /// <param name="includeInvalidated">If true, includes invalidated transitions in the search; otherwise, excludes them</param>
        /// <returns>True if the approver exists in the workflow; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown if transitions is null</exception>
        public static bool IsInTransitions(this IEnumerable<IPAFTransition> transitions, string username, bool includeInvalidated = false)
        {
            if (transitions == null)
            {
                throw new ArgumentNullException(nameof(transitions), "Transitions collection cannot be null");
            }

            // Check if there are any transitions
            var transitionsList = transitions.ToList();
            if (transitionsList.Count == 0)
            {
                return false;
            }

            // Filter out invalidated transitions if needed
            var filteredTransitions = includeInvalidated 
                ? transitionsList 
                : transitionsList.Where(t => t.ApproverDecision != DecisionType.Invalidated).ToList();

            // Check if approver exists
            return filteredTransitions.Any(t => IsApproverInTransition(t, username));
        }

        /// <summary>
        /// Check if approver can approve
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <param name="username">Approver's username</param>
        /// <returns>True if the approver can approve; otherwise, false</returns>
        public static bool IsApprovedEnabled(this IEnumerable<IPAFTransition> transitions, string username)
        {
            var transitionsList = transitions.ToList();
            var approverTransitions = GetApproverTransitions(transitionsList, username).ToList();
            
            return approverTransitions.Any(t => t.ApproverDecision != DecisionType.Approved) || 
                   transitionsList.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        /// <summary>
        /// Check if approver can reject
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <param name="username">Approver's username</param>
        /// <returns>True if the approver can reject; otherwise, false</returns>
        public static bool IsRejectEnabled(this IEnumerable<IPAFTransition> transitions, string username)
        {
            var transitionsList = transitions.ToList();
            var approverTransitions = GetApproverTransitions(transitionsList, username).ToList();
            
            return approverTransitions.All(t => t.ApproverDecision != DecisionType.Approved) || 
                   transitionsList.Any(t => t.ApproverDecision == DecisionType.Rejected);
        }

        /// <summary>
        /// Check if approver has made a decision
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <param name="username">Approver's username</param>
        /// <returns>True if the approver has made a decision; otherwise, false</returns>
        public static bool IsTakenDecision(this IEnumerable<IPAFTransition> transitions, string username)
        {
            var approverTransitions = GetApproverTransitions(transitions, username).ToList();
            if (approverTransitions.Count == 0)
            {
                return false;
            }
            
            var currentTransition = approverTransitions.OrderByDescending(o => o.Order).First();
            return currentTransition.ApproverDecision != DecisionType.AwaitingDecision;
        }

        /// <summary>
        /// Check if approver has checked in
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <param name="username">Approver's username</param>
        /// <returns>True if the approver has checked in; otherwise, false</returns>
        public static bool IsCheckedIn(this IEnumerable<IPAFTransition> transitions, string username)
        {
            var transitionsList = transitions.ToList();
            return transitionsList.Count > 0 && 
                   transitionsList.Any(t => IsApproverInTransition(t, username) && t.IsCheckedIn);
        }

        /// <summary>
        /// Check if all approvers have approved
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <returns>True if all approvers have approved; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown if transitions is null</exception>
        public static bool IsApproved(this IEnumerable<IPAFTransition> transitions)
        {
            if (transitions == null)
            {
                throw new ArgumentNullException(nameof(transitions), "Transitions collection cannot be null");
            }
            
            // Use ToList only once for performance
            var validTransitions = transitions
                .Where(t => t.ApproverDecision != DecisionType.Invalidated)
                .ToList();
            
            return validTransitions.Count > 0 && 
                   validTransitions.All(t => t.ApproverDecision == DecisionType.Approved);
        }
        
        /// <summary>
        /// Check if any approver has approved
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <returns>True if any approver has approved; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown if transitions is null</exception>
        public static bool IsAnyApproved(this IEnumerable<IPAFTransition> transitions)
        {
            if (transitions == null)
            {
                throw new ArgumentNullException(nameof(transitions), "Transitions collection cannot be null");
            }
            
            // No need to convert to list for Any operation - directly use Where+Any 
            return transitions
                .Where(t => t.ApproverDecision != DecisionType.Invalidated)
                .Any(t => t.ApproverDecision == DecisionType.Approved);
        }
        
        /// <summary>
        /// Check if any approver has yet to make a decision
        /// </summary>
        /// <param name="transitions">The collection of transitions to check</param>
        /// <returns>True if any approver has a pending decision; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">Thrown if transitions is null</exception>
        public static bool IsAnyDecisionPending(this IEnumerable<IPAFTransition> transitions)
        {
            if (transitions == null)
            {
                throw new ArgumentNullException(nameof(transitions), "Transitions collection cannot be null");
            }
            
            // Only use ToList if we need to iterate multiple times
            return transitions
                .Where(t => t.ApproverDecision != DecisionType.Invalidated)
                .Any(t => t.ApproverDecision == DecisionType.AwaitingDecision);
        }

        /// <summary>
        /// Helper method to determine if an approver is in a transition
        /// </summary>
        /// <param name="transition">The transition to check</param>
        /// <param name="username">The approver's username</param>
        /// <returns>True if the approver is in the transition; otherwise, false</returns>
        private static bool IsApproverInTransition(IPAFTransition transition, string username)
        {
            if (transition.ApproverID == username)
            {
                return true;
            }
            
            bool hasActiveGroup = (transition.ApproverGroup?.GroupID ?? 0) != 0;
            
            return hasActiveGroup &&
                   transition.ApproverID == null &&
                   transition.ApproverGroup.Contains(username) &&
                   transition.ApproverGroup.IsActive();
        }

        /// <summary>
        /// Get all transitions for a specific approver
        /// </summary>
        /// <param name="transitions">The collection of transitions to filter</param>
        /// <param name="username">The approver's username</param>
        /// <returns>A filtered collection of transitions for the approver</returns>
        private static IEnumerable<IPAFTransition> GetApproverTransitions(IEnumerable<IPAFTransition> transitions, string username)
        {
            if (transitions == null)
            {
                return new List<IPAFTransition>();
            }
            
            return transitions.Where(t => IsApproverInTransition(t, username)).ToList();
        }
    }
}
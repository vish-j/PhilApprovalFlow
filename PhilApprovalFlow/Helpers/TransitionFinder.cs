using PhilApprovalFlow.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow.Helpers
{
    /// <summary>
    /// Provides helper methods for finding and matching transitions in approval workflows.
    /// </summary>
    /// <typeparam name="T">The type of the approval transition.</typeparam>
    internal class TransitionFinder<T> where T : IPAFTransition
    {
        private readonly IApprovalFlow<T> approvalFlowEntity;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionFinder{T}"/> class.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        public TransitionFinder(IApprovalFlow<T> entity)
        {
            approvalFlowEntity = entity;
        }

        /// <summary>
        /// Retrieves a transition for the specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <returns>
        /// The first transition found for the specified approver, or <c>null</c> if no matching
        /// transition is found.
        /// </returns>
        /// <remarks>
        /// A transition matches an approver if the approver is directly assigned to the transition
        /// or if the approver is a member of an active approver group assigned to the transition.
        /// </remarks>
        public IPAFTransition GetTransition(string approver)
        {
            return approvalFlowEntity.Transitions
                .FirstOrDefault(t => IsApproverMatch(t, approver));
        }

        /// <summary>
        /// Finds a transition associated with the specified approver group.
        /// </summary>
        /// <param name="group">The approver group to search for.</param>
        /// <returns>
        /// The first transition found with the matching group ID, or <c>null</c> if no matching
        /// transition is found.
        /// </returns>
        public IPAFTransition FindTransitionByGroup(IPAFApproverGroup group)
        {
            return approvalFlowEntity.Transitions
                .FirstOrDefault(t => t.ApproverGroup?.GroupID == group.GroupID);
        }

        /// <summary>
        /// Determines if a transition is associated with a specific approver.
        /// </summary>
        /// <param name="transition">The transition to check.</param>
        /// <param name="approver">The approver's identifier.</param>
        /// <returns>
        /// <c>true</c> if the transition is directly associated with the approver or if the approver
        /// is a member of an active approver group assigned to the transition; otherwise, <c>false</c>.
        /// </returns>
        public bool IsApproverMatch(IPAFTransition transition, string approver)
        {
            // Direct match to approver ID
            if (transition.ApproverID == approver)
            {
                return true;
            }

            // Check if approver is in an active group
            if (HasActiveApproverGroup(transition) &&
                transition.ApproverGroup.Contains(approver))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if a transition has an active approver group.
        /// </summary>
        /// <param name="transition">The transition to check.</param>
        /// <returns>
        /// <c>true</c> if the transition has a non-null approver group with a non-zero group ID 
        /// and the group is active; otherwise, <c>false</c>.
        /// </returns>
        public bool HasActiveApproverGroup(IPAFTransition transition)
        {
            return transition.ApproverGroup != null &&
                   (transition.ApproverGroup.GroupID != 0) &&
                   transition.ApproverGroup.IsActive();
        }

        /// <summary>
        /// Gets all transitions for the approval flow entity.
        /// </summary>
        /// <returns>The collection of transitions.</returns>
        public ICollection<T> GetAllTransitions()
        {
            return approvalFlowEntity.Transitions;
        }
    }
}
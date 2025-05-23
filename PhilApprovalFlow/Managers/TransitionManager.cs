using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Helpers;
using PhilApprovalFlow.Interfaces;
using PhilApprovalFlow.Models;
using System;
using System.Linq;

namespace PhilApprovalFlow.Managers
{
    /// <summary>
    /// Manages transition operations for approval workflows.
    /// </summary>
    /// <typeparam name="T">The type of the approval transition.</typeparam>
    internal class TransitionManager<T> where T : IPAFTransition, new()
    {
        private readonly IApprovalFlow<T> approvalFlowEntity;
        private readonly TransitionFinder<T> transitionFinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionManager{T}"/> class.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public TransitionManager(IApprovalFlow<T> entity)
        {
            approvalFlowEntity = entity ?? throw new ArgumentNullException(nameof(entity));
            transitionFinder = new TransitionFinder<T>(entity);
        }

        /// <summary>
        /// Creates a new transition or updates an existing one for a specific approver.
        /// </summary>
        /// <param name="requester">The requester's identifier.</param>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="role">The approver's role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <exception cref="ArgumentNullException">Thrown if requester or approver is null.</exception>
        public void CreateTransition(string requester, string approver, string role, string comments)
        {
            if (requester == null)
            {
                throw new ArgumentNullException(nameof(requester), "Requester cannot be null or empty");
            }

            if (approver == null)
            {
                throw new ArgumentNullException(nameof(approver), "Approver cannot be null or empty");
            }

            // Find existing transition by approver
            var existingTransition = approvalFlowEntity.Transitions.FirstOrDefault(t => t.ApproverID == approver);

            if (existingTransition == null)
            {
                // No existing transition, create a new one
                int order = !approvalFlowEntity.Transitions.Any() ? 1 : approvalFlowEntity.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initialize(order, requester, approver, role, comments);
                approvalFlowEntity.Transitions.Add(newTransition);
            }
            else if (IsTransitionAwaitingOrInvalidated(existingTransition))
            {
                // Update existing transition
                existingTransition.ApproverRole = role;
                if (comments != null)
                {
                    existingTransition.RequesterComments = comments;
                }

                SetDecision(existingTransition, DecisionType.AwaitingDecision, comments);
            }
        }

        /// <summary>
        /// Creates a new transition or updates an existing one for an approver group.
        /// </summary>
        /// <param name="requester">The requester's identifier.</param>
        /// <param name="group">The approver group.</param>
        /// <param name="role">The approvers' role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <exception cref="ArgumentNullException">Thrown if requester or group is null.</exception>
        /// <exception cref="ArgumentException">Thrown if group is not of the expected type.</exception>
        public void CreateTransition(string requester, IPAFApproverGroup group, string role, string comments)
        {
            if (requester == null)
            {
                throw new ArgumentNullException(nameof(requester), "Requester cannot be null or empty");
            }

            if (group == null)
            {
                throw new ArgumentNullException(nameof(group), "Approver Group cannot be null");
            }

            // Find existing transition by group
            var existingTransition = transitionFinder.FindTransitionByGroup(group);

            if (existingTransition == null)
            {
                // No existing transition, create a new one
                int order = !approvalFlowEntity.Transitions.Any() ? 1 : approvalFlowEntity.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initialize(order, requester, group, role, comments);
                approvalFlowEntity.Transitions.Add(newTransition);
            }
            else if (IsTransitionAwaitingOrInvalidated(existingTransition))
            {
                // Update existing transition
                if (existingTransition.ApproverGroup == null)
                {
                    existingTransition.ApproverGroup = group as PAFApproverGroup
                                    ?? throw new ArgumentException($"The group must be of type {nameof(PAFApproverGroup)}", nameof(group));
                }
                existingTransition.ApproverRole = role;
                if (comments != null)
                {
                    existingTransition.RequesterComments = comments;
                }

                SetDecision(existingTransition, DecisionType.AwaitingDecision, comments);
            }
        }

        /// <summary>
        /// Resets all transitions to a default "Awaiting Decision" state with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to associate with the reset transitions.</param>
        public void ResetAllTransitions(string comments = null)
        {
            foreach (var item in approvalFlowEntity.Transitions)
            {
                SetDecision(item, DecisionType.AwaitingDecision, comments);
            }
        }

        /// <summary>
        /// Marks a transition as checked in.
        /// </summary>
        /// <param name="username">The user checking in.</param>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the user.</exception>
        public void CheckIn(string username)
        {
            IPAFTransition transition = transitionFinder.GetTransition(username);
            if (transition == null)
            {
                throw new InvalidOperationException("No transition found for the current user");
            }

            transition.ApproverCheckInDate = DateTime.Now;
        }

        /// <summary>
        /// Sets the decision for a user's transition.
        /// </summary>
        /// <param name="username">The user making the decision.</param>
        /// <param name="decision">The decision type.</param>
        /// <param name="comments">Optional comments.</param>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the user.</exception>
        public void SetUserDecision(string username, DecisionType decision, string comments = null)
        {
            IPAFTransition transition = transitionFinder.GetTransition(username);
            if (transition == null)
            {
                throw new InvalidOperationException("No transition found for the current user");
            }

            SetDecision(transition, decision, comments);
        }

        /// <summary>
        /// Invalidates a transition for a specific approver.
        /// </summary>
        /// <param name="approver">The approver whose transition should be invalidated.</param>
        /// <param name="comments">Optional comments.</param>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the approver.</exception>
        public void InvalidateTransition(string approver, string comments = null)
        {
            IPAFTransition transition = transitionFinder.GetTransition(approver);
            if (transition == null)
            {
                throw new InvalidOperationException("No transition found for the current user");
            }

            SetDecision(transition, DecisionType.Invalidated, comments);
        }

        /// <summary>
        /// Retrieves a transition for the specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <returns>The transition if found; otherwise, null.</returns>
        public IPAFTransition GetTransition(string approver)
        {
            return transitionFinder.GetTransition(approver);
        }

        /// <summary>
        /// Finds a transition associated with the specified approver group.
        /// </summary>
        /// <param name="group">The approver group to search for.</param>
        /// <returns>The transition if found; otherwise, null.</returns>
        public IPAFTransition FindTransitionByGroup(IPAFApproverGroup group)
        {
            return transitionFinder.FindTransitionByGroup(group);
        }

        /// <summary>
        /// Sets the decision status for a transition.
        /// </summary>
        /// <param name="transition">The transition to update.</param>
        /// <param name="decision">The decision to set.</param>
        /// <param name="comments">Optional comments associated with the decision.</param>
        private void SetDecision(IPAFTransition transition, DecisionType decision, string comments)
        {
            transition.ApproverDecision = decision;

            // Only set check-in date if transition is not already checked in
            if (transition.ApproverCheckInDate == null && decision != DecisionType.AwaitingDecision)
            {
                transition.ApproverCheckInDate = DateTime.Now;
            }

            // Set acknowledgement date based on decision
            transition.AcknowledgementDate = decision == DecisionType.AwaitingDecision ? null : (DateTime?)DateTime.Now;
            transition.ApproverComments = comments;
        }

        /// <summary>
        /// Determines if a transition is in a state where it can be updated.
        /// </summary>
        /// <param name="transition">The transition to check.</param>
        /// <returns>True if the transition can be updated; otherwise, false.</returns>
        private bool IsTransitionAwaitingOrInvalidated(IPAFTransition transition)
        {
            return transition.ApproverDecision == DecisionType.Invalidated ||
                   transition.ApproverDecision == DecisionType.AwaitingDecision;
        }
    }
}
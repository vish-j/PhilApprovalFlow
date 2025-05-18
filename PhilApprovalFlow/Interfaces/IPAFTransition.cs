using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Interfaces;
using PhilApprovalFlow.Models;
using System;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Defines a transition within an approval workflow.
    /// A transition represents a single approval step with its associated metadata.
    /// </summary>
    public interface IPAFTransition
    {
        /// <summary>
        /// Gets or sets the unique identifier for this transition.
        /// </summary>
        /// <remarks>
        /// This value is generated automatically when a transition is created.
        /// </remarks>
        Guid TransitionID { get; set; }

        /// <summary>
        /// Gets or sets the sequential order of this transition within the workflow.
        /// </summary>
        /// <remarks>
        /// Higher values indicate later transitions. This is used to determine the chronological
        /// order of transitions when multiple transitions exist for the same approver.
        /// </remarks>
        int Order { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the approver assigned to this transition.
        /// </summary>
        /// <remarks>
        /// This can be null if the transition is assigned to an approver group rather than an individual.
        /// </remarks>
        string ApproverID { get; set; }

        /// <summary>
        /// Gets or sets the current decision status of this transition.
        /// </summary>
        /// <remarks>
        /// The initial value is typically <see cref="DecisionType.AwaitingDecision"/>.
        /// </remarks>
        DecisionType ApproverDecision { get; set; }

        /// <summary>
        /// Gets or sets the role of the approver in this transition.
        /// </summary>
        /// <remarks>
        /// Examples include "Reviewer", "Manager", "Director", etc.
        /// This value is used for display purposes and can be used for filtering transitions.
        /// </remarks>
        string ApproverRole { get; set; }

        /// <summary>
        /// Gets or sets the date and time when the approver first viewed or checked in to this transition.
        /// </summary>
        /// <remarks>
        /// This is null until the approver checks in or takes an action on the transition.
        /// </remarks>
        DateTime? ApproverCheckInDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time when a decision (approval, rejection, or invalidation) was made.
        /// </summary>
        /// <remarks>
        /// This is null while the transition is awaiting a decision.
        /// </remarks>
        DateTime? AcknowledgementDate { get; set; }

        /// <summary>
        /// Gets or sets the comments provided by the approver when making a decision.
        /// </summary>
        string ApproverComments { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the user who requested this approval transition.
        /// </summary>
        string RequesterID { get; set; }

        /// <summary>
        /// Gets or sets the date and time when this transition was requested.
        /// </summary>
        DateTime RequestedDate { get; set; }

        /// <summary>
        /// Gets or sets the comments provided by the requester when creating this transition.
        /// </summary>
        string RequesterComments { get; set; }

        /// <summary>
        /// Gets a value indicating whether the approver has checked in to this transition.
        /// </summary>
        /// <remarks>
        /// This is true if <see cref="ApproverCheckInDate"/> has a value; otherwise, false.
        /// </remarks>
        bool IsCheckedIn { get; }

        /// <summary>
        /// Gets or sets the group of approvers assigned to this transition.
        /// </summary>
        /// <remarks>
        /// This is null if the transition is assigned to an individual approver rather than a group.
        /// When this property is set, <see cref="ApproverID"/> is typically null.
        /// </remarks>
        PAFApproverGroup ApproverGroup { get; set; }

        /// <summary>
        /// Initializes this transition for an individual approver.
        /// </summary>
        /// <param name="order">The sequential order of this transition within the workflow.</param>
        /// <param name="requester">The identifier of the user requesting approval.</param>
        /// <param name="approver">The identifier of the user who will approve or reject the request.</param>
        /// <param name="role">The role of the approver in this transition.</param>
        /// <param name="comments">Optional comments from the requester about this transition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="requester"/> or <paramref name="approver"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="requester"/> or <paramref name="approver"/> is an empty string or contains only whitespace.
        /// </exception>
        void Initialize(int order, string requester, string approver, string role, string comments);

        /// <summary>
        /// Initializes this transition for a group of approvers.
        /// </summary>
        /// <param name="order">The sequential order of this transition within the workflow.</param>
        /// <param name="requester">The identifier of the user requesting approval.</param>
        /// <param name="group">The group of users who can approve or reject the request.</param>
        /// <param name="role">The role of the approvers in this transition.</param>
        /// <param name="comments">Optional comments from the requester about this transition.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="requester"/> or <paramref name="group"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="requester"/> is an empty string or contains only whitespace,
        /// or when <paramref name="group"/> is not of the expected type.
        /// </exception>
        void Initialize(int order, string requester, IPAFApproverGroup group, string role, string comments);
    }
}
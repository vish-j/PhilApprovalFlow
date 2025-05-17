using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Interfaces;
using System;

namespace PhilApprovalFlow.Models
{
    /// <summary>
    /// Represents a transition in an approval flow process.
    /// </summary>
    public abstract class PAFTransition : IPAFTransition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PAFTransition"/> class.
        /// </summary>
        public PAFTransition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PAFTransition"/> class by copying the properties of another transition.
        /// </summary>
        /// <param name="t">The transition to copy.</param>
        /// <exception cref="ArgumentNullException">Thrown if the provided transition is null.</exception>
        public PAFTransition(PAFTransition t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t), "The transition to copy cannot be null.");
            }

            TransitionID = t.TransitionID;
            Order = t.Order;
            RequesterID = t.RequesterID;
            RequestedDate = t.RequestedDate;
            ApproverID = t.ApproverID;
            ApproverGroup = t.ApproverGroup;
            ApproverRole = t.ApproverRole;
            ApproverDecision = t.ApproverDecision;
            RequesterComments = t.RequesterComments;
            ApproverComments = t.ApproverComments;
            ApproverCheckInDate = t.ApproverCheckInDate;
            AcknowledgementDate = t.AcknowledgementDate;
        }

        /// <summary>
        /// Gets or sets the unique identifier for the transition.
        /// </summary>
        public Guid TransitionID { get; set; }

        /// <summary>
        /// Gets or sets the order of the transition in the approval flow.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the approver's unique identifier.
        /// </summary>
        public string ApproverID { get; set; }

        /// <summary>
        /// Gets or sets the role of the approver in the transition.
        /// </summary>
        public string ApproverRole { get; set; }

        /// <summary>
        /// Gets or sets the group of approvers for the transition.
        /// </summary>
        public PAFApproverGroup ApproverGroup { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the approver checked in.
        /// </summary>
        public DateTime? ApproverCheckInDate { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the transition was acknowledged.
        /// </summary>
        public DateTime? AcknowledgementDate { get; set; }

        /// <summary>
        /// Gets or sets the decision made by the approver.
        /// </summary>
        public DecisionType ApproverDecision { get; set; }

        /// <summary>
        /// Gets or sets the comments provided by the approver.
        /// </summary>
        public string ApproverComments { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the requester.
        /// </summary>
        public string RequesterID { get; set; }

        /// <summary>
        /// Gets or sets the timestamp of when the transition was requested.
        /// </summary>
        public DateTime RequestedDate { get; set; }

        /// <summary>
        /// Gets or sets the comments provided by the requester.
        /// </summary>
        public string RequesterComments { get; set; }

        /// <summary>
        /// Gets a value indicating whether the approver has checked in.
        /// </summary>
        public bool IsCheckedIn => ApproverCheckInDate != null;

        /// <summary>
        /// Initializes a transition with specified details.
        /// </summary>
        /// <param name="order">The order of the transition in the approval flow.</param>
        /// <param name="requester">The unique identifier of the requester.</param>
        /// <param name="approver">The unique identifier of the approver.</param>
        /// <param name="role">The role of the approver.</param>
        /// <param name="comments">The comments provided by the requester.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="requester"/> or <paramref name="approver"/> is null or whitespace.</exception>
        public void Initialize(int order, string requester, string approver, string role, string comments)
        {
            if (string.IsNullOrWhiteSpace(requester))
            {
                throw new ArgumentNullException(nameof(requester), "Requester cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(approver))
            {
                throw new ArgumentNullException(nameof(approver), "Approver cannot be null or empty.");
            }

            Order = order;
            RequesterID = requester;
            RequestedDate = DateTime.Now;
            ApproverID = approver;
            ApproverRole = role;
            ApproverDecision = DecisionType.AwaitingDecision;
            RequesterComments = comments;
        }

        /// <summary>
        /// Initializes a transition with specified details for an approver group.
        /// </summary>
        /// <param name="order">The order of the transition in the approval flow.</param>
        /// <param name="requester">The unique identifier of the requester.</param>
        /// <param name="group">The group of approvers.</param>
        /// <param name="role">The role of the approvers.</param>
        /// <param name="comments">The comments provided by the requester.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="requester"/> or <paramref name="group"/> is null or whitespace.</exception>
        public void Initalize(int order, string requester, IPAFApproverGroup group, string role, string comments)
        {
            if (string.IsNullOrWhiteSpace(requester))
            {
                throw new ArgumentNullException(nameof(requester), "Requester cannot be null or empty.");
            }

            if (group == null)
            {
                throw new ArgumentNullException(nameof(group), "Approver group cannot be null.");
            }

            Order = order;
            RequesterID = requester;
            RequestedDate = DateTime.Now;
            ApproverGroup = group as PAFApproverGroup;
            ApproverRole = role;
            ApproverDecision = DecisionType.AwaitingDecision;
            RequesterComments = comments;
        }
    }
}

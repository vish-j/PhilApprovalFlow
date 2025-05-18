using PhilApprovalFlow.Enum;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Defines a notification generated from an approval flow transition.
    /// Notifications can be used to send emails, create tasks, or trigger other notification systems.
    /// </summary>
    public interface IPAFNotification
    {
        /// <summary>
        /// Gets or sets the identifier of the sender of the notification.
        /// </summary>
        /// <remarks>
        /// This will be the requester for AwaitingDecision or Invalidated transitions,
        /// or the approver for Approved or Rejected transitions.
        /// Can be null if the sender is the same as the recipient.
        /// </remarks>
        string From { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the recipient of the notification.
        /// </summary>
        /// <remarks>
        /// This will be the approver for AwaitingDecision or Invalidated transitions,
        /// or the requester for Approved or Rejected transitions.
        /// </remarks>
        string To { get; set; }

        /// <summary>
        /// Gets or sets the comments included in the notification.
        /// </summary>
        /// <remarks>
        /// This will be the requester's comments for AwaitingDecision or Invalidated transitions,
        /// or the approver's comments for Approved or Rejected transitions.
        /// </remarks>
        string Comments { get; set; }

        /// <summary>
        /// Gets or sets the decision type associated with the notification.
        /// </summary>
        /// <remarks>
        /// Indicates the current state of the transition that generated this notification:
        /// - AwaitingDecision: Notifying an approver that their decision is needed
        /// - Approved: Notifying the requester that the transition was approved
        /// - Rejected: Notifying the requester that the transition was rejected
        /// - Invalidated: Notifying the approver that they are no longer required to make a decision
        /// </remarks>
        DecisionType DecisionType { get; set; }

        /// <summary>
        /// Gets or sets the group ID if this notification is associated with an approver group.
        /// </summary>
        /// <remarks>
        /// This will be null for notifications associated with individual approvers,
        /// or the group ID for notifications associated with an approver group.
        /// </remarks>
        long? GroupID { get; set; }

        /// <summary>
        /// Gets or sets an array of user identifiers to copy on the notification.
        /// </summary>
        /// <remarks>
        /// These users will receive a copy of the notification in addition to the primary recipient.
        /// Can be null if no additional users need to be copied.
        /// </remarks>
        string[] UsersToCC { get; set; }

        /// <summary>
        /// Gets or sets an array of email addresses to copy on the notification.
        /// </summary>
        /// <remarks>
        /// These email addresses will receive a copy of the notification in addition to the primary recipient.
        /// Can be null if no additional email addresses need to be copied.
        /// </remarks>
        string[] MailsToCC { get; set; }
    }
}
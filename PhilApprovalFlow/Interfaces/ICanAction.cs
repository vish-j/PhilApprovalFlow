using PhilApprovalFlow.Interfaces;
using System;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Defines the operations that can be performed on an approval flow after setting the user context.
    /// This interface provides the fluent API for interacting with the approval workflow.
    /// </summary>
    public interface ICanAction
    {
        /// <summary>
        /// Adds or updates metadata associated with the approval flow.
        /// </summary>
        /// <param name="key">The key of the metadata entry to add or update.</param>
        /// <param name="value">The value to associate with the specified key.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="key"/> is null, empty, or contains only whitespace.
        /// The exception message will be: "Key cannot be null or whitespace".
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="value"/> is null.
        /// The exception message will be: "Value cannot be null".
        /// </exception>
        /// <remarks>
        /// If the key already exists, its value will be updated. If the key doesn't exist, a new entry will be added.
        /// Metadata can be used to store additional information about the workflow that isn't captured in the transitions.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetMetadata("Priority", "High");
        /// workflow.SetMetadata("DueDate", "2023-12-31");
        /// </code>
        /// </example>
        ICanAction SetMetadata(string key, string value);

        /// <summary>
        /// Retrieves a metadata value by its key.
        /// </summary>
        /// <param name="key">The key of the metadata entry to retrieve.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified <paramref name="key"/> does not exist in the metadata collection.
        /// </exception>
        /// <example>
        /// <code>
        /// string priority = workflow.GetMetadata("Priority"); // Returns "High"
        /// </code>
        /// </example>
        string GetMetadata(string key);

        /// <summary>
        /// Creates a transition to request approval from a specified approver.
        /// </summary>
        /// <param name="approver">The identifier of the user who will approve or reject the request.</param>
        /// <param name="role">The role of the approver in this transition (e.g., "Reviewer", "Manager").</param>
        /// <param name="comments">Optional comments to include with the request explaining what needs approval.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="approver"/> is null.
        /// The exception message will be: "Approver cannot be null or empty".
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="approver"/> is an empty string or contains only whitespace.
        /// </exception>
        /// <remarks>
        /// If a transition already exists for the specified approver and it is in an AwaitingDecision or Invalidated state,
        /// that transition will be updated instead of creating a new one.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetUserName("john.doe@example.com")
        ///         .RequestApproval("jane.smith@example.com", "Manager", "Please review this invoice");
        /// </code>
        /// </example>
        ICanAction RequestApproval(string approver, string role, string comments = null);

        /// <summary>
        /// Creates a transition to request approval from a specified group of approvers.
        /// </summary>
        /// <param name="group">The group of users who can approve or reject the request.</param>
        /// <param name="role">The role of the approvers in this transition (e.g., "Finance Team", "Executive Committee").</param>
        /// <param name="comments">Optional comments to include with the request explaining what needs approval.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="group"/> is null.
        /// The exception message will be: "Approver group cannot be null".
        /// </exception>
        /// <remarks>
        /// If a transition already exists for the specified group and it is in an AwaitingDecision or Invalidated state,
        /// that transition will be updated instead of creating a new one.
        /// 
        /// When an approval is requested from a group, any member of the group can approve or reject the transition.
        /// The group must be active for approvers to take action.
        /// </remarks>
        /// <example>
        /// <code>
        /// var financeTeam = new PAFApproverGroup { GroupID = 1 };
        /// financeTeam.SetApprovers(new[] { "john.doe@example.com", "jane.smith@example.com" });
        /// financeTeam.SetActiveStatus(true);
        /// 
        /// workflow.SetUserName("manager@example.com")
        ///         .RequestApproval(financeTeam, "Finance Reviewers", "Please verify budget allocation");
        /// </code>
        /// </example>
        ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments = null);

        /// <summary>
        /// Marks the current user's transition as "Checked In".
        /// </summary>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transition is found for the current user.
        /// The exception message will be: "No transition found for the current user".
        /// </exception>
        /// <remarks>
        /// Checking in indicates that the approver has viewed the request but has not yet made a decision.
        /// This updates the <see cref="IPAFTransition.ApproverCheckInDate"/> property to the current date and time.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetUserName("jane.smith@example.com").CheckIn();
        /// </code>
        /// </example>
        ICanAction CheckIn();

        /// <summary>
        /// Approves the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the approval explaining the decision.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transition is found for the current user.
        /// The exception message will be: "No transition found for the current user".
        /// </exception>
        /// <remarks>
        /// This updates the transition's decision to <see cref="Enum.DecisionType.Approved"/> and
        /// sets the <see cref="IPAFTransition.AcknowledgementDate"/> to the current date and time.
        /// If the transition has not been checked in, it will be automatically checked in.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetUserName("jane.smith@example.com").Approve("Looks good, approved!");
        /// </code>
        /// </example>
        ICanAction Approve(string comments = null);

        /// <summary>
        /// Rejects the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the rejection explaining the decision.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transition is found for the current user.
        /// The exception message will be: "No transition found for the current user".
        /// </exception>
        /// <remarks>
        /// This updates the transition's decision to <see cref="Enum.DecisionType.Rejected"/> and
        /// sets the <see cref="IPAFTransition.AcknowledgementDate"/> to the current date and time.
        /// If the transition has not been checked in, it will be automatically checked in.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetUserName("jane.smith@example.com").Reject("Budget exceeded, please revise.");
        /// </code>
        /// </example>
        ICanAction Reject(string comments = null);

        /// <summary>
        /// Invalidates a transition for a specified approver.
        /// </summary>
        /// <param name="approver">The identifier of the approver whose transition should be invalidated.</param>
        /// <param name="comments">Optional comments to include with the invalidation explaining the reason.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transition is found for the specified approver.
        /// The exception message will be: "No transition found for the current user".
        /// </exception>
        /// <remarks>
        /// Invalidating a transition removes it from consideration when determining if a workflow is complete.
        /// This is useful when an approver is no longer relevant or available to make a decision.
        /// 
        /// This updates the transition's decision to <see cref="Enum.DecisionType.Invalidated"/> and
        /// sets the <see cref="IPAFTransition.AcknowledgementDate"/> to the current date and time.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetUserName("admin@example.com").Invalidate("jane.smith@example.com", "Jane has left the company.");
        /// </code>
        /// </example>
        ICanAction Invalidate(string approver, string comments = null);

        /// <summary>
        /// Loads a notification for a specified approver.
        /// </summary>
        /// <param name="approver">The identifier of the approver for whom to generate a notification.</param>
        /// <param name="usersToCC">Optional list of user identifiers to copy on the notification.</param>
        /// <param name="mailsToCC">Optional list of email addresses to copy on the notification.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transition is found for the specified approver.
        /// The exception message will be: "No transition found for approver '[approver]'".
        /// </exception>
        /// <remarks>
        /// This creates a notification in the internal cache, which can be retrieved using <see cref="GetPAFNotifications"/>.
        /// The notification includes details about the transition, such as the requester, approver, decision type, and comments.
        /// 
        /// The sender and recipient of the notification depend on the transition's state:
        /// - For AwaitingDecision or Invalidated transitions, the notification is sent from the requester to the approver.
        /// - For Approved or Rejected transitions, the notification is sent from the approver to the requester.
        /// </remarks>
        /// <example>
        /// <code>
        /// workflow.SetUserName("admin@example.com")
        ///         .LoadNotification("jane.smith@example.com", new[] { "team@example.com" }, new[] { "manager@example.com" });
        /// </code>
        /// </example>
        ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null);

        /// <summary>
        /// Loads notifications for a specified approver group.
        /// </summary>
        /// <param name="group">The approver group for which to generate notifications.</param>
        /// <param name="usersToCC">Optional list of user identifiers to copy on the notifications.</param>
        /// <param name="mailsToCC">Optional list of email addresses to copy on the notifications.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="group"/> is null.
        /// The exception message will be: "Approver group cannot be null".
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no transition is found for the specified group.
        /// The exception message will be: "No transition found for group with ID [group.GroupID]".
        /// </exception>
        /// <remarks>
        /// This creates notifications in the internal cache for each member of the approver group.
        /// The notifications can be retrieved using <see cref="GetPAFNotifications"/>.
        /// 
        /// The sender and recipients of the notifications depend on the transition's state:
        /// - For AwaitingDecision or Invalidated transitions, notifications are sent from the requester to each approver in the group.
        /// - For Approved or Rejected transitions, a notification is sent from the deciding approver to the requester.
        /// </remarks>
        /// <example>
        /// <code>
        /// var financeTeam = new PAFApproverGroup { GroupID = 1 };
        /// financeTeam.SetApprovers(new[] { "john.doe@example.com", "jane.smith@example.com" });
        /// 
        /// workflow.SetUserName("admin@example.com")
        ///         .LoadNotification(financeTeam, new[] { "team@example.com" }, new[] { "manager@example.com" });
        /// </code>
        /// </example>
        ICanAction LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null);

        /// <summary>
        /// Sets metadata for the approval flow entity by adding key details such as ID, short description, and long description.
        /// </summary>
        /// <remarks>
        /// This method extracts metadata from the entity using its <see cref="IApprovalFlow{T}.GetID"/>,
        /// <see cref="IApprovalFlow{T}.GetShortDescription"/>, and <see cref="IApprovalFlow{T}.GetLongDescription"/> methods
        /// and adds it to the internal metadata dictionary with the keys "id", "shortDescription", and "longDescription" respectively.
        /// 
        /// If any of these keys already exist in the metadata, they will be overwritten.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the entity methods return null or invalid values.
        /// </exception>
        /// <example>
        /// <code>
        /// workflow.SetUserName("admin@example.com").SetEntityMetaData();
        /// 
        /// // Access the entity metadata
        /// string entityId = workflow.GetMetadata("id");
        /// string shortDesc = workflow.GetMetadata("shortDescription");
        /// string longDesc = workflow.GetMetadata("longDescription");
        /// </code>
        /// </example>
        void SetEntityMetaData();

        /// <summary>
        /// Gets all pending notifications.
        /// </summary>
        /// <returns>
        /// A collection of notifications, which may be empty if no notifications have been loaded.
        /// Each notification contains details about a transition, such as the sender, recipient, decision type, and comments.
        /// </returns>
        /// <remarks>
        /// Notifications must be explicitly loaded using <see cref="LoadNotification(string, string[], string[])"/> or
        /// <see cref="LoadNotification(IPAFApproverGroup, string[], string[])"/> before they can be retrieved.
        /// 
        /// After retrieving the notifications, you can clear them using <see cref="ClearNotifications"/> to prevent
        /// duplicate notifications from being sent.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Load notifications for multiple approvers
        /// workflow.SetUserName("admin@example.com")
        ///         .LoadNotification("jane.smith@example.com")
        ///         .LoadNotification("john.doe@example.com");
        /// 
        /// // Get all loaded notifications
        /// var notifications = workflow.GetPAFNotifications();
        /// 
        /// // Process notifications (e.g., send emails)
        /// foreach (var notification in notifications)
        /// {
        ///     // Send email using notification details
        ///     SendEmail(notification.From, notification.To, notification.Comments, notification.DecisionType);
        /// }
        /// 
        /// // Clear notifications to prevent duplicates
        /// workflow.ClearNotifications();
        /// </code>
        /// </example>
        IEnumerable<IPAFNotification> GetPAFNotifications();

        /// <summary>
        /// Clears all notifications from the internal cache.
        /// </summary>
        /// <remarks>
        /// This method removes all notifications from the internal notification cache.
        /// It should be called after processing notifications to prevent duplicate notifications from being sent.
        /// 
        /// If there are no notifications in the cache, this method has no effect.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Load notifications
        /// workflow.SetUserName("admin@example.com")
        ///         .LoadNotification("jane.smith@example.com");
        /// 
        /// // Process notifications
        /// var notifications = workflow.GetPAFNotifications();
        /// // Send emails, etc.
        /// 
        /// // Clear the cache to prevent duplicates
        /// workflow.ClearNotifications();
        /// 
        /// // Verify the cache is empty
        /// var emptyList = workflow.GetPAFNotifications(); // Returns an empty collection
        /// </code>
        /// </example>
        void ClearNotifications();
    }
}
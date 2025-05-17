using PhilApprovalFlow.Interfaces;
using System;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Defines the operations that can be performed on an approval flow after setting the user context.
    /// </summary>
    public interface ICanAction
    {
        /// <summary>
        /// Adds or updates metadata associated with the approval flow.
        /// </summary>
        /// <param name="key">The key of the metadata entry.</param>
        /// <param name="value">The value of the metadata entry.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">Thrown if the key is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        ICanAction SetMetadata(string key, string value);

        /// <summary>
        /// Retrieves a metadata value by its key.
        /// </summary>
        /// <param name="key">The key of the metadata entry.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key does not exist in the metadata.</exception>
        string GetMetadata(string key);

        /// <summary>
        /// Creates a transition to request approval from a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="role">The approver's role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if approver is null.</exception>
        /// <exception cref="ArgumentException">Thrown if approver is empty or whitespace.</exception>
        ICanAction RequestApproval(string approver, string role, string comments = null);

        /// <summary>
        /// Creates a transition to request approval from a specified group of approvers.
        /// </summary>
        /// <param name="group">The group of approvers.</param>
        /// <param name="role">The approvers' role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if group is null.</exception>
        ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments = null);

        /// <summary>
        /// Marks the current user's transition as "Checked In".
        /// </summary>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="NullReferenceException">Thrown if no transition is found for the current user.</exception>
        ICanAction CheckIn();

        /// <summary>
        /// Approves the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the approval.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="NullReferenceException">Thrown if no transition is found for the current user.</exception>
        ICanAction Approve(string comments = null);

        /// <summary>
        /// Rejects the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the rejection.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="NullReferenceException">Thrown if no transition is found for the current user.</exception>
        ICanAction Reject(string comments = null);

        /// <summary>
        /// Invalidates a transition for a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="comments">Optional comments to include with the invalidation.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="NullReferenceException">Thrown if no transition is found for the specified approver.</exception>
        ICanAction Invalidate(string approver, string comments = null);

        /// <summary>
        /// Loads a notification for a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="usersToCC">Optional list of users to CC.</param>
        /// <param name="mailsToCC">Optional list of email addresses to CC.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the approver.</exception>
        ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null);

        /// <summary>
        /// Loads notifications for a specified approver group.
        /// </summary>
        /// <param name="group">The approver group for which to load notifications.</param>
        /// <param name="usersToCC">An optional array of usernames to CC in the notification.</param>
        /// <param name="mailsToCC">An optional array of email addresses to CC in the notification.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to allow chaining of additional actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="group"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the specified group.</exception>
        ICanAction LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null);

        /// <summary>
        /// Sets metadata for the approval flow entity by adding key details such as ID, short description, and long description.
        /// </summary>
        /// <remarks>
        /// This method extracts metadata from the entity using its methods and adds it to the internal metadata dictionary.
        /// If a key already exists, it will be overwritten. Keys added include: "id", "shortDescription", and "longDescription".
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the metadata dictionary already contains a key being added.</exception>
        void SetEntityMetaData();

        /// <summary>
        /// Gets all pending notifications.
        /// </summary>
        /// <returns>A collection of notifications, which may be empty if no notifications exist.</returns>
        IEnumerable<IPAFNotification> GetPAFNotifications();

        /// <summary>
        /// Clears all notifications.
        /// </summary>
        /// <remarks>
        /// This method removes all notifications from the internal notification cache. 
        /// If there are no notifications, this method has no effect.
        /// </remarks>
        void ClearNotifications();
    }
}
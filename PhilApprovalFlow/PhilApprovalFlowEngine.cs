using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Interfaces;
using PhilApprovalFlow.Managers;
using System;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Provides a workflow engine for handling approval flows.
    /// Coordinates between different managers to provide a unified approval workflow interface.
    /// </summary>
    /// <typeparam name="T">The type of the approval transition, implementing IPAFTransition.</typeparam>
    internal class PhilApprovalFlowEngine<T> : ICanSetUser, ICanAction where T : IPAFTransition, new()
    {
        private readonly MetadataManager<T> metadataManager;
        private readonly NotificationManager<T> notificationManager;
        private readonly TransitionManager<T> transitionManager;
        private string userContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhilApprovalFlowEngine{T}"/> class.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        private PhilApprovalFlowEngine(ref IApprovalFlow<T> entity)
        {
            metadataManager = new MetadataManager<T>(entity);
            notificationManager = new NotificationManager<T>(entity);
            transitionManager = new TransitionManager<T>(entity);
        }

        /// <summary>
        /// Creates an instance of <see cref="PhilApprovalFlowEngine{T}"/> for a specified entity.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <returns>An instance of <see cref="ICanSetUser"/> to configure the user context.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public static ICanSetUser SetEntity(ref IApprovalFlow<T> entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null.");
            }
            return new PhilApprovalFlowEngine<T>(ref entity);
        }

        #region ICanSetUser Implementation

        /// <summary>
        /// Sets the username context for the approval flow.
        /// </summary>
        /// <param name="username">The username of the current user who will perform subsequent actions.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="username"/> is null, empty, or contains only whitespace.
        /// The exception message will be: "Username cannot be null or whitespace".
        /// </exception>
        public ICanAction SetUserName(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be null or whitespace", nameof(username));
            }

            userContext = username;
            return this;
        }

        /// <summary>
        /// Resets all transitions to a default "Awaiting Decision" state with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to associate with the reset transitions.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction ResetTransitions(string comments = null)
        {
            transitionManager.ResetAllTransitions(comments);
            return this;
        }

        #endregion

        #region ICanAction Implementation - Metadata Operations

        /// <summary>
        /// Adds or updates metadata associated with the approval flow.
        /// </summary>
        /// <param name="key">The key of the metadata entry.</param>
        /// <param name="value">The value of the metadata entry.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">Thrown if the key is null or whitespace.</exception>
        /// <exception cref="ArgumentNullException">Thrown if the value is null.</exception>
        public ICanAction SetMetadata(string key, string value)
        {
            metadataManager.SetMetadata(key, value);
            return this;
        }

        /// <summary>
        /// Retrieves a metadata value by its key.
        /// </summary>
        /// <param name="key">The key of the metadata entry.</param>
        /// <returns>The value associated with the specified key.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key does not exist in the metadata.</exception>
        public string GetMetadata(string key)
        {
            return metadataManager.GetMetadata(key);
        }

        /// <summary>
        /// Sets metadata for the approval flow entity by adding key details such as ID, short description, and long description.
        /// </summary>
        /// <remarks>
        /// This method extracts metadata from the entity using its methods and adds it to the internal metadata dictionary.
        /// </remarks>
        public void SetEntityMetaData()
        {
            metadataManager.SetEntityMetaData();
        }

        #endregion

        #region ICanAction Implementation - Transition Operations

        /// <summary>
        /// Creates a transition to request approval from a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier. Cannot be null or empty.</param>
        /// <param name="role">The approver's role. This identifies the function or responsibility of the approver.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if approver or userContext is null.</exception>
        /// <exception cref="ArgumentException">Thrown if approver or userContext is empty or whitespace.</exception>
        public ICanAction RequestApproval(string approver, string role, string comments = null)
        {
            EnsureUserContext();
            transitionManager.CreateTransition(userContext, approver, role, comments);
            return this;
        }

        /// <summary>
        /// Creates a transition to request approval from a specified group of approvers.
        /// </summary>
        /// <param name="group">The group of approvers.</param>
        /// <param name="role">The approvers' role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if requester or group is null.</exception>
        public ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments = null)
        {
            EnsureUserContext();
            transitionManager.CreateTransition(userContext, group, role, comments);
            return this;
        }

        /// <summary>
        /// Marks the current user's transition as "Checked In".
        /// </summary>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the current user.</exception>
        public ICanAction CheckIn()
        {
            EnsureUserContext();
            transitionManager.CheckIn(userContext);
            return this;
        }

        /// <summary>
        /// Approves the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the approval.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the current user.</exception>
        public ICanAction Approve(string comments = null)
        {
            EnsureUserContext();
            transitionManager.SetUserDecision(userContext, DecisionType.Approved, comments);
            return this;
        }

        /// <summary>
        /// Rejects the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the rejection.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the current user.</exception>
        public ICanAction Reject(string comments = null)
        {
            EnsureUserContext();
            transitionManager.SetUserDecision(userContext, DecisionType.Rejected, comments);
            return this;
        }

        /// <summary>
        /// Invalidates a transition for a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="comments">Optional comments to include with the invalidation.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the approver.</exception>
        public ICanAction Invalidate(string approver, string comments = null)
        {
            EnsureUserContext();
            transitionManager.InvalidateTransition(approver, comments);
            return this;
        }

        #endregion

        #region ICanAction Implementation - Notification Operations

        /// <summary>
        /// Loads a notification for a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="usersToCC">Optional list of users to CC.</param>
        /// <param name="mailsToCC">Optional list of email addresses to CC.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the approver.</exception>
        public ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null)
        {
            notificationManager.LoadNotification(approver, usersToCC, mailsToCC);
            return this;
        }

        /// <summary>
        /// Loads notifications for a specified approver group.
        /// </summary>
        /// <param name="group">The approver group for which to load notifications.</param>
        /// <param name="usersToCC">An optional array of usernames to CC in the notification.</param>
        /// <param name="mailsToCC">An optional array of email addresses to CC in the notification.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to allow chaining of additional actions.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="group"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the specified group.</exception>
        public ICanAction LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null)
        {
            notificationManager.LoadNotification(group, usersToCC, mailsToCC);
            return this;
        }

        /// <summary>
        /// Gets all pending notifications.
        /// </summary>
        /// <returns>A collection of notifications.</returns>
        public IEnumerable<IPAFNotification> GetPAFNotifications()
        {
            return notificationManager.GetPAFNotifications();
        }

        /// <summary>
        /// Clears all notifications.
        /// </summary>
        public void ClearNotifications()
        {
            notificationManager.ClearNotifications();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Ensures that a user context has been set before performing operations.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if no user context has been set.</exception>
        private void EnsureUserContext()
        {
            if (string.IsNullOrWhiteSpace(userContext))
            {
                throw new InvalidOperationException("User context must be set before performing this operation. Call SetUserName first.");
            }
        }

        #endregion
    }
}
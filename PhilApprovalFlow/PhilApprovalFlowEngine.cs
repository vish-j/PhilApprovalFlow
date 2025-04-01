using PhilApprovalFlow.Attributes;
using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Interfaces;
using PhilApprovalFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Provides a workflow engine for handling approval flows.
    /// </summary>
    /// <typeparam name="T">The type of the approval transition, implementing IPAFTransition.</typeparam>
    internal class PhilApprovalFlowEngine<T> : ICanSetUser, ICanAction where T : IPAFTransition, new()
    {
        private readonly IApprovalFlow<T> approvalFlowEntity;
        private string userContext;
        private readonly Dictionary<string, string> metadata;
        private List<PAFNotification> pafNotifications;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhilApprovalFlowEngine{T}"/> class.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        private PhilApprovalFlowEngine(ref IApprovalFlow<T> entity)
        {
            approvalFlowEntity = entity;
            metadata = entity.GetType().GetCustomAttributes<PAFMetadataAttribute>().ToDictionary(c => c.Key, c => c.Value);
        }

        /// <summary>
        /// Creates an instance of <see cref="PhilApprovalFlowEngine{T}"/> for a specified entity.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <returns>An instance of <see cref="ICanSetUser"/> to configure the user context.</returns>
        public static ICanSetUser SetEntity(ref IApprovalFlow<T> entity)
        {
            return new PhilApprovalFlowEngine<T>(ref entity);
        }

        /// <summary>
        /// Sets the username context for the approval flow.
        /// </summary>
        /// <param name="username">The username of the current user.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">Thrown if the username is null or whitespace.</exception>
        public ICanAction SetUserName(string username)
        {
            userContext = username;
            return this;
        }

        /// <summary>
        /// Resets all transitions to a default "Awaiting Decision" state with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to associate with the reset transitions.</param>
        public void ResetTransitions(string comments = null)
        {
            foreach (var item in approvalFlowEntity.Transitions)
            {
                SetDecision(item, DecisionType.AwaitingDecision, comments);
            }
        }

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
            metadata.Add(key, value);
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
            return metadata[key];
        }

        /// <summary>
        /// Creates a transition to request approval from a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="role">The approver's role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction RequestApproval(string approver, string role, string comments = null)
        {
            CreateTransition(userContext, approver, role, comments);
            return this;
        }

        /// <summary>
        /// Creates a transition to request approval from a specified group of approvers.
        /// </summary>
        /// <param name="group">The group of approvers.</param>
        /// <param name="role">The approvers' role.</param>
        /// <param name="comments">Optional comments to include with the request.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments = null)
        {
            CreateTransition(userContext, group, role, comments);
            return this;
        }

        /// <summary>
        /// Marks the current user's transition as "Checked In".
        /// </summary>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction CheckIn()
        {
            IPAFTransition transition = GetTransition(userContext);
            Checkin(transition);
            return this;
        }

        /// <summary>
        /// Approves the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the approval.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction Approve(string comments = null)
        {
            IPAFTransition transition = GetTransition(userContext);
            SetDecision(transition, DecisionType.Approved, comments);
            return this;
        }

        /// <summary>
        /// Rejects the current user's transition with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to include with the rejection.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction Reject(string comments = null)
        {
            IPAFTransition transition = GetTransition(userContext);
            SetDecision(transition, DecisionType.Rejected, comments);
            return this;
        }

        /// <summary>
        /// Invalidates a transition for a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="comments">Optional comments to include with the invalidation.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        public ICanAction Invalidate(string approver, string comments = null)
        {
            IPAFTransition transition = GetTransition(approver);
            SetDecision(transition, DecisionType.Invalidated, comments);
            return this;
        }

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
            IPAFTransition transition = GetTransition(approver) ?? throw new NullReferenceException("No transition found by approver");
            AddNotification(transition.RequesterID, transition.ApproverID, transition.ApproverDecision, usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments);
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
            IPAFTransition transition = GetTransition(group) ?? throw new NullReferenceException("No transition found by group");

            if (transition.ApproverID == null)
            {
                foreach (var approver in group)
                {
                    AddNotification(transition.RequesterID, approver, transition.ApproverDecision, usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments, group.GroupID);
                }
            }
            else
            {
                AddNotification(transition.RequesterID, transition.ApproverID, transition.ApproverDecision, usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments);
            }

            return this;
        }

        private void AddNotification(string requesterID, string approverID, DecisionType decision, string[] usersToCC, string[] mailsToCC, string requesterComments, string approverComments, long? groupID = null)
        {
            string to;
            string from = null;
            string comments;
            if (decision == DecisionType.AwaitingDecision || decision == DecisionType.Invalidated)
            {
                to = approverID;
                comments = requesterComments;
                if (approverID != requesterID)
                {
                    from = requesterID;
                }
            }
            else
            {
                to = requesterID;
                comments = approverComments;
                if (approverID != requesterID)
                {
                    from = approverID;
                }
            }

            if (pafNotifications == null)
            {
                pafNotifications = new List<PAFNotification>();
            }

            pafNotifications.Add(new PAFNotification
            {
                From = from,
                To = to,
                Comments = comments,
                DecisionType = decision,
                UsersToCC = usersToCC,
                MailsToCC = mailsToCC,
                GroupID = groupID
            });
        }

        /// <summary>
        /// Sets metadata for the approval flow entity by adding key details such as ID, short description, and long description.
        /// </summary>
        /// <remarks>
        /// This method extracts metadata from the entity using its methods and adds it to the internal metadata dictionary.
        /// If a key already exists, an exception will be thrown. Ensure the metadata dictionary is clear of duplicates before calling this method.
        /// </remarks>
        /// <exception cref="ArgumentException">Thrown if the metadata dictionary already contains a key being added.</exception>
        public void SetEntityMetaData()
        {
            metadata.Add("id", approvalFlowEntity.GetID().ToString());
            metadata.Add("shortDescription", approvalFlowEntity.GetShortDescription());
            metadata.Add("longDescription", approvalFlowEntity.GetLongDescription());
        }

        /// <summary>
        /// Gets all pending notifications.
        /// </summary>
        /// <returns>A collection of notifications.</returns>
        public IEnumerable<IPAFNotification> GetPAFNotifications()
        {
            return pafNotifications ?? new List<PAFNotification>();
        }

        /// <summary>
        /// Clears all notifications.
        /// </summary>
        public void ClearNotifications()
        {
            pafNotifications?.Clear();
        }

        private void Checkin(IPAFTransition transition)
        {
            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.ApproverCheckInDate = (DateTime?)DateTime.Now;
        }

        private void SetDecision(IPAFTransition transition, DecisionType decision, string comments)
        {
            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.ApproverDecision = decision;

            if (transition.ApproverCheckInDate == null && decision != DecisionType.AwaitingDecision)
            {
                transition.ApproverCheckInDate = (DateTime?)DateTime.Now;
            }

            transition.AcknowledgementDate = decision == DecisionType.AwaitingDecision ? null : (DateTime?)DateTime.Now;
            transition.ApproverComments = comments;
        }

        private void CreateTransition(string requester, string approver, string role, string comments)
        {
            if (requester == null)
            {
                throw new ArgumentNullException("Requester is missing");
            }

            if (approver == null)
            {
                throw new ArgumentNullException("Approver is missing");
            }

            if (!approvalFlowEntity.Transitions.Any(t => t.ApproverID == approver))
            {
                int order = !approvalFlowEntity.Transitions.Any() ? 1 : approvalFlowEntity.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initalize(order, requester, approver, role, comments);
                approvalFlowEntity.Transitions.Add(newTransition);
            }

            if (approvalFlowEntity.Transitions.Any(t => t.ApproverID == approver && (t.ApproverDecision == DecisionType.Invalidated || t.ApproverDecision == DecisionType.AwaitingDecision)))
            {
                IPAFTransition transition = GetTransition(approver);
                transition.ApproverRole = role;
                if (comments != null)
                {
                    transition.RequesterComments = comments;
                }

                SetDecision(transition, DecisionType.AwaitingDecision, comments);
            }
        }

        private void CreateTransition(string requester, IPAFApproverGroup group, string role, string comments)
        {
            if (requester == null)
            {
                throw new ArgumentNullException("Requester is missing");
            }

            if (group == null)
            {
                throw new ArgumentNullException("Approver Group is missing");
            }

            if (!approvalFlowEntity.Transitions.Any(t => t.ApproverGroup == group))
            {
                int order = !approvalFlowEntity.Transitions.Any() ? 1 : approvalFlowEntity.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initalize(order, requester, group, role, comments);
                approvalFlowEntity.Transitions.Add(newTransition);
            }

            if (approvalFlowEntity.Transitions.Any(t => t.ApproverGroup == group && (t.ApproverDecision == DecisionType.Invalidated || t.ApproverDecision == DecisionType.AwaitingDecision)))
            {
                IPAFTransition transition = GetTransition(group);
                if (transition.ApproverGroup == null)
                {
                    transition.ApproverGroup = group as PAFApproverGroup;
                }
                transition.ApproverRole = role;
                if (comments != null)
                {
                    transition.RequesterComments = comments;
                }

                SetDecision(transition, DecisionType.AwaitingDecision, comments);
            }
        }

        private IPAFTransition GetTransition(string approver)
        {
            return approvalFlowEntity.Transitions
                .Where(t =>
                    t.ApproverID == approver ||
                    (((t.ApproverGroup?.GroupID ?? 0) != 0) &&
                     t.ApproverID == null &&
                     t.ApproverGroup.Contains(approver) &&
                     t.ApproverGroup.IsActive()))
                .FirstOrDefault();
        }


        private IPAFTransition GetTransition(IPAFApproverGroup group)
        {
            return approvalFlowEntity.Transitions
                .Where(t => t.ApproverGroup.GroupID == group.GroupID)
                .FirstOrDefault();
        }
    }
}
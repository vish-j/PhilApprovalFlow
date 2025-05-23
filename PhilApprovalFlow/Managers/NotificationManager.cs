using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Helpers;
using PhilApprovalFlow.Interfaces;
using System;
using System.Collections.Generic;

namespace PhilApprovalFlow.Managers
{
    /// <summary>
    /// Manages notification operations for approval workflows.
    /// </summary>
    /// <typeparam name="T">The type of the approval transition.</typeparam>
    internal class NotificationManager<T> where T : IPAFTransition
    {
        private readonly List<PAFNotification> pafNotifications;
        private readonly TransitionFinder<T> transitionFinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationManager{T}"/> class.
        /// </summary>
        /// <param name="entity">The approval flow entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if the entity is null.</exception>
        public NotificationManager(IApprovalFlow<T> entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
                
            transitionFinder = new TransitionFinder<T>(entity);
            pafNotifications = new List<PAFNotification>();
        }

        /// <summary>
        /// Loads a notification for a specified approver.
        /// </summary>
        /// <param name="approver">The approver's identifier.</param>
        /// <param name="usersToCC">Optional list of users to CC.</param>
        /// <param name="mailsToCC">Optional list of email addresses to CC.</param>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the approver.</exception>
        public void LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null)
        {
            IPAFTransition transition = transitionFinder.GetTransition(approver) 
                ?? throw new InvalidOperationException($"No transition found for approver '{approver}'");
                
            AddNotification(transition.RequesterID, transition.ApproverID, transition.ApproverDecision, 
                          usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments);
        }

        /// <summary>
        /// Loads notifications for a specified approver group.
        /// </summary>
        /// <param name="group">The approver group for which to load notifications.</param>
        /// <param name="usersToCC">An optional array of usernames to CC in the notification.</param>
        /// <param name="mailsToCC">An optional array of email addresses to CC in the notification.</param>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="group"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if no transition is found for the specified group.</exception>
        public void LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null)
        {
            if (group == null)
            {
                throw new ArgumentNullException(nameof(group), "Approver group cannot be null");
            }

            IPAFTransition transition = transitionFinder.FindTransitionByGroup(group) 
                ?? throw new InvalidOperationException($"No transition found for group with ID {group.GroupID}");

            if (transition.ApproverID == null)
            {
                foreach (var approver in group)
                {
                    AddNotification(transition.RequesterID, approver, transition.ApproverDecision, 
                                  usersToCC, mailsToCC, transition.RequesterComments, 
                                  transition.ApproverComments, group.GroupID);
                }
            }
            else
            {
                AddNotification(transition.RequesterID, transition.ApproverID, transition.ApproverDecision, 
                              usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments);
            }
        }

        /// <summary>
        /// Gets all pending notifications.
        /// </summary>
        /// <returns>A collection of notifications.</returns>
        public IEnumerable<IPAFNotification> GetPAFNotifications()
        {
            return pafNotifications;
        }

        /// <summary>
        /// Clears all notifications from the internal cache.
        /// </summary>
        public void ClearNotifications()
        {
            pafNotifications?.Clear();
        }

        /// <summary>
        /// Adds a notification to the internal cache based on transition state.
        /// </summary>
        /// <param name="requesterID">The requester's identifier.</param>
        /// <param name="approverID">The approver's identifier.</param>
        /// <param name="decision">The current decision state of the transition.</param>
        /// <param name="usersToCC">Optional users to copy on the notification.</param>
        /// <param name="mailsToCC">Optional email addresses to copy on the notification.</param>
        /// <param name="requesterComments">Comments from the requester.</param>
        /// <param name="approverComments">Comments from the approver.</param>
        /// <param name="groupID">Optional group ID if this is a group notification.</param>
        private void AddNotification(string requesterID, string approverID, DecisionType decision, 
                                   string[] usersToCC, string[] mailsToCC, string requesterComments, 
                                   string approverComments, long? groupID = null)
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
    }
}
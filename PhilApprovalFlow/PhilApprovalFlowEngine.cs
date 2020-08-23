using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow
{
    internal class PhilApprovalFlowEngine<T> : ICanSetUser, ICanAction where T : IPAFTransition, new()
    {
        private IApprovalFlow<T> approvalFlow;
        private string user;
        private Dictionary<string, string> metadata;
        private IPAFTransition transition;
        private List<PAFNotification> pafNotifications;

        private PhilApprovalFlowEngine(ref IApprovalFlow<T> value)
        {
            approvalFlow = value;
        }

        public static ICanSetUser SetEntity(ref IApprovalFlow<T> entity)
        {
            return new PhilApprovalFlowEngine<T>(ref entity);
        }

        public ICanAction SetUserName(string username)
        {
            user = username;
            return this;
        }

        /// <summary>
        /// Reset Approvals for every transistion to Awaiting Decision
        /// </summary>
        /// <param name="comments"></param>
        public void ResetTransitions(string comments = null)
        {
            foreach (var item in approvalFlow.Transitions)
            {
                user = item.ApproverID;
                setDecision(DecisionType.AwaitingDecision, comments);
            }
        }

        public ICanAction SetMetadata(string key, string value)
        {
            if (metadata == null)
                metadata = new Dictionary<string, string>();

            metadata.Add(key, value);
            return this;
        }

        public string GetMetadata(string key) => metadata[key];
        

        public ICanAction RequestApproval(string approver) =>
             RequestApproval(approver, null);

        public ICanAction RequestApproval(string approver, string Comments)
        {
            createTransition(user, approver, Comments);
            return this;
        }

        public ICanAction Approve(string comments = null)
        {
            setDecision(DecisionType.Approved, comments);
            return this;
        }

        public ICanAction Reject(string comments = null)
        {
            setDecision(DecisionType.Rejected, comments);
            return this;
        }

        public ICanAction Invalidate(string username, string comments = null)
        {
            if (username != null)
            {
                user = username;

                setDecision(DecisionType.Invalidated, comments);
            }
            return this;
        }

        public ICanAction LoadNotification(string approver, string[] usersToCC = null)
        {
            if (transition != null)
            {
                transition = approvalFlow.Transitions.Where(t => t.ApproverID == approver).FirstOrDefault();
            }

            string to;
            string from = null;
            string comments;
            if (transition.ApproverDecision == DecisionType.AwaitingDecision || transition.ApproverDecision == DecisionType.Invalidated)
            {
                to = transition.ApproverID;
                comments = transition.RequesterComments;
                if (transition.ApproverID != transition.RequesterID)
                    from = transition.RequesterID;
            }
            else
            {
                to = transition.RequesterID;
                comments = transition.ApproverComments;
                if (transition.ApproverID != transition.RequesterID)
                    from = transition.ApproverID;
            }

            if (pafNotifications == null)
                pafNotifications = new List<PAFNotification>();

            pafNotifications.Add(new PAFNotification
            {
                From = from,
                To = to,
                Comments = comments,
                DecisionType = transition.ApproverDecision,
                UsersToCC = usersToCC,
            });

            return this;
        }

        public void ClearNotifications()
        {
            if (pafNotifications != null)
                pafNotifications.Clear();
        }

        private void setDecision(DecisionType decision, string comments)
        {
            transition = approvalFlow.Transitions.Where(t => t.ApproverID == user).FirstOrDefault();
            if (transition != null)
            {
                transition.ApproverDecision = decision;
                transition.AcknowledgementDate = decision == DecisionType.AwaitingDecision ? null : (DateTime?)DateTime.Now;
                transition.ApproverComments = comments;
                editTransition((PAFTransition)transition);
            }
        }

        private void createTransition(string requester, string approver, string comments)
        {
            if (!approvalFlow.Transitions.Any(t => t.ApproverID == approver))
            {
                int order = !approvalFlow.Transitions.Any() ? 1 : approvalFlow.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initalize(order, requester, approver, comments);
                approvalFlow.Transitions.Add(newTransition);
            }

            if (approvalFlow.Transitions.Any(t => t.ApproverID == approver && t.ApproverDecision == DecisionType.Invalidated))
            {
                user = approver;
                setDecision(DecisionType.AwaitingDecision, comments);
            }
        }

        private void editTransition(PAFTransition transition)
        {
            var t = approvalFlow.Transitions.FirstOrDefault(tr => tr.TransitionID == transition.TransitionID);

            t.Order = transition.Order;
            t.RequesterID = transition.RequesterID;
            t.RequestedDate = transition.RequestedDate;
            t.ApproverID = transition.ApproverID;
            t.ApproverDecision = transition.ApproverDecision;
            t.RequesterComments = transition.RequesterComments;
            t.ApproverComments = transition.ApproverComments;
            t.AcknowledgementDate = transition.AcknowledgementDate;
        }
    }
}
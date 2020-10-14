using PhilApprovalFlow.Attributes;
using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhilApprovalFlow
{
    internal class PhilApprovalFlowEngine<T> : ICanSetUser, ICanAction where T : IPAFTransition, new()
    {
        private IApprovalFlow<T> approvalFlowEntity;
        private string user;
        private Dictionary<string, string> metadata;
        private List<PAFNotification> pafNotifications;

        private PhilApprovalFlowEngine(ref IApprovalFlow<T> entity)
        {
            approvalFlowEntity = entity;
            metadata = entity.GetType().GetCustomAttributes<PAFMetadataAttribute>().ToDictionary(c => c.Key, c => c.Value);
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
            foreach (var item in approvalFlowEntity.Transitions)
            {
                setDecision(DecisionType.AwaitingDecision, item.ApproverID, comments);
            }
        }

        public ICanAction SetMetadata(string key, string value)
        {
            metadata.Add(key, value);
            return this;
        }

        public string GetMetadata(string key) => metadata[key];

        public ICanAction RequestApproval(string approver, string role) =>
             RequestApproval(approver, role, null);

        public ICanAction RequestApproval(string approver, string role, string comments)
        {
            createTransition(user, approver, role, comments);
            return this;
        }

        public ICanAction CheckIn()
        {
            checkin(user);
            return this;
        }

        public ICanAction Approve(string comments = null)
        {
            setDecision(DecisionType.Approved, user, comments);
            return this;
        }

        public ICanAction Reject(string comments = null)
        {
            setDecision(DecisionType.Rejected, user, comments);
            return this;
        }

        public ICanAction Invalidate(string username, string comments = null)
        {
            setDecision(DecisionType.Invalidated, username, comments);

            return this;
        }

        public ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null)
        {
            IPAFTransition transition = approvalFlowEntity.Transitions.Where(t => t.ApproverID == approver).FirstOrDefault();

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
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
                MailsToCC = mailsToCC,
            });

            return this;
        }

        public void SetEntityMetaData()
        {
            metadata.Add("id", approvalFlowEntity.GetID().ToString());
            metadata.Add("shortDescription", approvalFlowEntity.GetShortDescription());
            metadata.Add("longDescription", approvalFlowEntity.GetLongDescription());
        }

        public IEnumerable<IPAFNotification> GetPAFNotifications()
        {
            return pafNotifications ?? new List<PAFNotification>();
        }

        public void ClearNotifications()
        {
            if (pafNotifications != null)
                pafNotifications.Clear();
        }

        private void checkin(string approver)
        {
            IPAFTransition transition = approvalFlowEntity.Transitions.Where(t => t.ApproverID == approver).FirstOrDefault();

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.ApproverCheckInDate = (DateTime?)DateTime.Now;

            editTransition((PAFTransition)transition);
        }

        private void setDecision(DecisionType decision, string approver, string comments)
        {
            IPAFTransition transition = approvalFlowEntity.Transitions.Where(t => t.ApproverID == approver).FirstOrDefault();

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.ApproverDecision = decision;
            if (transition.ApproverCheckInDate == null && decision != DecisionType.AwaitingDecision)
                transition.ApproverCheckInDate = (DateTime?)DateTime.Now;
            transition.AcknowledgementDate = decision == DecisionType.AwaitingDecision ? null : (DateTime?)DateTime.Now;
            transition.ApproverComments = comments;

            editTransition((PAFTransition)transition);
        }

        private void createTransition(string requester, string approver, string role, string comments)
        {
            if (!approvalFlowEntity.Transitions.Any(t => t.ApproverID == approver))
            {
                int order = !approvalFlowEntity.Transitions.Any() ? 1 : approvalFlowEntity.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initalize(order, requester, approver, role, comments);
                approvalFlowEntity.Transitions.Add(newTransition);
            }

            if (approvalFlowEntity.Transitions.Any(t => t.ApproverID == approver && t.ApproverDecision == DecisionType.Invalidated))
            {
                setDecision(DecisionType.AwaitingDecision, approver, comments);
            }
        }

        private void editTransition(PAFTransition pafT)
        {
            IPAFTransition transition = approvalFlowEntity.Transitions.Where(t => t.ApproverID == pafT.ApproverID).FirstOrDefault();

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.Order = pafT.Order;
            transition.RequesterID = pafT.RequesterID;
            transition.RequestedDate = pafT.RequestedDate;
            transition.ApproverID = pafT.ApproverID;
            transition.ApproverDecision = pafT.ApproverDecision;
            transition.RequesterComments = pafT.RequesterComments;
            transition.ApproverComments = pafT.ApproverComments;
            transition.AcknowledgementDate = pafT.AcknowledgementDate;
        }
    }
}
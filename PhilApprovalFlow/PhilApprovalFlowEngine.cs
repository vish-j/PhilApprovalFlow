﻿using PhilApprovalFlow.Attributes;
using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Interfaces;
using PhilApprovalFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PhilApprovalFlow
{
    internal class PhilApprovalFlowEngine<T> : ICanSetUser, ICanAction where T : IPAFTransition, new()
    {
        private readonly IApprovalFlow<T> approvalFlowEntity;
        private string userContext;
        private readonly Dictionary<string, string> metadata;
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
            userContext = username;
            return this;
        }

        public void ResetTransitions(string comments = null)
        {
            foreach (var item in approvalFlowEntity.Transitions)
            {
                setDecision(item, DecisionType.AwaitingDecision, comments);
            }
        }

        public ICanAction SetMetadata(string key, string value)
        {
            metadata.Add(key, value);
            return this;
        }

        public string GetMetadata(string key)
        {
            return metadata[key];
        }

        public ICanAction RequestApproval(string approver, string role)
        {
            return RequestApproval(approver, role, null);
        }

        public ICanAction RequestApproval(string approver, string role, string comments)
        {
            createTransition(userContext, approver, role, comments);
            return this;
        }

        public ICanAction RequestApproval(IPAFApproverGroup group, string role)
        {
            return RequestApproval(group, role, null);
        }

        public ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments)
        {
            createTransition(userContext, group, role, comments);
            return this;
        }

        public ICanAction CheckIn()
        {
            IPAFTransition transition = getTransition(userContext);
            checkin(transition);
            return this;
        }

        public ICanAction Approve(string comments = null)
        {
            IPAFTransition transition = getTransition(userContext);
            setDecision(transition, DecisionType.Approved, comments);
            return this;
        }

        public ICanAction Reject(string comments = null)
        {
            IPAFTransition transition = getTransition(userContext);
            setDecision(transition, DecisionType.Rejected, comments);
            return this;
        }

        public ICanAction Invalidate(string approver, string comments = null)
        {
            IPAFTransition transition = getTransition(approver);
            setDecision(transition, DecisionType.Invalidated, comments);
            return this;
        }

        public ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null)
        {
            IPAFTransition transition = getTransition(approver);

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            addNotification(transition.RequesterID, transition.ApproverID, transition.ApproverDecision, usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments);
            return this;
        }

        public ICanAction LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null)
        {
            IPAFTransition transition = getTransition(group);

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            if (transition.ApproverID == null)
            {
                foreach (var approver in group)
                {
                    addNotification(transition.RequesterID, approver, transition.ApproverDecision, usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments, group.GroupID);
                }
            }
            else
            {
                addNotification(transition.RequesterID, transition.ApproverID, transition.ApproverDecision, usersToCC, mailsToCC, transition.RequesterComments, transition.ApproverComments);
            }

            return this;
        }

        private void addNotification(string requesterID, string approverID, DecisionType decision, string[] usersToCC, string[] mailsToCC, string requesterComments, string approverComments, long? groupID = null)
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
            });
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
            {
                pafNotifications.Clear();
            }
        }

        private void checkin(IPAFTransition transition)
        {
            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.ApproverCheckInDate = (DateTime?)DateTime.Now;

            editTransition((PAFTransition)transition);
        }

        private void setDecision(IPAFTransition transition, DecisionType decision, string comments)
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

            editTransition((PAFTransition)transition);
        }

        private void createTransition(string requester, string approver, string role, string comments)
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
                IPAFTransition transition = getTransition(approver);
                transition.ApproverRole = role;
                if (comments != null)
                {
                    transition.RequesterComments = comments;
                }

                setDecision(transition, DecisionType.AwaitingDecision, comments);
            }
        }

        private void createTransition(string requester, IPAFApproverGroup group, string role, string comments)
        {
            if (requester == null)
            {
                throw new ArgumentNullException("Requester is missing");
            }

            if (group == null)
            {
                throw new ArgumentNullException("Approver Group is missing");
            }

            if (!approvalFlowEntity.Transitions.Any(t => t.ApproverGroup == group || group.Contains(t.ApproverID)))
            {
                int order = !approvalFlowEntity.Transitions.Any() ? 1 : approvalFlowEntity.Transitions.Max(a => a.Order) + 1;
                var newTransition = new T();
                newTransition.Initalize(order, requester, group, role, comments);
                approvalFlowEntity.Transitions.Add(newTransition);
            }

            if (approvalFlowEntity.Transitions.Any(t => group.Contains(t.ApproverID) && (t.ApproverDecision == DecisionType.Invalidated || t.ApproverDecision == DecisionType.AwaitingDecision)))
            {
                IPAFTransition transition = getTransition(group);
                if (transition.ApproverGroup == null)
                {
                    transition.ApproverGroup = group as PAFApproverGroup;
                }
                transition.ApproverRole = role;
                if (comments != null)
                {
                    transition.RequesterComments = comments;
                }

                setDecision(transition, DecisionType.AwaitingDecision, comments);
            }
        }

        private void editTransition(PAFTransition pafT)
        {
            IPAFTransition transition = getTransition(pafT.ApproverID);

            if (transition == null)
            {
                throw new NullReferenceException("No transition found");
            }

            transition.Order = pafT.Order;
            transition.RequesterID = pafT.RequesterID;
            transition.RequestedDate = pafT.RequestedDate;
            transition.ApproverID = pafT.ApproverID;
            transition.ApproverGroup = pafT.ApproverGroup;
            transition.ApproverRole = pafT.ApproverRole;
            transition.ApproverDecision = pafT.ApproverDecision;
            transition.RequesterComments = pafT.RequesterComments;
            transition.ApproverComments = pafT.ApproverComments;
            transition.AcknowledgementDate = pafT.AcknowledgementDate;
        }

        private IPAFTransition getTransition(string approver)
        {
            return approvalFlowEntity.Transitions.Where(t => t.ApproverID == approver || (t.ApproverGroup != null && (t.ApproverGroup.Contains(approver) && t.ApproverGroup.IsActive)) && t.ApproverID == null).FirstOrDefault();
        }

        private IPAFTransition getTransition(IPAFApproverGroup group)
        {
            return approvalFlowEntity.Transitions.Where(t => t.ApproverGroup == group || (group.Contains(t.ApproverID))).FirstOrDefault();
        }
    }
}
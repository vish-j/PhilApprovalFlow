using PhilApprovalFlow.Enum;
using PhilApprovalFlow.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow
{
    internal class ApprovalEngine<T> : ICanSetUser, ICanAction where T : ITransition
    {
        private IApprovalFlow<T> _approvalFlow;
        private string _user;
        private ITransition _transition;
        private List<AFNotification> aFNotifications;

        private ApprovalEngine(ref IApprovalFlow<T> value)
        {
            _approvalFlow = value;
        }

        public static ICanSetUser SetEntity(ref IApprovalFlow<T> entity)
        {
            return new ApprovalEngine<T>(ref entity);
        }

        public ICanAction SetUserName(string username)
        {
            _user = username;
            return this;
        }

        /// <summary>
        /// Reset Approvals for every transistion
        /// </summary>
        /// <param name="comments"></param>
        public void ResetTransitions(string comments = null)
        {
            foreach (var item in _approvalFlow.Transitions)
            {
                _user = item.ApproverID;
                setDecision(DecisionType.AwaitingDecision, comments);
            }
        }

        public ICanAction RequestApproval(string approver) =>
             RequestApproval(approver, null);

        public ICanAction RequestApproval(string approver, string Comments)
        {
            createTransition(_user, approver, Comments);
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
                _user = username;

                setDecision(DecisionType.Invalidated, comments);
            }
            return this;
        }

        public ICanAction LoadNotification(string approver, string[] usersToCC = null)
        {
            if (_transition != null)
            {
                _transition = _approvalFlow.Transitions.Where(t => t.ApproverID == approver).FirstOrDefault();
            }

            string to;
            string from = null;
            string comments;
            if (_transition.ApproverDecision == DecisionType.AwaitingDecision || _transition.ApproverDecision == DecisionType.Invalidated)
            {
                to = _transition.ApproverID;
                comments = _transition.RequesterComments;
                if (_transition.ApproverID != _transition.RequesterID)
                    from = _transition.RequesterID;
            }
            else
            {
                to = _transition.RequesterID;
                comments = _transition.ApproverComments;
                if (_transition.ApproverID != _transition.RequesterID)
                    from = _transition.ApproverID;
            }

            if (aFNotifications == null)
                aFNotifications = new List<AFNotification>();

            aFNotifications.Add(new AFNotification
            {
                From = from,
                To = to,
                Comments = comments,
                DecisionType = _transition.ApproverDecision,
                UsersToCC = usersToCC,
            });

            return this;
        }

        public void ClearNotifications()
        {
            if (aFNotifications != null)
                aFNotifications.Clear();
        }

        private void setDecision(DecisionType decision, string comments)
        {
            _transition = _approvalFlow.Transitions.Where(t => t.ApproverID == _user).FirstOrDefault();
            if (_transition != null)
            {
                _transition.ApproverDecision = decision;
                _transition.AcknowledgementDate = decision == DecisionType.AwaitingDecision ? null : (DateTime?)DateTime.Now;
                _transition.ApproverComments = comments;
                editTransition((Transition)_transition);
            }
        }

        private void createTransition(string requester, string approver, string comments)
        {
            Transition transision = new Transition
            {
                Order = !_approvalFlow.Transitions.Any() ? 1 : _approvalFlow.Transitions.Max(a => a.Order) + 1,
                RequesterID = requester,
                RequestedDate = DateTime.Now,
                ApproverID = approver,
                ApproverDecision = DecisionType.AwaitingDecision,
                RequesterComments = comments
            };

            if (!_approvalFlow.Transitions.Any(t => t.ApproverID == approver))
            {
                _approvalFlow.AddTransition(transision);
            }

            if (_approvalFlow.Transitions.Any(t => t.ApproverID == approver && t.ApproverDecision == DecisionType.Invalidated))
            {
                _user = approver;
                setDecision(DecisionType.AwaitingDecision, comments);
            }
        }

        private void editTransition(Transition transition)
        {
            var t = _approvalFlow.Transitions.FirstOrDefault(tr => tr.TransitionID == transition.TransitionID);

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
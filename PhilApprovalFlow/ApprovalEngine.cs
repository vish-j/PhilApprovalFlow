using PhilApprovalFlow.Classes;
using PhilApprovalFlow.Enum;
using System;
using System.Linq;

namespace PhilApprovalFlow
{
    internal class ApprovalEngine<T> : ICanSetUser, ICanAction where T : ITransition
    {
        private IApprovalFlow<T> _approvalFlow;
        private string _user;
        private ITransition _transition;

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
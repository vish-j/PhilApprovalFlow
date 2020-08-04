using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface ICanAction
    {
        ICanAction RequestApproval(string approver);

        ICanAction RequestApproval(string approver, string comments);

        ICanAction Approve(string comments = null);

        ICanAction Reject(string comments = null);

        ICanAction Invalidate(string username, string Comments = null);
    }
}
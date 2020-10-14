using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface ICanAction
    {
        ICanAction SetMetadata(string key, string value);

        string GetMetadata(string key);

        ICanAction RequestApproval(string approver, string role);

        ICanAction RequestApproval(string approver, string role, string comments);

        ICanAction CheckIn();

        ICanAction Approve(string comments = null);

        ICanAction Reject(string comments = null);

        ICanAction Invalidate(string username, string Comments = null);

        ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null);

        void SetEntityMetaData();

        IEnumerable<IPAFNotification> GetPAFNotifications();

        void ClearNotifications();
    }
}
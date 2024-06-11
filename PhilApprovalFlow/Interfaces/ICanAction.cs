using PhilApprovalFlow.Interfaces;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface ICanAction
    {
        /// <summary>
        /// Add info to Metadata dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns>ICanAction</returns>
        ICanAction SetMetadata(string key, string value);

        /// <summary>
        /// Retrieve info from Metadata dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <returns>string</returns>
        string GetMetadata(string key);

        /// <summary>
        /// Add Approver with their role into workflow
        /// </summary>
        /// <param name="approver"></param>
        /// <param name="role"></param>
        /// <returns>ICanAction</returns>
        ICanAction RequestApproval(string approver, string role);

        /// <summary>
        /// Add Approver with their role into workflow.
        /// </summary>
        /// <param name="approver"></param>
        /// <param name="role"></param>
        /// <param name="comments">Requester Comments</param>
        /// <returns>ICanAction</returns>
        ICanAction RequestApproval(string approver, string role, string comments);

        /// <summary>
        /// Add Approver Group with their role into workflow.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="role"></param>
        /// <returns>ICanAction</returns>
        ICanAction RequestApproval(IPAFApproverGroup group, string role);

        /// <summary>
        /// Add Approver Group with their role into workflow.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="role"></param>
        /// <param name="comments">Requester Comments</param>
        /// <returns>ICanAction</returns>
        ICanAction RequestApproval(IPAFApproverGroup group, string role, string comments);

        /// <summary>
        /// Records DateTime when User Checks In 
        /// </summary>
        /// <returns></returns>
        ICanAction CheckIn();

        /// <summary>
        /// Approves Transition of the user in context
        /// </summary>
        /// <param name="comments"></param>
        /// <returns>ICanAction</returns>
        ICanAction Approve(string comments = null);

        /// <summary>
        /// Rejects Transition of the user in context
        /// </summary>
        /// <param name="comments"></param>
        /// <returns></returns>
        ICanAction Reject(string comments = null);

        /// <summary>
        /// Invalidates Transition of the approver
        /// </summary>
        /// <param name="approver">Username of Approver</param>
        /// <param name="comments"></param>
        /// <returns>ICanAction</returns>
        ICanAction Invalidate(string approver, string comments = null);

        /// <summary>
        /// Loads the Notification into a List.
        /// </summary>
        /// <param name="approver"></param>
        /// <param name="usersToCC">CC to all usernames in array</param>
        /// <param name="mailsToCC">CC to all email addresses in array</param>
        /// <returns></returns>S
        ICanAction LoadNotification(string approver, string[] usersToCC = null, string[] mailsToCC = null);

        /// <summary>
        /// Loads the Notification into a List.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="usersToCC">CC to all usernames in array</param>
        /// <param name="mailsToCC">CC to all email addresses in array</param>
        /// <returns></returns>S
        ICanAction LoadNotification(IPAFApproverGroup group, string[] usersToCC = null, string[] mailsToCC = null);

        /// <summary>
        /// Set Information from entity into Metadata Dictionary
        /// </summary>
        void SetEntityMetaData();

        /// <summary>
        /// Retrieve Notifications from PAF Notifications List
        /// </summary>
        /// <returns>Enumerable of IPAFNotification</returns>
        IEnumerable<IPAFNotification> GetPAFNotifications();

        /// <summary>
        /// Clear all notifications from the List
        /// </summary>
        void ClearNotifications();
    }
}
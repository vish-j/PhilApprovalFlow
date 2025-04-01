using PhilApprovalFlow.Enum;

namespace PhilApprovalFlow
{
    public interface IPAFNotification
    {
        string From { get; set; }
        string To { get; set; }
        string Comments { get; set; }
        DecisionType DecisionType { get; set; }
        long? GroupID { get; set; }
        string[] UsersToCC { get; set; }

        string[] MailsToCC { get; set; }
    }
}
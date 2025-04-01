using PhilApprovalFlow.Enum;

namespace PhilApprovalFlow
{
    public class PAFNotification : IPAFNotification
    {
        public string From { get; set; }
        public string To { get; set; }
        public string Comments { get; set; }
        public DecisionType DecisionType { get; set; }
        public long? GroupID { get; set; }
        public string[] UsersToCC { get; set; }
        public string[] MailsToCC { get; set; }
    }
}
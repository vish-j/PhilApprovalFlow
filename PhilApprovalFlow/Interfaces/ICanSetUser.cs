namespace PhilApprovalFlow
{
    public interface ICanSetUser
    {
        ICanAction SetUserName(string username);

        void ResetTransitions(string comments = null);
    }
}
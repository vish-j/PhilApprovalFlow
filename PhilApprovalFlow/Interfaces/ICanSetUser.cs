namespace PhilApprovalFlow
{
    public interface ICanSetUser
    {        
        /// <summary>
        /// Set User to establish user context
        /// </summary>
        /// <param name="username"></param>
        /// <returns>ICanAction</returns>
        ICanAction SetUserName(string username);

        /// <summary>
        /// Reset Approvals for every transistion to Awaiting Decision
        /// </summary>
        /// <param name="comments"></param>
        void ResetTransitions(string comments = null);
    }
}
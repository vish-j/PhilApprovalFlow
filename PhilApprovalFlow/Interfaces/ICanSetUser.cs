namespace PhilApprovalFlow
{
    public interface ICanSetUser
    {
        /// <summary>
        /// Sets the username context for the approval flow.
        /// </summary>
        /// <param name="username">The username of the current user.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">Thrown if the username is null or whitespace.</exception>
        ICanAction SetUserName(string username);

        /// <summary>
        /// Resets all transitions to a default "Awaiting Decision" state with optional comments.
        /// </summary>
        /// <param name="comments">Optional comments to associate with the reset transitions.</param>
        void ResetTransitions(string comments = null);
    }
}
using System;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Defines operations to set the user context for an approval flow.
    /// This interface is the entry point for interacting with an approval flow
    /// and must be used before performing any actions on the workflow.
    /// </summary>
    public interface ICanSetUser
    {
        /// <summary>
        /// Sets the username context for the approval flow.
        /// </summary>
        /// <param name="username">The username of the current user who will perform subsequent actions.</param>
        /// <returns>An instance of <see cref="ICanAction"/> to chain additional actions.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="username"/> is null, empty, or contains only whitespace.
        /// The exception message will be: "Username cannot be null or whitespace".
        /// </exception>
        /// <example>
        /// <code>
        /// var workflow = entity.GetApprovalFlow().SetUserName("john.doe@example.com");
        /// </code>
        /// </example>
        ICanAction SetUserName(string username);

        /// <summary>
        /// Resets all transitions to a default "Awaiting Decision" state with optional comments.
        /// </summary>
        /// <param name="comments">
        /// Optional comments to associate with the reset transitions explaining why they were reset.
        /// This value can be null.
        /// </param>
        /// <returns>This instance to allow method chaining.</returns>
        /// <remarks>
        /// This method clears any previous decisions (approvals, rejections, etc.) on all transitions
        /// and sets them back to the initial awaiting state. This is useful when a workflow needs
        /// to be restarted due to changes in the underlying entity or process requirements.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Reset all transitions with a comment
        /// workflow.ResetTransitions("Process restarted due to requirement changes");
        /// 
        /// // Reset all transitions without a comment
        /// workflow.ResetTransitions();
        /// </code>
        /// </example>
        ICanAction ResetTransitions(string comments = null);
    }
}
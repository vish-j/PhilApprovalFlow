using System;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Provides extension methods to attach approval workflow functionality to entities.
    /// This is the main entry point for using the PhilApprovalFlow framework.
    /// </summary>
    public static class Flow
    {
        /// <summary>
        /// Attaches the PhilApprovalFlow engine to an entity.
        /// </summary>
        /// <typeparam name="T">The type of transition used in the approval flow, which must implement <see cref="IPAFTransition"/> and have a parameterless constructor.</typeparam>
        /// <param name="f">The entity to attach the approval flow to.</param>
        /// <returns>An instance of <see cref="ICanSetUser"/> that can be used to set the user context and perform actions on the workflow.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="f"/> is null.</exception>
        /// <example>
        /// <code>
        /// // Create an entity that implements IApprovalFlow&lt;YourTransitionType&gt;
        /// var entity = new Invoice();
        /// 
        /// // Attach the approval flow engine
        /// var workflow = entity.GetApprovalFlow();
        /// 
        /// // Set the user context and request approval
        /// workflow.SetUserName("john.doe@example.com")
        ///         .RequestApproval("jane.smith@example.com", "Manager", "Please review this invoice");
        /// </code>
        /// </example>
        public static ICanSetUser GetApprovalFlow<T>(this IApprovalFlow<T> f) where T : IPAFTransition, new()
        {
            return PhilApprovalFlowEngine<T>.SetEntity(ref f);
        }
    }
}
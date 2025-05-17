using System.Collections.Generic;

namespace PhilApprovalFlow
{
    /// <summary>
    /// Defines an entity that can have an approval workflow attached.
    /// </summary>
    /// <typeparam name="T">The type of transition used in the approval flow, which must implement <see cref="IPAFTransition"/>.</typeparam>
    public interface IApprovalFlow<T> where T : IPAFTransition
    {
        /// <summary>
        /// Gets or sets the collection of transitions representing the approval workflow history.
        /// </summary>
        /// <remarks>
        /// This collection contains all approval transitions, including pending, approved, rejected, and invalidated transitions.
        /// The transitions are typically ordered by their <see cref="IPAFTransition.Order"/> property.
        /// </remarks>
        ICollection<T> Transitions { get; set; }

        /// <summary>
        /// Gets the primary identifier of the entity.
        /// </summary>
        /// <returns>The primary key or unique identifier of the entity.</returns>
        /// <remarks>
        /// This value is used to identify the entity in notifications and audit logs.
        /// The return value should be the entity's primary key or a unique identifier.
        /// </remarks>
        object GetID();

        /// <summary>
        /// Gets a short description of the entity suitable for notifications and summary displays.
        /// </summary>
        /// <returns>A short string describing the entity (typically under 100 characters).</returns>
        /// <remarks>
        /// This description will be used in email notifications, task lists, and other compact displays.
        /// It should be concise but informative enough to identify the entity.
        /// </remarks>
        string GetShortDescription();

        /// <summary>
        /// Gets a detailed description of the entity for comprehensive displays.
        /// </summary>
        /// <returns>A detailed string describing the entity, which may include multiple lines or paragraphs.</returns>
        /// <remarks>
        /// This description will be used in detailed views, audit logs, and comprehensive reports.
        /// It can include more detailed information than <see cref="GetShortDescription"/>.
        /// </remarks>
        string GetLongDescription();
    }
}
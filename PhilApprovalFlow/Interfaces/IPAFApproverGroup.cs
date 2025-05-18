using System.Collections.Generic;

namespace PhilApprovalFlow.Interfaces
{
    /// <summary>
    /// Defines a group of approvers that can collectively approve or reject a transition.
    /// This interface extends <see cref="IEnumerable{T}"/> for <see cref="string"/>, allowing
    /// iteration over the approver IDs in the group.
    /// </summary>
    public interface IPAFApproverGroup : IEnumerable<string>
    {
        /// <summary>
        /// Gets or sets the unique identifier for the approver group.
        /// </summary>
        /// <remarks>
        /// This ID is used to associate transitions with this group and must be unique
        /// across all approver groups in the system.
        /// </remarks>
        long GroupID { get; set; }

        /// <summary>
        /// Determines whether the group is currently active.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the group is active and its members can approve or reject transitions;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// When a group is inactive, its members cannot make decisions on associated transitions.
        /// This can be used to temporarily disable a group without removing its members.
        /// </remarks>
        bool IsActive();
    }
}
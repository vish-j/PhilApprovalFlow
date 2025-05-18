using PhilApprovalFlow.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PhilApprovalFlow.Models
{
    /// <summary>
    /// Represents a group of approvers in an approval flow.
    /// </summary>
    /// <remarks>
    /// This class implements <see cref="IEnumerable{T}"/> for <see cref="string"/>, allowing
    /// iteration over the approver IDs in the group. Use <see cref="GetApprovers"/> to access
    /// the full collection of approver IDs.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Create a new approver group
    /// var group = new PAFApproverGroup { GroupID = 1 };
    /// 
    /// // Add approvers to the group
    /// group.SetApprovers(new[] { "john.doe@example.com", "jane.smith@example.com" });
    /// 
    /// // Activate the group
    /// group.SetActiveStatus(true);
    /// 
    /// // Use the group in an approval workflow
    /// workflow.RequestApproval(group, "Finance Team", "Please review this invoice");
    /// </code>
    /// </example>
    public class PAFApproverGroup : IPAFApproverGroup
    {
        /// <summary>
        /// Gets or sets the unique identifier for the approver group.
        /// </summary>
        /// <remarks>
        /// This ID is used to associate transitions with this group and must be unique
        /// across all approver groups in the system.
        /// </remarks>
        public long GroupID { get; set; }

        // Backing field for approver IDs
        private IEnumerable<string> approverIDs = new List<string>();

        /// <summary>
        /// Sets the approvers for this group.
        /// </summary>
        /// <param name="approvers">A collection of approver IDs that will be members of this group.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="approvers"/> is null.
        /// The exception message will be: "Approvers cannot be null."
        /// </exception>
        /// <remarks>
        /// This method replaces any existing approvers with the specified collection.
        /// The approvers can be usernames, email addresses, or any other string identifiers
        /// used to identify users in your system.
        /// </remarks>
        public void SetApprovers(IEnumerable<string> approvers)
        {
            approverIDs = approvers ?? throw new ArgumentNullException(nameof(approvers), "Approvers cannot be null.");
        }

        /// <summary>
        /// Retrieves the list of approver IDs in this group.
        /// </summary>
        /// <returns>An enumerable collection of approver IDs.</returns>
        /// <remarks>
        /// This method returns a reference to the internal collection, so changes to the
        /// returned collection will not affect the group. Use <see cref="SetApprovers"/> to
        /// modify the approvers in the group.
        /// </remarks>
        public IEnumerable<string> GetApprovers()
        {
            return approverIDs;
        }

        // Tracks whether the group is active
        private bool isActive;

        /// <summary>
        /// Checks if the group is currently active.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the group is active and its members can approve or reject transitions;
        /// otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// When a group is inactive, its members cannot make decisions on associated transitions.
        /// This can be used to temporarily disable a group without removing its members.
        /// </remarks>
        public bool IsActive()
        {
            return isActive;
        }

        /// <summary>
        /// Sets the active status of the group.
        /// </summary>
        /// <param name="isActive">
        /// <c>true</c> to activate the group, allowing its members to make decisions;
        /// <c>false</c> to deactivate the group, preventing its members from making decisions.
        /// </param>
        /// <remarks>
        /// Use this method to enable or disable the group without removing its members.
        /// When a group is deactivated, its associated transitions remain in the workflow
        /// but cannot be approved or rejected until the group is activated again.
        /// </remarks>
        public void SetActiveStatus(bool isActive)
        {
            this.isActive = isActive;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the approver IDs.
        /// </summary>
        /// <returns>An enumerator for the approver IDs.</returns>
        /// <remarks>
        /// This method allows the group to be used in foreach loops and LINQ queries.
        /// </remarks>
        public IEnumerator<string> GetEnumerator()
        {
            return approverIDs.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the approver IDs.
        /// </summary>
        /// <returns>An enumerator for the approver IDs.</returns>
        /// <remarks>
        /// This method is required to implement <see cref="IEnumerable"/>.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>
        /// <c>true</c> if the specified object is a <see cref="PAFApproverGroup"/> with the same
        /// <see cref="GroupID"/> as this instance; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Two approver groups are considered equal if they have the same <see cref="GroupID"/>.
        /// The approvers in the groups are not considered in the equality comparison.
        /// </remarks>
        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (PAFApproverGroup)obj;
            return GroupID == other.GroupID;
        }

        /// <summary>
        /// Returns a hash code for the current object.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        /// <remarks>
        /// The hash code is based on the <see cref="GroupID"/> property.
        /// </remarks>
        public override int GetHashCode()
        {
            return GroupID.GetHashCode();
        }
    }
}
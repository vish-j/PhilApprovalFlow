using PhilApprovalFlow.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PhilApprovalFlow.Models
{
    /// <summary>
    /// Represents a group of approvers in an approval flow.
    /// </summary>
    public class PAFApproverGroup : IPAFApproverGroup
    {
        /// <summary>
        /// Gets or sets the unique identifier for the approver group.
        /// </summary>
        public long GroupID { get; set; }

        // Backing field for approver IDs
        private IEnumerable<string> approverIDs = new List<string>();

        /// <summary>
        /// Sets the approvers for this group.
        /// </summary>
        /// <param name="approvers">A collection of approver IDs.</param>
        /// <exception cref="ArgumentNullException">Thrown if approvers is null.</exception>
        public void SetApprovers(IEnumerable<string> approvers)
        {
            approverIDs = approvers ?? throw new ArgumentNullException(nameof(approvers), "Approvers cannot be null.");
        }

        /// <summary>
        /// Retrieves the list of approver IDs in this group.
        /// </summary>
        /// <returns>An enumerable collection of approver IDs.</returns>
        public IEnumerable<string> GetApprovers()
        {
            return approverIDs;
        }

        // Tracks whether the group is active
        private bool isActive;

        /// <summary>
        /// Checks if the group is currently active.
        /// </summary>
        /// <returns>True if the group is active; otherwise, false.</returns>
        public bool IsActive()
        {
            return isActive;
        }

        /// <summary>
        /// Sets the active status of the group.
        /// </summary>
        /// <param name="isActive">The active status to set.</param>
        public void SetActiveStatus(bool isActive)
        {
            this.isActive = isActive;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the approver IDs.
        /// </summary>
        /// <returns>An enumerator for the approver IDs.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            return approverIDs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>True if the objects are equal; otherwise, false.</returns>
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
        public override int GetHashCode()
        {
            return GroupID.GetHashCode();
        }
    }
}

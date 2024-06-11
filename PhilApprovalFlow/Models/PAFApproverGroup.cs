using PhilApprovalFlow.Interfaces;
using System.Collections;
using System.Collections.Generic;

namespace PhilApprovalFlow.Models
{
    public class PAFApproverGroup : IPAFApproverGroup
    {
        public long GroupID { get; set; }
        private IEnumerable<string> approverIDs { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return approverIDs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void SetApprovers(IEnumerable<string> approvers)
        {
            approverIDs = approvers;
        }

        public bool IsActive { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            PAFApproverGroup other = (PAFApproverGroup)obj;
            return GroupID == other.GroupID;
        }

        public override int GetHashCode()
        {
            return GroupID.GetHashCode();
        }
    }
}
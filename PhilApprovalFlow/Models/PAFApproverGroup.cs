using PhilApprovalFlow.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PhilApprovalFlow.Models
{
    public class PAFApproverGroup : IPAFGroup
    {
        public IEnumerable<string> ApproverIDs { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            return ApproverIDs.GetEnumerator();
        }

        public bool IsExists(string ID)
        {
            return ApproverIDs.Contains(ID);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
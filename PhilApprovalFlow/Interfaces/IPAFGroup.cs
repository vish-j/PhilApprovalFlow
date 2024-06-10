using System.Collections.Generic;

namespace PhilApprovalFlow.Interfaces
{
    public interface IPAFGroup : IEnumerable<string>
    {
        IEnumerable<string> ApproverIDs { get; set; }
        bool IsExists(string ID);
    }
}
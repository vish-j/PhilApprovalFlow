using System.Collections.Generic;

namespace PhilApprovalFlow.Interfaces
{
    public interface IPAFApproverGroup : IEnumerable<string>
    {
        long GroupID { get; set; }
        bool IsActive();
    }
}
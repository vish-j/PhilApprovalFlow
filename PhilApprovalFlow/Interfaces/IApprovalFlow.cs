using PhilApprovalFlow.Models;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface IApprovalFlow<T>
    {
        ICollection<T> Transitions { get; set; }

        object GetID();

        string GetShortDescription();

        string GetLongDescription();
    }
}
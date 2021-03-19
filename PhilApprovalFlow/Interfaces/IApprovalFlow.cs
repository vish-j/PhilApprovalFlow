using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface IApprovalFlow<T> where T: IPAFTransition
    {
        ICollection<T> Transitions { get; set; }

        object GetID();

        string GetShortDescription();

        string GetLongDescription();
    }
}
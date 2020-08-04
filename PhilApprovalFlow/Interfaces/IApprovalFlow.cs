using PhilApprovalFlow.Classes;
using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface IApprovalFlow<T>
    {
        ICollection<T> Transitions { get; set; }

        void AddTransition(Transition transition);

        object GetID();

        string GetShortDescription();

        string GetLongDescription();
    }
}
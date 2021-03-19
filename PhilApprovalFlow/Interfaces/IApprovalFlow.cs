using System.Collections.Generic;

namespace PhilApprovalFlow
{
    public interface IApprovalFlow<T> where T : IPAFTransition
    {
        /// <summary>Transition Collection containing the workflow</summary>
        ICollection<T> Transitions { get; set; }

        /// <summary>
        /// Get Entity's Primary ID
        /// </summary>
        /// <returns></returns>
        object GetID();

        /// <summary>
        /// Get Short Description of Entity
        /// </summary>
        /// <returns></returns>
        string GetShortDescription();

        /// <summary>
        /// Get Long Description of Entity
        /// </summary>
        /// <returns></returns>
        string GetLongDescription();
    }
}
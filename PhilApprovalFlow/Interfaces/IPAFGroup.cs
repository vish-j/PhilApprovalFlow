using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhilApprovalFlow.Interfaces
{
    public interface IPAFGroup
    {
        bool IsExists(string ID);
    }
}
using System;

namespace PhilApprovalFlow.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PAFMetadataAttribute : Attribute
    {
        public string Key { get; set; }

        public string Value { get; set; }
    }
}
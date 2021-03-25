namespace PhilApprovalFlow.Enum
{
    public enum DecisionType
    {
        /// <summary>Awaiting Decision From Approver</summary>
        AwaitingDecision = 0,

        /// <summary>Action Approved</summary>
        Approved = 1,

        /// <summary>Action Rejected</summary>
        Rejected = 2,

        /// <summary>Approver Invalidated</summary>
        Invalidated = 3,
    }
}
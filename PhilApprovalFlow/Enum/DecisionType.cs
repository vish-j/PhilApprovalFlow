namespace PhilApprovalFlow.Enum
{
    /// <summary>
    /// Defines the possible decision states for a transition in an approval workflow.
    /// </summary>
    public enum DecisionType
    {
        /// <summary>
        /// Indicates that a decision is pending from the approver.
        /// This is the initial state of any transition when it is first created.
        /// </summary>
        AwaitingDecision = 0,

        /// <summary>
        /// Indicates that the approver has approved the transition.
        /// When all non-invalidated transitions are in this state, the workflow is considered fully approved.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// Indicates that the approver has rejected the transition.
        /// A single rejection typically means the entire workflow is considered rejected,
        /// even if other transitions are approved.
        /// </summary>
        Rejected = 2,

        /// <summary>
        /// Indicates that the approver has been invalidated and can no longer make a decision.
        /// This happens when an approver is removed from the workflow or their role changes.
        /// Invalidated transitions are excluded from workflow completion checks.
        /// </summary>
        Invalidated = 3,
    }
}
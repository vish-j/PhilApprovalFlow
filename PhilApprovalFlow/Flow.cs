namespace PhilApprovalFlow
{
    public static class Flow
    {
        public static ICanSetUser GetApprovalFlow<T>(this IApprovalFlow<T> f) where T : ITransition, new()
        {
            return PhilApprovalFlowEngine<T>.SetEntity(ref f);
        }
    }
}
namespace PhilApprovalFlow
{
    public static class Flow
    {
        public static ICanSetUser GetApprovalFlow<T>(this IApprovalFlow<T> f) where T : ITransition
        {
            return ApprovalEngine<T>.SetEntity(ref f);
        }
    }
}
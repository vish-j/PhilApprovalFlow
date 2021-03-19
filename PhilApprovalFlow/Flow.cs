namespace PhilApprovalFlow
{
    public static class Flow
    {
        /// <summary>
        /// Attaching PAF Engine to an entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static ICanSetUser GetApprovalFlow<T>(this IApprovalFlow<T> f) where T : IPAFTransition, new()
        {
            return PhilApprovalFlowEngine<T>.SetEntity(ref f);
        }
    }
}
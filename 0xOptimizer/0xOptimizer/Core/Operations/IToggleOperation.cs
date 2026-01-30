namespace _0xOptimizer.Core.Operations
{
    public interface IToggleOperation : IOperation
    {
        bool DefaultEnabled { get; }
        void Set(IOperationContext ctx, bool enabled);
        bool? IsEnabled();
    }
}
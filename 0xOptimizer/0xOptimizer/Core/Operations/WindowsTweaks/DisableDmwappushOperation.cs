#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableDmwappushOperation : IOperation
    {
        public string Name => "Desativar dmwappushservice (WAP Push)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            ServiceOps.StopAndDisable(context, "dmwappushservice");
        }

        public bool CheckApplied() => true;
    }
}
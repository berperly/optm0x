#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class SetWindowsUpdateManualOperation : IOperation
    {
        public string Name => "Windows Update em Manual (reversível)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            // MANUAL (não disabled)
            ServiceOps.SetManual(context, "wuauserv");
        }

        public bool CheckApplied() => true;
    }
}
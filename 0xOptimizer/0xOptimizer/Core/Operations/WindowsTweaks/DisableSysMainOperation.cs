#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableSysMainOperation : IOperation
    {
        public string Name => "Desativar SysMain (Superfetch)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            ServiceOps.StopAndDisable(context, "SysMain");
        }

        public bool CheckApplied() => true;
    }
}
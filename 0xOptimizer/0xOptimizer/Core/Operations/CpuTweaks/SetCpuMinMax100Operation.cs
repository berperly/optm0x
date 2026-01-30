#nullable enable
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.CpuTweaks
{
    public sealed class SetCpuMinMax100Operation : IOperation
    {
        public string Name => "CPU Min/Max 100% (AC)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            context.Cmd.Run("powercfg /setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMIN 100");
            context.Cmd.Run("powercfg /setacvalueindex SCHEME_CURRENT SUB_PROCESSOR PROCTHROTTLEMAX 100");
            context.Cmd.Run("powercfg /S SCHEME_CURRENT");
        }

        public bool CheckApplied() => true;
    }
}
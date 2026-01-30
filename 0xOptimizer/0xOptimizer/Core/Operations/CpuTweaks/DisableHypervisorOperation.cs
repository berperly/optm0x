#nullable enable
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.CpuTweaks
{
    public sealed class DisableHypervisorOperation : IOperation
    {
        public string Name => "Desativar Hypervisor (bcdedit)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            context.Cmd.Run("bcdedit /set hypervisorlaunchtype off");
        }

        public bool CheckApplied() => true;
    }
}
#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableDiagTrackOperation : IOperation
    {
        public string Name => "Desativar DiagTrack (telemetry)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            ServiceOps.StopAndDisable(context, "DiagTrack");
        }

        public bool CheckApplied() => true; // sc query parsing opcional depois
    }
}
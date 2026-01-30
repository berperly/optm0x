#nullable enable
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableHdcpOperation : IOperation
    {
        public string Name => "Disable HDCP (Informational)";
        public bool RequiresAdmin => false;

        public void Apply(IOperationContext context)
        {
            context.Log.Warn("HDCP is driver-controlled. There is no universal, stable registry toggle for AMD/NVIDIA.");
            context.Log.Info("AMD: Radeon Software -> Settings -> Video -> HDCP Support (toggle)");
            context.Log.Info("NVIDIA: Control Panel settings may vary by driver/GPU/monitor.");
            context.Log.Success("HDCP: informational guidance shown.");
        }

        public bool CheckApplied() => true;
    }
}

#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ToggleBluetoothServiceOperation : IToggleOperation
    {
        public string Name => "Bluetooth Service";
        public bool RequiresAdmin => true;
        public bool DefaultEnabled => false;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // bthserv = Bluetooth Support Service
            if (enabled)
            {
                // Manual é mais seguro que Automatic
                ServiceOps.SetManual(ctx, "bthserv");
                ctx.Log.Success("Bluetooth Service: ENABLED (Manual).");
            }
            else
            {
                ServiceOps.StopAndDisable(ctx, "bthserv");
                ctx.Log.Success("Bluetooth Service: DISABLED.");
            }
        }

        public bool? IsEnabled() => null;
        public bool CheckApplied() => true;
    }
}
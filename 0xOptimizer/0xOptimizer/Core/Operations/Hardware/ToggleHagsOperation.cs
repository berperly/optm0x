#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.Hardware
{
    public sealed class ToggleHagsOperation : IToggleOperation
    {
        public string Name => "HAGS (Hardware Accelerated GPU Scheduling)";
        public bool RequiresAdmin => true;
        public bool DefaultEnabled => true;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // 2=enabled, 1=default, 0=disabled
            RegistryOps.SetDword(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "HwSchMode",
                enabled ? 2 : 0);

            ctx.Log.Success(enabled ? "HAGS: ENABLED." : "HAGS: DISABLED.");
            ctx.Log.Warn("Reboot required for changes to take effect.");
        }

        public bool? IsEnabled()
        {
            var v = RegistryOps.GetDword(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers",
                "HwSchMode");

            if (v == null) return null;
            return v.Value == 2;
        }

        public bool CheckApplied() => true;
    }
}
#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ToggleDriverSearchingOperation : IToggleOperation
    {
        public string Name => "Windows Driver Searching (Windows Update)";
        public bool RequiresAdmin => true;
        public bool DefaultEnabled => false;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // Disable driver searching = set these values to block WU driver search
            RegistryOps.SetDword(Registry.LocalMachine,
                @"SOFTWARE\Policies\Microsoft\Windows\DriverSearching",
                "DontSearchWindowsUpdate", enabled ? 0 : 1);

            RegistryOps.SetDword(Registry.LocalMachine,
                @"SOFTWARE\Policies\Microsoft\Windows\DriverSearching",
                "DriverUpdateWizardWuSearchEnabled", enabled ? 1 : 0);

            ctx.Log.Success(enabled ? "Driver searching: ENABLED." : "Driver searching: DISABLED.");
        }

        public bool? IsEnabled() => null;
        public bool CheckApplied() => true;
    }
}
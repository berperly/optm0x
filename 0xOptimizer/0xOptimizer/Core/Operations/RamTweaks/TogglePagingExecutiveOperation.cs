#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.RamTweaks
{
    public sealed class TogglePagingExecutiveOperation : IToggleOperation
    {
        public string Name => "DisablePagingExecutive";
        public bool RequiresAdmin => true;
        public bool DefaultEnabled => false;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            RegistryOps.SetDword(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "DisablePagingExecutive",
                enabled ? 1 : 0);

            ctx.Log.Success(enabled ? "DisablePagingExecutive: ENABLED." : "DisablePagingExecutive: DISABLED.");
        }

        public bool? IsEnabled()
        {
            var v = RegistryOps.GetDword(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management",
                "DisablePagingExecutive");

            if (v == null) return null;
            return v.Value == 1;
        }

        public bool CheckApplied() => true;
    }
}
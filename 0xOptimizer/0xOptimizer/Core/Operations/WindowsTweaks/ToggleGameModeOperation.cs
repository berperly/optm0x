#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ToggleGameModeOperation : IToggleOperation
    {
        public string Name => "Game Mode";
        public bool RequiresAdmin => false;
        public bool DefaultEnabled => true;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            int v = enabled ? 1 : 0;

            RegistryOps.SetDword(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AllowAutoGameMode", v);
            RegistryOps.SetDword(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", v);

            ctx.Log.Success(enabled ? "Game Mode: ENABLED." : "Game Mode: DISABLED.");
        }

        public bool? IsEnabled()
        {
            var v = RegistryOps.GetDword(Registry.CurrentUser, @"Software\Microsoft\GameBar", "AutoGameModeEnabled");
            if (v == null) return null;
            return v.Value == 1;
        }

        public bool CheckApplied() => true;
    }
}
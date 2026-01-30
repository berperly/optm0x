#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ToggleGameDvrOperation : IToggleOperation
    {
        public string Name => "GameDVR / Background Recording";
        public bool RequiresAdmin => false;
        public bool DefaultEnabled => false;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // Disable = 0
            RegistryOps.SetDword(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", enabled ? 1 : 0);

            // recomendados para “disable”
            if (!enabled)
            {
                RegistryOps.SetDword(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 2);
                RegistryOps.SetDword(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_HonorUserFSEBehaviorMode", 1);
            }

            ctx.Log.Success(enabled ? "GameDVR: ENABLED." : "GameDVR: DISABLED.");
        }

        public bool? IsEnabled()
        {
            var v = RegistryOps.GetDword(Registry.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled");
            if (v == null) return null;
            return v.Value == 1;
        }

        public bool CheckApplied() => true;
    }
}
#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.RamTweaks
{
    public sealed class TogglePrefetcherOperation : IToggleOperation
    {
        public string Name => "Prefetcher";
        public bool RequiresAdmin => true;
        public bool DefaultEnabled => true;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // EnablePrefetcher: 0=off, 3=on (default)
            RegistryOps.SetDword(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
                "EnablePrefetcher",
                enabled ? 3 : 0);

            ctx.Log.Success(enabled ? "Prefetcher: ENABLED." : "Prefetcher: DISABLED.");
        }

        public bool? IsEnabled()
        {
            var v = RegistryOps.GetDword(Registry.LocalMachine,
                @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\PrefetchParameters",
                "EnablePrefetcher");

            if (v == null) return null;
            return v.Value != 0;
        }

        public bool CheckApplied() => true;
    }
}
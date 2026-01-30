#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ToggleSearchIndexingOperation : IToggleOperation
    {
        public string Name => "Search Indexing (Windows Search)";
        public bool RequiresAdmin => true;
        public bool DefaultEnabled => true;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            if (enabled)
            {
                ServiceOps.SetManual(ctx, "WSearch");
                ctx.Log.Success("Search indexing: ENABLED (Manual).");
            }
            else
            {
                ServiceOps.StopAndDisable(ctx, "WSearch");
                ctx.Log.Success("Search indexing: DISABLED.");
            }
        }

        public bool? IsEnabled() => null;
        public bool CheckApplied() => true;
    }
}
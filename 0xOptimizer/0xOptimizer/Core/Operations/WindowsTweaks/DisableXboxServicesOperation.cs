#nullable enable
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableXboxServicesOperation : IOperation
    {
        public string Name => "Disable Xbox Services";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            // Services list (common)
            ServiceOps.StopAndDisable(context, "XblAuthManager");
            ServiceOps.StopAndDisable(context, "XblGameSave");
            ServiceOps.StopAndDisable(context, "XboxNetApiSvc");
            ServiceOps.StopAndDisable(context, "XboxGipSvc");

            context.Log.Success("Xbox services disabled (best-effort).");
        }

        public bool CheckApplied() => true;
    }
}

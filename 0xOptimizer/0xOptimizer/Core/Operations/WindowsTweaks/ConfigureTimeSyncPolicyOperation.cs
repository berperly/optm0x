#nullable enable
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ConfigureTimeSyncPolicyOperation : IOperation
    {
        public string Name => "Configure TimeSync (w32tm)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            context.Log.Info("Configuring Windows Time Service policy (w32tm)...");
            context.Cmd.Run("cmd.exe", "/c w32tm /config /syncfromflags:manual /manualpeerlist:\"time.windows.com\"");
            context.Cmd.Run("cmd.exe", "/c w32tm /config /reliable:yes");
            context.Cmd.Run("cmd.exe", "/c w32tm /config /update");

            context.Log.Success("TimeSync policy applied (best-effort).");
        }

        public bool CheckApplied() => true;
    }
}

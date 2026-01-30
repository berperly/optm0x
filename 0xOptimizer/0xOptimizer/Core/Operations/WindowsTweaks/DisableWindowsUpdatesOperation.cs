#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableWindowsUpdatesOperation : IOperation
    {
        public string Name => "Disable Windows Updates (Service + Policy)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            // Service
            ServiceOps.StopAndDisable(context, "wuauserv");

            // Policy
            const string keyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
            using var k = Registry.LocalMachine.CreateSubKey(keyPath, true);
            k?.SetValue("NoAutoUpdate", 1, RegistryValueKind.DWord);

            context.Log.Warn("Windows Updates disabled (best-effort). This is not recommended for security.");
            context.Log.Success("Policy set: HKLM WindowsUpdate\\AU\\NoAutoUpdate=1");
        }

        public bool CheckApplied()
        {
            // Best-effort: policy present
            try
            {
                const string keyPath = @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU";
                using var k = Registry.LocalMachine.OpenSubKey(keyPath, false);
                var v = k?.GetValue("NoAutoUpdate");
                return v is int i && i == 1;
            }
            catch { return true; }
        }
    }
}

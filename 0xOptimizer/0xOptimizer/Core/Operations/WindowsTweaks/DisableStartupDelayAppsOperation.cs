#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableStartupDelayAppsOperation : IOperation
    {
        public string Name => "Disable Startup Delay for Apps";
        public bool RequiresAdmin => false;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize";
            using var k = Registry.CurrentUser.CreateSubKey(keyPath, true);

            // 0 = no delay (best-effort). (Algumas builds usam StartupDelayInMSec)
            k?.SetValue("StartupDelayInMSec", 0, RegistryValueKind.DWord);

            context.Log.Success("Startup delay disabled (HKCU Serialize\\StartupDelayInMSec=0).");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Serialize";
                using var k = Registry.CurrentUser.OpenSubKey(keyPath, false);
                var v = k?.GetValue("StartupDelayInMSec");
                return v is int i && i == 0;
            }
            catch { return true; }
        }
    }
}

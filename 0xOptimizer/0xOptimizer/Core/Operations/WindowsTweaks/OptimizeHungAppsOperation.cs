#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class OptimizeHungAppsOperation : IOperation
    {
        public string Name => "Optimize Hung Apps (AutoEndTasks + Timeouts)";
        public bool RequiresAdmin => false;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"Control Panel\Desktop";
            using var k = Registry.CurrentUser.CreateSubKey(keyPath, true);

            // strings (REG_SZ)
            k?.SetValue("HungAppTimeout", "5000", RegistryValueKind.String);
            k?.SetValue("WaitToKillAppTimeout", "5000", RegistryValueKind.String);
            k?.SetValue("AutoEndTasks", "1", RegistryValueKind.String);

            context.Log.Success("Hung apps timeouts applied (HKCU Desktop). Sign out may be required.");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"Control Panel\Desktop";
                using var k = Registry.CurrentUser.OpenSubKey(keyPath, false);
                var a = k?.GetValue("AutoEndTasks") as string;
                var h = k?.GetValue("HungAppTimeout") as string;
                var w = k?.GetValue("WaitToKillAppTimeout") as string;

                return a == "1" && h == "5000" && w == "5000";
            }
            catch { return true; }
        }
    }
}

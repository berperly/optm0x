#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class OptimizeWindowsSearchOperation : IOperation
    {
        public string Name => "Optimize Windows Search (Bing/Cortana Off)";
        public bool RequiresAdmin => false;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Search";
            using var k = Registry.CurrentUser.CreateSubKey(keyPath, true);

            k?.SetValue("BingSearchEnabled", 0, RegistryValueKind.DWord);
            k?.SetValue("CortanaConsent", 0, RegistryValueKind.DWord);

            context.Log.Success("Windows Search optimized (BingSearchEnabled=0, CortanaConsent=0).");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Search";
                using var k = Registry.CurrentUser.OpenSubKey(keyPath, false);
                var a = k?.GetValue("BingSearchEnabled");
                var b = k?.GetValue("CortanaConsent");
                return (a is int ai && ai == 0) && (b is int bi && bi == 0);
            }
            catch { return true; }
        }
    }
}

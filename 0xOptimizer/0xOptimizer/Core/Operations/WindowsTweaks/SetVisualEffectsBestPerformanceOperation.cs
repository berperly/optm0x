#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class SetVisualEffectsBestPerformanceOperation : IOperation
    {
        public string Name => "Visual Effects: Best Performance (Preset)";
        public bool RequiresAdmin => false;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects";
            using var k = Registry.CurrentUser.CreateSubKey(keyPath, true);

            // 0 = Let Windows choose
            // 1 = Best appearance
            // 2 = Best performance
            // 3 = Custom
            k?.SetValue("VisualFXSetting", 2, RegistryValueKind.DWord);

            context.Log.Success("Visual effects set to Best Performance (HKCU VisualFXSetting=2). Logoff may be required.");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects";
                using var k = Registry.CurrentUser.OpenSubKey(keyPath, false);
                var v = k?.GetValue("VisualFXSetting");
                return v is int i && i == 2;
            }
            catch { return true; }
        }
    }
}

#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableTransparencyEffectsOperation : IOperation
    {
        public string Name => "Disable Transparency Effects";
        public bool RequiresAdmin => false;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
            using var k = Registry.CurrentUser.CreateSubKey(keyPath, true);

            // 0 = off, 1 = on
            k?.SetValue("EnableTransparency", 0, RegistryValueKind.DWord);

            context.Log.Success("Transparency effects disabled (HKCU Personalize\\EnableTransparency=0).");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
                using var k = Registry.CurrentUser.OpenSubKey(keyPath, false);
                var v = k?.GetValue("EnableTransparency");
                return v is int i && i == 0;
            }
            catch { return true; }
        }
    }
}

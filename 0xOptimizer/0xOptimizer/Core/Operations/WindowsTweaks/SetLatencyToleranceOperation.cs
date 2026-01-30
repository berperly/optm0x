#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class SetLatencyToleranceOperation : IOperation
    {
        public string Name => "Latency Tolerance Tweaks (Multimedia SystemProfile)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
            using var k = Registry.LocalMachine.CreateSubKey(keyPath, true);

            // ffffffff = disable throttling index
            k?.SetValue("NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF), RegistryValueKind.DWord);
            k?.SetValue("SystemResponsiveness", 0, RegistryValueKind.DWord);

            context.Log.Success("Latency tolerance tweaks applied. Reboot recommended.");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile";
                using var k = Registry.LocalMachine.OpenSubKey(keyPath, false);

                var a = k?.GetValue("NetworkThrottlingIndex");
                var b = k?.GetValue("SystemResponsiveness");

                var okA = a is int ai && unchecked((uint)ai) == 0xFFFFFFFF;
                var okB = b is int bi && bi == 0;
                return okA && okB;
            }
            catch { return true; }
        }
    }
}

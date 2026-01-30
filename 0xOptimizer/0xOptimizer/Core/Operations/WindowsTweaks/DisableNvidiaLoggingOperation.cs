#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableNvidiaLoggingOperation : IOperation
    {
        public string Name => "Disable NVIDIA Logging (Registry)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            const string keyPath = @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client";
            using var k = Registry.LocalMachine.CreateSubKey(keyPath, true);

            k?.SetValue("OptInOrOutPreference", 0, RegistryValueKind.DWord);
            k?.SetValue("LogLevel", 0, RegistryValueKind.DWord);

            context.Log.Success("NVIDIA logging preferences set (best-effort).");
        }

        public bool CheckApplied()
        {
            try
            {
                const string keyPath = @"SOFTWARE\NVIDIA Corporation\NvControlPanel2\Client";
                using var k = Registry.LocalMachine.OpenSubKey(keyPath, false);
                var a = k?.GetValue("OptInOrOutPreference");
                var b = k?.GetValue("LogLevel");
                return (a is int ai && ai == 0) && (b is int bi && bi == 0);
            }
            catch { return true; }
        }
    }
}

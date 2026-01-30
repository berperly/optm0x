#nullable enable

using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class EnableHagsOperation : IOperation
    {
        public string Name => "Ativar HAGS (Hardware-accelerated GPU scheduling)";
        public bool RequiresAdmin => true; // HKLM

        public void Apply(IOperationContext ctx)
        {
            ctx.Log.Info("Ativando HAGS (HKLM\\...\\GraphicsDrivers HwSchMode=2)...");

            // 2 = enabled, 1 = default/let Windows decide, 0 = disabled
            Run(ctx, @"reg add ""HKLM\SYSTEM\CurrentControlSet\Control\GraphicsDrivers"" /v HwSchMode /t REG_DWORD /d 2 /f");

            ctx.Log.Success("HAGS: aplicado. (Pode precisar reiniciar o PC)");
        }

        public bool CheckApplied() => true;

        private static void Run(IOperationContext ctx, string cmd)
        {
            var r = ctx.Cmd.Run("cmd.exe", "/c " + cmd);
            if (r.ExitCode != 0 && !string.IsNullOrWhiteSpace(r.Error))
                ctx.Log.Warn($"{cmd} -> {r.Error.Trim()}");
        }
    }
}
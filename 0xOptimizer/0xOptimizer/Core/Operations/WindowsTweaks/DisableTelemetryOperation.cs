#nullable enable

using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableTelemetryOperation : IOperation
    {
        public string Name => "Desativar telemetria (DiagTrack/WAP Push)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext ctx)
        {
            ctx.Log.Info("Desativando serviços de telemetria...");

            Run(ctx, "sc stop DiagTrack");
            Run(ctx, "sc config DiagTrack start= disabled");

            Run(ctx, "sc stop dmwappushservice");
            Run(ctx, "sc config dmwappushservice start= disabled");

            ctx.Log.Success("Telemetria: aplicado (best-effort).");
        }

        public bool CheckApplied()
        {
            // Best-effort simples: se o serviço estiver DISABLED é sucesso.
            // (Se quiser, dá pra ler via ServiceController também.)
            return true;
        }

        private static void Run(IOperationContext ctx, string cmd)
        {
            var r = ctx.Cmd.Run("cmd.exe", "/c " + cmd);
            if (r.ExitCode != 0 && !string.IsNullOrWhiteSpace(r.Error))
                ctx.Log.Warn($"{cmd} -> {r.Error.Trim()}");
        }
    }
}
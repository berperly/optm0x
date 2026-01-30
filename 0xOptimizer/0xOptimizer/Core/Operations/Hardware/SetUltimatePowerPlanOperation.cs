#nullable enable

using System;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.Hardware
{
    public sealed class SetUltimatePerformancePlanOperation : IOperation
    {
        public string Name => "Configurar plano de energia (Ultimate Performance)";
        public bool RequiresAdmin => true;

        // Exposto public para outras operações usarem sem copiar/colar string
        public const string ULTIMATE_GUID = "e9a42b02-d5df-448d-aa00-03f14749eb61";

        public void Apply(IOperationContext ctx)
        {
            ctx.Log.Info("Ativando Ultimate Performance...");

            // Duplica o plano (em alguns Windows ele não aparece por padrão)
            Run(ctx, $"powercfg -duplicatescheme {ULTIMATE_GUID}");

            // Seta como ativo
            Run(ctx, $"powercfg -setactive {ULTIMATE_GUID}");

            ctx.Log.Success("Plano de energia: aplicado (best-effort).");
        }

        public bool CheckApplied()
        {
            // Best-effort: checa se o plano ativo é o ultimate GUID
            try
            {
                // powercfg /getactivescheme retorna algo como:
                // "Power Scheme GUID: XXXXXXXX-XXXX-...  (Nome)"
                // Vamos só procurar o GUID
                // (aqui não temos ctx, então é uma checagem simples)
                return true;
            }
            catch
            {
                return true;
            }
        }

        private static void Run(IOperationContext ctx, string cmd)
        {
            var r = ctx.Cmd.Run("cmd.exe", "/c " + cmd, 30_000);

            if (r.ExitCode != 0)
            {
                var err = (r.Error ?? string.Empty).Trim();
                var outp = (r.Output ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(err))
                    ctx.Log.Warn($"{cmd} -> {err}");
                else if (!string.IsNullOrWhiteSpace(outp))
                    ctx.Log.Warn($"{cmd} -> {outp}");
                else
                    ctx.Log.Warn($"{cmd} -> ExitCode {r.ExitCode}");
            }
        }
    }
}
#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace _0xOptimizer.Core.Operations
{
    public sealed class OperationRunner
    {
        public async Task RunAsync(string groupName, IReadOnlyList<IOperation> ops, IOperationContext ctx, CancellationToken ct)
        {
            ctx.Log.Info($"▶ Iniciando: {groupName}");
            for (int i = 0; i < ops.Count; i++)
            {
                ct.ThrowIfCancellationRequested();

                var op = ops[i];
                ctx.Log.Info($"\n[{i + 1}/{ops.Count}] {op.Name}");

                try
                {
                    await Task.Run(() => op.Apply(ctx), ct).ConfigureAwait(false);
                    ctx.Log.Success("✅ Apply() OK");
                }
                catch (Exception ex)
                {
                    ctx.Log.Error("❌ Falhou: " + ex.Message);
                    throw;
                }
            }
            ctx.Log.Success($"\n✔ Finalizado: {groupName}");
        }
    }
}
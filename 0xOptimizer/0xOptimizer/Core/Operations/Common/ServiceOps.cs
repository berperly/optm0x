#nullable enable
using System;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.Common
{
    public static class ServiceOps
    {
        public static void StopAndDisable(IOperationContext ctx, string service)
        {
            ctx.Cmd.Run($"sc stop \"{service}\"");
            ctx.Cmd.Run($"sc config \"{service}\" start= disabled");
        }

        public static void SetManual(IOperationContext ctx, string service)
        {
            ctx.Cmd.Run($"sc stop \"{service}\"");
            ctx.Cmd.Run($"sc config \"{service}\" start= demand");
        }

        public static string? QueryStartType(IOperationContext ctx, string service)
        {
            // best-effort parsing
            var output = ctx.Cmd.RunCapture($"sc qc \"{service}\"");
            return output;
        }
    }
}
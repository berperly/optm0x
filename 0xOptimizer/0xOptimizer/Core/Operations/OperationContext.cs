#nullable enable

using _0xOptimizer.Core.Logging;
using _0xOptimizer.Core.Utils;

namespace _0xOptimizer.Core.Operations
{
    public sealed class OperationContext : IOperationContext
    {
        public ILogSink Log { get; }
        public CommandRunner Cmd { get; }

        public OperationContext(ILogSink log, CommandRunner cmd)
        {
            Log = log;
            Cmd = cmd;
        }
    }
}
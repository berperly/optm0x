#nullable enable

using _0xOptimizer.Core.Logging;
using _0xOptimizer.Core.Utils;

namespace _0xOptimizer.Core.Operations
{
    public interface IOperationContext
    {
        ILogSink Log { get; }
        CommandRunner Cmd { get; }
    }
}
#nullable enable
using System.Collections.Generic;
using _0xOptimizer.Core.Operations.Cleanup;

namespace _0xOptimizer.Core.Operations
{
    public static class CleanupOperations
    {
        public static IReadOnlyList<IOperation> All { get; } = new IOperation[]
        {
            new CleanTempOperation(),
            new CleanPrefetchOperation(),
        };
    }
}
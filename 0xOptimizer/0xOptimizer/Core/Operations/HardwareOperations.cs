#nullable enable
using System.Collections.Generic;
using _0xOptimizer.Core.Operations.Hardware;

namespace _0xOptimizer.Core.Operations
{
    public static class HardwareOperations
    {
        public static IReadOnlyList<IOperation> All { get; } = new IOperation[]
        {
            new SetUltimatePerformancePlanOperation(),
        };
    }
}
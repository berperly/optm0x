#nullable enable
using System.Collections.Generic;
using _0xOptimizer.Core.Operations.Cleanup;
using _0xOptimizer.Core.Operations.Hardware;
using _0xOptimizer.Core.Operations.RamTweaks;
using _0xOptimizer.Core.Operations.WindowsTweaks;
using _0xOptimizer.Core.Operations.CpuTweaks;

namespace _0xOptimizer.Core.Operations
{
    public static class OperationCatalog
    {
        public static IReadOnlyList<IOperation> SystemCleanup { get; } = new IOperation[]
        {
            new CleanTempOperation(),
            new CleanPrefetchOperation(),
            new ToggleNotificationsOperation(),
        };

        public static IReadOnlyList<IOperation> HardwareOptimization { get; } = new IOperation[]
        {
            new SetUltimatePerformancePlanOperation(),
            new ToggleHagsOperation(),
            new ToggleBluetoothServiceOperation(),
            new ToggleSearchIndexingOperation(),
            new ToggleGameModeOperation(),
            new ToggleGameDvrOperation(),
        };

        public static IReadOnlyList<IOperation> RamOptimization { get; } = new IOperation[]
        {
            new TogglePrefetcherOperation(),
            new TogglePagingExecutiveOperation(),
            new ToggleAnimationsOperation(),
        };

        public static IReadOnlyList<IOperation> CpuOptimization { get; } = new IOperation[]
        {
            new DisableHypervisorOperation(),
            new SetCpuMinMax100Operation(),
        };
    }
}
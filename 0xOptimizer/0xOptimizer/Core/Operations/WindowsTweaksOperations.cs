//
// File: 0xOptimizer\Core\Operations\WindowsTweaksOperations.cs
//
#nullable enable

using System;
using System.Collections.Generic;

namespace _0xOptimizer.Core.Operations
{
    /// <summary>
    /// Registry/Service tweaks are being triggered directly from the UI via LambdaOperation.
    /// Keep this list empty (or only add operations that реально existem no Core).
    /// </summary>
    public static class WindowsTweaksOperations
    {
        public static IReadOnlyList<IOperation> All => Array.Empty<IOperation>();
    }
}
#nullable enable
using System;
using System.IO;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.Cleanup
{
    public sealed class CleanPrefetchOperation : IOperation
    {
        public string Name => "Limpar Prefetch";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            var path = Environment.ExpandEnvironmentVariables(@"%WINDIR%\Prefetch");
            if (!Directory.Exists(path)) return;

            foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.TopDirectoryOnly))
            {
                try { File.Delete(f); } catch { }
            }
        }

        public bool CheckApplied() => true;
    }
}
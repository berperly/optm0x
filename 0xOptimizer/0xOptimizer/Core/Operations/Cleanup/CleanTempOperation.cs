#nullable enable
using System;
using System.IO;
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.Cleanup
{
    public sealed class CleanTempOperation : IOperation
    {
        public string Name => "Limpar TEMP (%temp% + Windows Temp)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            void Wipe(string path)
            {
                if (!Directory.Exists(path)) return;
                foreach (var f in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                {
                    try { File.Delete(f); } catch { }
                }
                foreach (var d in Directory.EnumerateDirectories(path, "*", SearchOption.AllDirectories))
                {
                    try { Directory.Delete(d, true); } catch { }
                }
            }

            Wipe(Path.GetTempPath());
            Wipe(Environment.ExpandEnvironmentVariables(@"%WINDIR%\Temp"));
        }

        public bool CheckApplied() => true;
    }
}
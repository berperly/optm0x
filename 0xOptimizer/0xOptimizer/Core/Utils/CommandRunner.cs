using System;
using System.Diagnostics;

namespace _0xOptimizer.Core.Utils
{
    public sealed class CommandRunner
    {
        public (int ExitCode, string Output, string Error) Run(string file, string args, int timeoutMs)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = file,
                    Arguments = args,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                using var p = Process.Start(psi);
                if (p == null) return (-1, "", "Falha ao iniciar processo");

                var output = p.StandardOutput.ReadToEnd();
                var error = p.StandardError.ReadToEnd();

                if (!p.WaitForExit(timeoutMs))
                {
                    try { p.Kill(entireProcessTree: true); } catch { }
                    return (-2, output, "Timeout");
                }

                return (p.ExitCode, output, error);
            }
            catch (Exception ex)
            {
                return (-1, "", ex.Message);
            }
        }

        // overload padrão
        public (int ExitCode, string Output, string Error) Run(string file, string args)
            => Run(file, args, 20_000);

        // ✅ novo: roda uma linha inteira no cmd.exe
        public (int ExitCode, string Output, string Error) Run(string cmdLine, int timeoutMs = 20_000)
            => Run("cmd.exe", "/c " + cmdLine, timeoutMs);

        // ✅ novo: captura output (pra “qc”, “query”, etc.)
        public string RunCapture(string cmdLine, int timeoutMs = 20_000)
        {
            var r = Run(cmdLine, timeoutMs);
            return (r.Output ?? "") + (string.IsNullOrWhiteSpace(r.Error) ? "" : ("\n" + r.Error));
        }

        // ✅ novo: helper pra sucesso/erro (pra logs)
        public void RunOrThrow(string cmdLine, int timeoutMs = 20_000)
        {
            var r = Run(cmdLine, timeoutMs);
            if (r.ExitCode != 0)
                throw new InvalidOperationException($"Falhou ({r.ExitCode}): {cmdLine}\n{r.Output}\n{r.Error}");
        }
    }
}
#nullable enable
using _0xOptimizer.Core.Operations;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class DisableScheduledTasksOperation : IOperation
    {
        public string Name => "Disable Scheduled Tasks (Telemetry/Maintenance)";
        public bool RequiresAdmin => true;

        public void Apply(IOperationContext context)
        {
            context.Log.Info("Disabling scheduled tasks (best-effort)...");
            var ps =
                "-NoProfile -ExecutionPolicy Bypass -Command " +
                "\"$paths=@(" +
                "'\\\\Microsoft\\\\Windows\\\\Application Experience\\\\'," +
                "'\\\\Microsoft\\\\Windows\\\\Customer Experience Improvement Program\\\\'," +
                "'\\\\Microsoft\\\\Windows\\\\DiskDiagnostic\\\\'" +
                ");" +
                "foreach($p in $paths){" +
                "Get-ScheduledTask -TaskPath $p -ErrorAction SilentlyContinue | Disable-ScheduledTask -ErrorAction SilentlyContinue | Out-Null" +
                "}\"";

            context.Cmd.Run("powershell.exe", ps);
            context.Log.Success("Scheduled tasks disabled (best-effort).");
        }

        public bool CheckApplied() => true;
    }
}

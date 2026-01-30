#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.WindowsTweaks
{
    public sealed class ToggleNotificationsOperation : IToggleOperation
    {
        public string Name => "Notifications";
        public bool RequiresAdmin => false;
        public bool DefaultEnabled => false;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // 0 = off (Disable), 1 = on (Enable)
            // Aqui vamos fazer o "Disable all notifications"
            // ToastEnabled é a forma padrão limpa.
            RegistryOps.SetDword(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\PushNotifications",
                "ToastEnabled",
                enabled ? 1 : 0);

            ctx.Log.Success(enabled ? "Notifications: ENABLED." : "Notifications: DISABLED.");
        }

        public bool? IsEnabled()
        {
            var v = RegistryOps.GetDword(Registry.CurrentUser,
                @"Software\Microsoft\Windows\CurrentVersion\PushNotifications",
                "ToastEnabled");

            if (v == null) return null;
            return v.Value == 1;
        }

        public bool CheckApplied() => true;
    }
}
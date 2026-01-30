#nullable enable
using Microsoft.Win32;
using _0xOptimizer.Core.Operations;
using _0xOptimizer.Core.Operations.Common;

namespace _0xOptimizer.Core.Operations.RamTweaks
{
    public sealed class ToggleAnimationsOperation : IToggleOperation
    {
        public string Name => "Windows UI Animations";
        public bool RequiresAdmin => false;
        public bool DefaultEnabled => false;

        public void Apply(IOperationContext ctx) => Set(ctx, true);

        public void Set(IOperationContext ctx, bool enabled)
        {
            // Melhor abordagem: UserPreferencesMask é delicado,
            // então vamos aplicar só o que é seguro e perceptível.
            // MenuShowDelay -> 0 para responsividade
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "MenuShowDelay", enabled ? "400" : "0");

            // Outra flag comum: "MinAnimate"
            Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", enabled ? "1" : "0");

            ctx.Log.Success(enabled ? "Animations: ENABLED." : "Animations: DISABLED.");
            ctx.Log.Warn("Some UI changes require sign-out or reboot.");
        }

        public bool? IsEnabled()
        {
            var v = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop\WindowMetrics", "MinAnimate", null);
            if (v is not string s) return null;
            return s == "1";
        }

        public bool CheckApplied() => true;
    }
}
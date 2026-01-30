#nullable enable

using System;
using System.Security.Principal;
using System.Windows.Forms;
using _0xOptimizer.UI.Forms;

namespace _0xOptimizer
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Win10 22H2: garante que vai rodar como admin (necessário pros tweaks)
            if (!IsAdmin())
            {
                var r = MessageBox.Show(
                    "Este programa precisa ser executado como Administrador para aplicar os tweaks.\n\n" +
                    "Deseja reiniciar como Administrador agora?",
                    "0xOptimizer - Permissão",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (r == DialogResult.Yes)
                {
                    RelaunchAsAdmin();
                }

                return;
            }

            Application.Run(new MainOptimizerForm());
        }

        private static bool IsAdmin()
        {
            try
            {
                using var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private static void RelaunchAsAdmin()
        {
            try
            {
                var exe = Application.ExecutablePath;

                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = exe,
                    UseShellExecute = true,
                    Verb = "runas" // UAC prompt
                };

                System.Diagnostics.Process.Start(psi);
            }
            catch
            {
                // usuário cancelou o UAC ou falhou
                MessageBox.Show(
                    "Não foi possível iniciar como Administrador (UAC cancelado ou falhou).",
                    "0xOptimizer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
    }
}
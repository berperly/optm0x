using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace _0xOptimizer
{
    public static class WindowChrome
    {
        public static void EnableRoundedCorners(Form form)
        {
            try
            {
                // DWMWA_WINDOW_CORNER_PREFERENCE = 33
                // DWMWCP_ROUND = 2
                int attr = 33;
                int pref = 2;
                DwmSetWindowAttribute(form.Handle, attr, ref pref, sizeof(int));
            }
            catch { }
        }

        public static void EnableDrag(Control dragHandle, Form form)
        {
            dragHandle.MouseDown += (_, e) =>
            {
                if (e.Button != MouseButtons.Left) return;
                ReleaseCapture();
                SendMessage(form.Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            };
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    }
}
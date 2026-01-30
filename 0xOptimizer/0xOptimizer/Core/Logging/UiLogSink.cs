using System;
using System.Drawing;
using System.Windows.Forms;

namespace _0xOptimizer.Core.Logging
{
    public sealed class UiLogSink : ILogSink
    {
        private readonly RichTextBox _box;

        public UiLogSink(RichTextBox box) => _box = box;

        public void Info(string msg) => Append(msg, Color.FromArgb(220, 220, 230));
        public void Success(string msg) => Append(msg, Color.FromArgb(0, 200, 83));
        public void Warn(string msg) => Append(msg, Color.FromArgb(255, 193, 7));
        public void Error(string msg) => Append(msg, Color.FromArgb(255, 100, 100));

        private void Append(string msg, Color c)
        {
            if (_box.IsDisposed) return;

            void Do()
            {
                _box.SelectionColor = c;
                _box.AppendText(msg + Environment.NewLine);
                _box.ScrollToCaret();
            }

            if (_box.InvokeRequired) _box.BeginInvoke(new Action(Do));
            else Do();
        }
    }
}
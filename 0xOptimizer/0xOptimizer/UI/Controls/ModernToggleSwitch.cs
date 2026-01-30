//
// File: 0xOptimizer\UI\Controls\ModernToggleSwitch.cs
//
#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace _0xOptimizer.UI.Controls
{
    public sealed class ModernToggleSwitch : UserControl
    {
        private bool _checked;

        public event EventHandler? CheckedChanged;

        [DefaultValue(false)]
        public bool Checked
        {
            get => _checked;
            set
            {
                if (_checked == value) return;
                _checked = value;
                Invalidate();
                CheckedChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        // ===== Appearance (Designer-friendly) =====

        [Category("Modern")]
        [Browsable(true)]
        [DefaultValue(typeof(Color), "0, 200, 83")]
        public Color OnBackColor { get; set; } = Color.FromArgb(0, 200, 83);

        [Category("Modern")]
        [Browsable(true)]
        [DefaultValue(typeof(Color), "70, 70, 90")]
        public Color OffBackColor { get; set; } = Color.FromArgb(70, 70, 90);

        [Category("Modern")]
        [Browsable(true)]
        [DefaultValue(typeof(Color), "255, 255, 255")]
        public Color ThumbColor { get; set; } = Color.White;

        [Category("Modern")]
        [Browsable(true)]
        [DefaultValue(typeof(Color), "90, 90, 120")]
        public Color BorderColor { get; set; } = Color.FromArgb(90, 90, 120);

        [Category("Modern")]
        [Browsable(true)]
        [DefaultValue(14)]
        public int CornerRadius { get; set; } = 14;

        // ---- Designer serialization helpers (silence WFO1000) ----
        public bool ShouldSerializeOnBackColor() => OnBackColor != Color.FromArgb(0, 200, 83);
        public bool ShouldSerializeOffBackColor() => OffBackColor != Color.FromArgb(70, 70, 90);
        public bool ShouldSerializeThumbColor() => ThumbColor != Color.White;
        public bool ShouldSerializeBorderColor() => BorderColor != Color.FromArgb(90, 90, 120);
        public bool ShouldSerializeCornerRadius() => CornerRadius != 14;

        public ModernToggleSwitch()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            Cursor = Cursors.Hand;
            Size = new Size(54, 28);
            MinimumSize = new Size(40, 22);
            MaximumSize = new Size(80, 40);

            TabStop = true;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            Checked = !Checked;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                Checked = !Checked;
                e.Handled = true;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            // avoid GDI+ weirdness on tiny sizes
            if (Width < 2 || Height < 2) return;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var radius = Math.Max(6, Math.Min(CornerRadius, Height / 2));

            // track
            using (var trackBrush = new SolidBrush(Checked ? OnBackColor : OffBackColor))
                FillRoundedRect(e.Graphics, rect, radius, trackBrush);

            // border
            using (var pen = new Pen(BorderColor, 1))
                DrawRoundedRect(e.Graphics, rect, radius, pen);

            // thumb
            int padding = Math.Max(2, Height / 10);
            int thumbSize = Math.Max(2, Height - (padding * 2));
            int thumbX = Checked ? Width - padding - thumbSize : padding;

            var thumbRect = new Rectangle(thumbX, padding, thumbSize, thumbSize);
            using (var thumbBrush = new SolidBrush(ThumbColor))
                e.Graphics.FillEllipse(thumbBrush, thumbRect);

            // focus
            if (Focused)
            {
                var focusRect = new Rectangle(2, 2, Math.Max(1, Width - 5), Math.Max(1, Height - 5));
                using var focusPen = new Pen(Color.FromArgb(180, 255, 255, 255), 1)
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dot
                };
                DrawRoundedRect(e.Graphics, focusRect, Math.Max(4, radius - 2), focusPen);
            }
        }

        private static void FillRoundedRect(Graphics g, Rectangle r, int radius, Brush b)
        {
            using var path = RoundedRectPath(r, radius);
            g.FillPath(b, path);
        }

        private static void DrawRoundedRect(Graphics g, Rectangle r, int radius, Pen p)
        {
            using var path = RoundedRectPath(r, radius);
            g.DrawPath(p, path);
        }

        private static System.Drawing.Drawing2D.GraphicsPath RoundedRectPath(Rectangle r, int radius)
        {
            radius = Math.Max(1, radius);

            int d = radius * 2;
            d = Math.Min(d, Math.Min(r.Width, r.Height)); // clamp

            var path = new System.Drawing.Drawing2D.GraphicsPath();

            if (d <= 1 || r.Width <= 1 || r.Height <= 1)
            {
                path.AddRectangle(r);
                path.CloseFigure();
                return path;
            }

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
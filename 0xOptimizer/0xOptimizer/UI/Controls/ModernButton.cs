//
// File: 0xOptimizer\UI\Controls\ModernButton.cs
//
#nullable enable

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace _0xOptimizer.UI.Controls
{
    [DefaultEvent(nameof(Click))]
    public sealed class ModernButton : Button
    {
        private bool _hover;
        private bool _pressed;

        public ModernButton()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint |
                ControlStyles.SupportsTransparentBackColor,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;

            BackColor = Color.Transparent;
            ForeColor = Color.White;

            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            Cursor = Cursors.Hand;

            Size = new Size(140, 40);
            Padding = new Padding(14, 0, 14, 0);
            TextAlign = ContentAlignment.MiddleCenter;
            UseMnemonic = false;
            TabStop = true;
        }

        // ===== Visual props (Designer-friendly) =====

        [Category("Modern")]
        [DefaultValue(typeof(Color), "35, 35, 48")]
        public Color BaseBackColor { get; set; } = Color.FromArgb(35, 35, 48);

        [Category("Modern")]
        [DefaultValue(typeof(Color), "50, 50, 64")]
        public Color HoverBackColor { get; set; } = Color.FromArgb(50, 50, 64);

        [Category("Modern")]
        [DefaultValue(typeof(Color), "28, 28, 38")]
        public Color PressBackColor { get; set; } = Color.FromArgb(28, 28, 38);

        [Category("Modern")]
        [DefaultValue(typeof(Color), "70, 70, 90")]
        public Color BorderColor { get; set; } = Color.FromArgb(70, 70, 90);

        [Category("Modern")]
        [DefaultValue(14)]
        public int CornerRadius { get; set; } = 14;

        // Glyph icon (Segoe MDL2 Assets)
        [Category("Modern")]
        [DefaultValue("")]
        public string IconGlyph { get; set; } = "";

        [Category("Modern")]
        [DefaultValue(0)]
        public int IconSize { get; set; } = 0;

        [Category("Modern")]
        [DefaultValue(10)]
        public int IconGap { get; set; } = 10;

        [Category("Modern")]
        [DefaultValue(true)]
        public bool ShowIcon { get; set; } = true;

        // ===== Designer serialization helpers =====
        public bool ShouldSerializeBaseBackColor() => BaseBackColor != Color.FromArgb(35, 35, 48);
        public bool ShouldSerializeHoverBackColor() => HoverBackColor != Color.FromArgb(50, 50, 64);
        public bool ShouldSerializePressBackColor() => PressBackColor != Color.FromArgb(28, 28, 38);
        public bool ShouldSerializeBorderColor() => BorderColor != Color.FromArgb(70, 70, 90);
        public bool ShouldSerializeCornerRadius() => CornerRadius != 14;
        public bool ShouldSerializeIconGlyph() => !string.IsNullOrEmpty(IconGlyph);
        public bool ShouldSerializeIconGap() => IconGap != 10;
        public bool ShouldSerializeShowIcon() => ShowIcon != true;
        public bool ShouldSerializeIconSize() => IconSize != 0;

        protected override void OnMouseEnter(EventArgs e) { _hover = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { _hover = false; _pressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }
            base.OnMouseDown(mevent);
        }
        protected override void OnMouseUp(MouseEventArgs mevent) { _pressed = false; Invalidate(); base.OnMouseUp(mevent); }

        protected override void OnPaint(PaintEventArgs e)
        {
            // IMPORTANT:
            // - We DO NOT use Graphics.MeasureString for layout anymore
            //   because it can cause weird widths and occasionally GDI+ errors.
            // - We use TextRenderer.MeasureText (WinForms-friendly).
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);
            var radius = Math.Max(6, Math.Min(CornerRadius, Height / 2));

            var bg = _pressed ? PressBackColor : (_hover ? HoverBackColor : BaseBackColor);

            using (var path = RoundedRectPath(rect, radius))
            using (var b = new SolidBrush(bg))
            using (var p = new Pen(BorderColor, 1))
            {
                e.Graphics.FillPath(b, path);
                e.Graphics.DrawPath(p, path);
            }

            // Focus ring
            if (Focused && ShowFocusCues)
            {
                var focusRect = new Rectangle(3, 3, Width - 7, Height - 7);
                using var focusPath = RoundedRectPath(focusRect, Math.Max(4, radius - 2));
                using var focusPen = new Pen(Color.FromArgb(180, 255, 255, 255), 1) { DashStyle = DashStyle.Dot };
                e.Graphics.DrawPath(focusPen, focusPath);
            }

            var text = Text ?? string.Empty;
            bool drawIcon = ShowIcon && !string.IsNullOrWhiteSpace(IconGlyph);

            // Icon font
            Font? iconFont = null;
            try
            {
                int autoSize = Math.Max(12, (int)(Height * 0.42f));
                int finalIconSize = IconSize > 0 ? IconSize : autoSize;
                iconFont = new Font("Segoe MDL2 Assets", finalIconSize, FontStyle.Regular, GraphicsUnit.Point);
            }
            catch
            {
                drawIcon = false; // fallback
            }

            // measure
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;

            Size textSize = TextRenderer.MeasureText(e.Graphics, text, Font, new Size(int.MaxValue, Height), flags);

            Size iconSize = Size.Empty;
            if (drawIcon && iconFont != null)
                iconSize = TextRenderer.MeasureText(e.Graphics, IconGlyph, iconFont, new Size(int.MaxValue, Height), flags);

            int totalW = textSize.Width;
            if (drawIcon) totalW += IconGap + iconSize.Width;

            int startX = (Width - totalW) / 2;
            int centerY = Height / 2;

            // Icon
            if (drawIcon && iconFont != null)
            {
                var iconRect = new Rectangle(
                    startX,
                    0,
                    iconSize.Width,
                    Height
                );

                TextRenderer.DrawText(
                    e.Graphics,
                    IconGlyph,
                    iconFont,
                    iconRect,
                    ForeColor,
                    flags
                );

                startX += iconSize.Width + IconGap;
            }

            // Text
            var textRect = new Rectangle(startX, 0, Width - startX, Height);
            TextRenderer.DrawText(
                e.Graphics,
                text,
                Font,
                textRect,
                ForeColor,
                flags
            );

            iconFont?.Dispose();
        }

        private static GraphicsPath RoundedRectPath(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();

            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
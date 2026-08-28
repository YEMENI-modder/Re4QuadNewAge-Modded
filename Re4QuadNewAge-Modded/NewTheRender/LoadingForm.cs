using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Re4QuadExtremeEditor;

namespace NewAgeTheRender
{
    internal class LoadingForm : Form
    {
        private string _message;
        private float _angle;
        private readonly Timer _timer;

        public LoadingForm(string message)
        {
            _message = message ?? "Loading...";

            Text = "Loading";
            Size = new Size(380, 200);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ControlBox = false;
            TopMost = true;
            ShowInTaskbar = false;
            DoubleBuffered = true;

            DarkTheme.Apply(this);

            _timer = new Timer { Interval = 30 };
            _timer.Tick += (s, e) =>
            {
                _angle += 15f;
                if (_angle >= 360f) _angle -= 360f;
                Invalidate();
            };

            Shown += (s, e) => _timer.Start();
            FormClosing += (s, e) => _timer.Stop();
        }

        public void UpdateText(string text)
        {
            _message = text ?? "Loading...";
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int cx = ClientSize.Width / 2;
            int cy = ClientSize.Height / 2 - 15;

            using (var font = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var textBrush = new SolidBrush(DarkTheme.Text))
            using (var accentBrush = new SolidBrush(DarkTheme.Accent))
            using (var dimBrush = new SolidBrush(Color.FromArgb(60, DarkTheme.Accent)))
            {
                var textSize = g.MeasureString(_message, font);
                g.DrawString(_message, font, textBrush, cx - textSize.Width / 2f, cy + 35);
            }

            int arcRadius = 16;
            int outerDiameter = arcRadius * 2;
            float penWidth = 4f;

            using (var dimPen = new Pen(dimBrushFor(), penWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawArc(dimPen, cx - arcRadius, cy - arcRadius, outerDiameter, outerDiameter, 0, 360);
            }

            using (var pen = new Pen(accentBrushFor(), penWidth) { StartCap = LineCap.Round, EndCap = LineCap.Round })
            {
                g.DrawArc(pen, cx - arcRadius, cy - arcRadius, outerDiameter, outerDiameter, _angle, 90);
            }
        }

        private SolidBrush accentBrushFor()
        {
            return new SolidBrush(DarkTheme.Accent);
        }

        private SolidBrush dimBrushFor()
        {
            return new SolidBrush(Color.FromArgb(40, DarkTheme.Accent.R, DarkTheme.Accent.G, DarkTheme.Accent.B));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

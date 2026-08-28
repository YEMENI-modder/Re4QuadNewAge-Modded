using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Controls
{
    /// <summary>
    /// GroupBox that behaves exactly like the normal WinForms GroupBox in light mode,
    /// and uses a custom charcoal renderer only when DarkTheme enables it.
    /// </summary>
    internal sealed class DarkGroupBox : GroupBox
    {
        private bool darkMode;

        public DarkGroupBox()
        {
            // Keep the designer/native WinForms appearance by default.
            darkMode = false;
        }

        internal void SetDarkMode(bool enabled)
        {
            if (darkMode == enabled)
                return;

            darkMode = enabled;

            if (darkMode)
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                FlatStyle = FlatStyle.Flat;
                BackColor = DarkTheme.Surface;
                ForeColor = DarkTheme.Text;
            }
            else
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, false);
                FlatStyle = FlatStyle.Standard;
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;
            }

            Invalidate(true);
            Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!darkMode)
            {
                base.OnPaint(e);
                return;
            }

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.Clear(BackColor == Color.Transparent ? DarkTheme.Surface : BackColor);

            string title = Text ?? string.Empty;
            Size titleSize = TextRenderer.MeasureText(title, Font);
            int titleLeft = 10;
            int titleRight = Math.Min(Width - 8, titleLeft + titleSize.Width + 6);
            int lineY = Math.Max(8, Font.Height / 2 + 1);

            using (var pen = new Pen(DarkTheme.Border))
            {
                g.DrawLine(pen, 0, lineY, titleLeft - 2, lineY);
                if (titleRight < Width - 4)
                    g.DrawLine(pen, titleRight, lineY, Width - 1, lineY);
                g.DrawLine(pen, 0, lineY, 0, Height - 1);
                g.DrawLine(pen, 0, Height - 1, Width - 1, Height - 1);
                g.DrawLine(pen, Width - 1, lineY, Width - 1, Height - 1);
            }

            if (!string.IsNullOrEmpty(title))
            {
                TextRenderer.DrawText(g, title, Font,
                    new Rectangle(titleLeft, 0, titleSize.Width + 4, Font.Height + 2),
                    ForeColor, BackColor,
                    TextFormatFlags.NoPadding | TextFormatFlags.NoPrefix);
            }
        }
    }

    /// <summary>
    /// TabControl that remains native in light mode and uses owner-drawn tabs in dark mode.
    /// It deliberately does not force UserPaint, because that can suppress the native
    /// tab header and leave the empty strip seen in previous builds.
    /// </summary>
    internal sealed class DarkTabControl : TabControl
    {
        private bool darkMode;

        public DarkTabControl()
        {
            darkMode = false;
            DrawMode = TabDrawMode.Normal;
        }

        internal void SetDarkMode(bool enabled)
        {
            if (darkMode == enabled)
                return;

            darkMode = enabled;

            if (darkMode)
            {
                DrawMode = TabDrawMode.OwnerDrawFixed;
                Appearance = TabAppearance.Normal;
                //size every tab to its own caption instead of one shared
                //fixed width - long names like "JADERLINK TOOLS" used to
                //overflow their 80px button
                SizeMode = TabSizeMode.Normal;
                Padding = new Point(12, 5);
                BackColor = DarkTheme.Window;
                ForeColor = DarkTheme.Text;

                foreach (TabPage page in TabPages)
                {
                    page.UseVisualStyleBackColor = false;
                    page.BackColor = DarkTheme.Window;
                    page.ForeColor = DarkTheme.Text;
                }
            }
            else
            {
                DrawMode = TabDrawMode.Normal;
                Appearance = TabAppearance.FlatButtons;
                BackColor = SystemColors.Control;
                ForeColor = SystemColors.ControlText;

                foreach (TabPage page in TabPages)
                {
                    page.UseVisualStyleBackColor = true;
                    page.BackColor = SystemColors.Control;
                    page.ForeColor = SystemColors.ControlText;
                }
            }

            Invalidate(true);
            Update();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (!darkMode)
            {
                base.OnDrawItem(e);
                return;
            }

            // The final tab header is painted in OnPaint below. Keeping this callback
            // dark as well prevents the native owner-draw pass from flashing white.
            DrawDarkTab(e.Graphics, e.Bounds, e.Index, e.Index == SelectedIndex);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (!darkMode)
            {
                base.OnPaint(e);
                return;
            }

            // Let WinForms paint the child TabPage and its normal layout first.
            base.OnPaint(e);

            // WinForms leaves the unused part of the native tab header in the
            // system color (white on many Windows configurations). Paint the whole
            // header ourselves so there is one continuous charcoal surface.
            Rectangle header = new Rectangle(0, 0, ClientSize.Width, 0);
            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle tab = GetTabRect(i);
                if (tab.Bottom > header.Height)
                    header.Height = tab.Bottom;
            }

            if (header.Height <= 0)
                header.Height = ItemSize.Height + 6;

            using (var brush = new SolidBrush(DarkTheme.Window))
                e.Graphics.FillRectangle(brush, 0, 0, ClientSize.Width, header.Height);

            // Draw a subtle separator under the tabs.
            using (var pen = new Pen(DarkTheme.BorderSoft))
                e.Graphics.DrawLine(pen, 0, header.Height - 1, ClientSize.Width - 1, header.Height - 1);

            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle tab = GetTabRect(i);
                DrawDarkTab(e.Graphics, tab, i, i == SelectedIndex);
            }
        }

        private void DrawDarkTab(Graphics graphics, Rectangle bounds, int index, bool selected)
        {
            if (index < 0 || index >= TabPages.Count)
                return;

            Color bg = selected ? DarkTheme.Selection : DarkTheme.Surface;
            using (var brush = new SolidBrush(bg))
                graphics.FillRectangle(brush, bounds);
            using (var pen = new Pen(selected ? DarkTheme.Accent : DarkTheme.BorderSoft))
                graphics.DrawRectangle(pen, bounds.X, bounds.Y, bounds.Width - 1, bounds.Height - 1);

            TextRenderer.DrawText(graphics, TabPages[index].Text, Font, bounds,
                DarkTheme.Text,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_PAINT = 0x000F;
            const int WM_ERASEBKGND = 0x0014;

            if (darkMode && m.Msg == WM_ERASEBKGND)
            {
                // The native TabControl can erase its header with the Windows
                // system color (white). Paint the client area with our dark
                // surface and prevent the native erase pass from restoring white.
                using (Graphics g = CreateGraphics())
                using (var brush = new SolidBrush(DarkTheme.Window))
                {
                    g.FillRectangle(brush, ClientRectangle);
                }

                m.Result = (IntPtr)1;
                return;
            }

            base.WndProc(ref m);

            if (darkMode && m.Msg == WM_PAINT)
                PaintDarkHeader();
        }

        private void PaintDarkHeader()
        {
            if (!IsHandleCreated || TabPages.Count == 0)
                return;

            int headerBottom = ItemSize.Height + 6;
            for (int i = 0; i < TabPages.Count; i++)
            {
                Rectangle tab = GetTabRect(i);
                if (tab.Bottom > headerBottom)
                    headerBottom = tab.Bottom;
            }

            Rectangle client = ClientRectangle;
            headerBottom = Math.Min(headerBottom, client.Height);
            if (headerBottom <= 0 || client.Width <= 0)
                return;

            using (Graphics g = CreateGraphics())
            {
                // Paint the complete header first. This removes the white native
                // area to the right of the last tab and any small white margins.
                using (var brush = new SolidBrush(DarkTheme.Window))
                    g.FillRectangle(brush, 0, 0, client.Width, headerBottom);

                // Repaint all tabs on top of the dark header.
                for (int i = 0; i < TabPages.Count; i++)
                {
                    Rectangle tab = GetTabRect(i);
                    DrawDarkTab(g, tab, i, i == SelectedIndex);
                }

                using (var pen = new Pen(DarkTheme.BorderSoft))
                    g.DrawLine(pen, 0, headerBottom - 1, client.Width - 1, headerBottom - 1);
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (!darkMode)
            {
                base.OnPaintBackground(e);
                return;
            }

            e.Graphics.Clear(DarkTheme.Window);
        }
    }
}

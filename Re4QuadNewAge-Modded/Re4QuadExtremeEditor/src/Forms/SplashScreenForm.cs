using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Forms
{
    public partial class SplashScreenForm : Form
    {
        private SplashScreenConteiner conteiner;

        private bool BlockClose = true;

        public SplashScreenForm(SplashScreenConteiner conteiner)
        {
            conteiner.Close = CloseForm;
            conteiner.ReleasedToClose = ReleasedToClose;
            this.conteiner = conteiner;
            InitializeComponent();

            //borderless window: allow dragging it from anywhere
            MouseDown += SplashDrag_MouseDown;
            foreach (Control c in Controls)
            {
                c.MouseDown += SplashDrag_MouseDown;
            }
        }

        // ---------------- custom chrome painting ----------------

        protected void SplashScreenForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            //accent strip along the top edge
            using (SolidBrush accent = new SolidBrush(DarkTheme.Accent))
            {
                g.FillRectangle(accent, 0, 0, ClientSize.Width, 4);
            }

            //subtle outer border so the floating window reads cleanly
            using (Pen border = new Pen(DarkTheme.Border))
            {
                g.DrawRectangle(border, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }

            //app icon centered above the title
            if (Icon != null)
            {
                g.DrawIcon(Icon, new Rectangle(ClientSize.Width / 2 - 24, 28, 48, 48));
            }
        }

        // ---------------- borderless drag support ----------------

        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private void SplashDrag_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }
        private void ReleasedToClose() 
        {
            if (conteiner.FormIsClosed == false)
            {
                this.Invoke(new Action(InvokedReleasedToClose));
            }
        }

        private void InvokedReleasedToClose() 
        {
            BlockClose = false;
        }

        private void CloseForm() 
        {
            if (conteiner.FormIsClosed == false)
            {
                this.Invoke(new Action(InvokedCloseForm));
            }
        }

        private void InvokedCloseForm() 
        {
            BlockClose = false;
            Close();
        }

        private void SplashScreenForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            conteiner.FormIsClosed = true;
        }

        private void SplashScreenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BlockClose)
            {
                e.Cancel = true;
            }
        }


        //----------------------
        private void To(string url)
        {
            try { System.Diagnostics.Process.Start("explorer.exe", url); } catch (Exception) { }
        }

        private void linkLabelJaderLinkBlog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            To("https://jaderlink.blogspot.com/");
        }

        private void linkLabelJaderLinkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            To("https://github.com/JADERLINK");
        }

        private void linkLabelYoutubeJaderLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            To("https://www.youtube.com/@JADERLINK");
        }

        private void linkLabelDonate_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            To("https://jaderlink.github.io/Donate/");
        }
    }
}

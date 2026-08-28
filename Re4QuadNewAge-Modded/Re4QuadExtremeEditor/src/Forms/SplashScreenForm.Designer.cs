
namespace Re4QuadExtremeEditor.src.Forms
{
    partial class SplashScreenForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SplashScreenForm));
            this.labelTitle = new System.Windows.Forms.Label();
            this.labelSubtitle = new System.Windows.Forms.Label();
            this.labelStatus = new System.Windows.Forms.Label();
            this.linkLabelYoutubeJaderLink = new System.Windows.Forms.LinkLabel();
            this.linkLabelJaderLinkGitHub = new System.Windows.Forms.LinkLabel();
            this.linkLabelJaderLinkBlog = new System.Windows.Forms.LinkLabel();
            this.linkLabelDonate = new System.Windows.Forms.LinkLabel();
            this.progressBarLoading = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();

            //
            // labelTitle
            //
            this.labelTitle.Font = new System.Drawing.Font("Segoe UI", 19.75F, System.Drawing.FontStyle.Bold);
            this.labelTitle.ForeColor = Re4QuadExtremeEditor.DarkTheme.Text;
            this.labelTitle.BackColor = System.Drawing.Color.Transparent;
            this.labelTitle.Location = new System.Drawing.Point(0, 86);
            this.labelTitle.Name = "labelTitle";
            this.labelTitle.Size = new System.Drawing.Size(480, 42);
            this.labelTitle.TabIndex = 0;
            this.labelTitle.Text = "RE4 QUAD EXTREME EDITOR";
            this.labelTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelSubtitle
            //
            this.labelSubtitle.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.labelSubtitle.ForeColor = Re4QuadExtremeEditor.DarkTheme.TextSecondary;
            this.labelSubtitle.BackColor = System.Drawing.Color.Transparent;
            this.labelSubtitle.Location = new System.Drawing.Point(0, 128);
            this.labelSubtitle.Name = "labelSubtitle";
            this.labelSubtitle.Size = new System.Drawing.Size(480, 24);
            this.labelSubtitle.TabIndex = 1;
            this.labelSubtitle.Text = "[ NEW AGE ]     BY JADERLINK";
            this.labelSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // labelStatus
            //
            this.labelStatus.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold);
            this.labelStatus.ForeColor = Re4QuadExtremeEditor.DarkTheme.Accent;
            this.labelStatus.BackColor = System.Drawing.Color.Transparent;
            this.labelStatus.Location = new System.Drawing.Point(0, 168);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(480, 20);
            this.labelStatus.TabIndex = 7;
            this.labelStatus.Text = "LOADING...";
            this.labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // progressBarLoading
            //
            this.progressBarLoading.BackColor = Re4QuadExtremeEditor.DarkTheme.Surface2;
            this.progressBarLoading.ForeColor = Re4QuadExtremeEditor.DarkTheme.Accent;
            this.progressBarLoading.Location = new System.Drawing.Point(90, 198);
            this.progressBarLoading.MarqueeAnimationSpeed = 30;
            this.progressBarLoading.Name = "progressBarLoading";
            this.progressBarLoading.Size = new System.Drawing.Size(300, 10);
            this.progressBarLoading.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarLoading.TabIndex = 2;
            //
            // linkLabelYoutubeJaderLink
            //
            this.linkLabelYoutubeJaderLink.ActiveLinkColor = Re4QuadExtremeEditor.DarkTheme.AccentHover;
            this.linkLabelYoutubeJaderLink.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabelYoutubeJaderLink.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelYoutubeJaderLink.LinkColor = Re4QuadExtremeEditor.DarkTheme.TextSecondary;
            this.linkLabelYoutubeJaderLink.Location = new System.Drawing.Point(0, 240);
            this.linkLabelYoutubeJaderLink.Name = "linkLabelYoutubeJaderLink";
            this.linkLabelYoutubeJaderLink.Size = new System.Drawing.Size(120, 22);
            this.linkLabelYoutubeJaderLink.TabIndex = 3;
            this.linkLabelYoutubeJaderLink.TabStop = true;
            this.linkLabelYoutubeJaderLink.Text = "YouTube";
            this.linkLabelYoutubeJaderLink.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelYoutubeJaderLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelYoutubeJaderLink_LinkClicked);
            //
            // linkLabelJaderLinkGitHub
            //
            this.linkLabelJaderLinkGitHub.ActiveLinkColor = Re4QuadExtremeEditor.DarkTheme.AccentHover;
            this.linkLabelJaderLinkGitHub.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabelJaderLinkGitHub.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelJaderLinkGitHub.LinkColor = Re4QuadExtremeEditor.DarkTheme.TextSecondary;
            this.linkLabelJaderLinkGitHub.Location = new System.Drawing.Point(120, 240);
            this.linkLabelJaderLinkGitHub.Name = "linkLabelJaderLinkGitHub";
            this.linkLabelJaderLinkGitHub.Size = new System.Drawing.Size(120, 22);
            this.linkLabelJaderLinkGitHub.TabIndex = 4;
            this.linkLabelJaderLinkGitHub.TabStop = true;
            this.linkLabelJaderLinkGitHub.Text = "GitHub";
            this.linkLabelJaderLinkGitHub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelJaderLinkGitHub.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelJaderLinkGitHub_LinkClicked);
            //
            // linkLabelJaderLinkBlog
            //
            this.linkLabelJaderLinkBlog.ActiveLinkColor = Re4QuadExtremeEditor.DarkTheme.AccentHover;
            this.linkLabelJaderLinkBlog.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabelJaderLinkBlog.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelJaderLinkBlog.LinkColor = Re4QuadExtremeEditor.DarkTheme.TextSecondary;
            this.linkLabelJaderLinkBlog.Location = new System.Drawing.Point(240, 240);
            this.linkLabelJaderLinkBlog.Name = "linkLabelJaderLinkBlog";
            this.linkLabelJaderLinkBlog.Size = new System.Drawing.Size(120, 22);
            this.linkLabelJaderLinkBlog.TabIndex = 5;
            this.linkLabelJaderLinkBlog.TabStop = true;
            this.linkLabelJaderLinkBlog.Text = "Blog";
            this.linkLabelJaderLinkBlog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelJaderLinkBlog.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelJaderLinkBlog_LinkClicked);
            //
            // linkLabelDonate
            //
            this.linkLabelDonate.ActiveLinkColor = Re4QuadExtremeEditor.DarkTheme.AccentHover;
            this.linkLabelDonate.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.linkLabelDonate.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.linkLabelDonate.LinkColor = Re4QuadExtremeEditor.DarkTheme.TextSecondary;
            this.linkLabelDonate.Location = new System.Drawing.Point(360, 240);
            this.linkLabelDonate.Name = "linkLabelDonate";
            this.linkLabelDonate.Size = new System.Drawing.Size(120, 22);
            this.linkLabelDonate.TabIndex = 6;
            this.linkLabelDonate.TabStop = true;
            this.linkLabelDonate.Text = "Donate";
            this.linkLabelDonate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.linkLabelDonate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelDonate_LinkClicked);
            //
            // SplashScreenForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = Re4QuadExtremeEditor.DarkTheme.Window;
            this.ClientSize = new System.Drawing.Size(480, 290);
            this.Controls.Add(this.linkLabelDonate);
            this.Controls.Add(this.linkLabelJaderLinkBlog);
            this.Controls.Add(this.linkLabelJaderLinkGitHub);
            this.Controls.Add(this.linkLabelYoutubeJaderLink);
            this.Controls.Add(this.progressBarLoading);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelSubtitle);
            this.Controls.Add(this.labelTitle);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.ForeColor = Re4QuadExtremeEditor.DarkTheme.Text;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SplashScreenForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RE4 QUAD EXTREME EDITOR [NEW AGE]";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SplashScreenForm_Paint);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SplashScreenForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SplashScreenForm_FormClosed);
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Label labelTitle;
        private System.Windows.Forms.Label labelSubtitle;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.LinkLabel linkLabelYoutubeJaderLink;
        private System.Windows.Forms.LinkLabel linkLabelJaderLinkGitHub;
        private System.Windows.Forms.LinkLabel linkLabelJaderLinkBlog;
        private System.Windows.Forms.LinkLabel linkLabelDonate;
        private System.Windows.Forms.ProgressBar progressBarLoading;
    }
}

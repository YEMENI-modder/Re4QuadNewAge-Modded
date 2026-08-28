using System;
using System.Drawing;
using System.Windows.Forms;

namespace Re4QuadExtremeEditor.src.Forms
{
    /// <summary>
    /// Dark themed "save before exit" dialog that replaces the stock white
    /// MessageBox: Save Project / Save Project As... / Close without saving /
    /// Cancel, styled with the same graphite palette as the rest of the app.
    /// </summary>
    internal class ExitConfirmForm : Form
    {
        public enum ExitChoice { Cancel, Save, SaveAs, Discard }

        public ExitChoice Choice { get; private set; } = ExitChoice.Cancel;

        private readonly string projectPath;

        public ExitConfirmForm(string projectPath)
        {
            this.projectPath = projectPath;
            bool hasProject = !string.IsNullOrEmpty(projectPath);

            DarkTheme.Apply(this);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            DoubleBuffered = true;
            Text = "Exit";
            ClientSize = new Size(400, 262);

            var title = new Label();
            title.Text = "Save project before closing?";
            title.AutoSize = true;
            title.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            title.ForeColor = DarkTheme.Text;
            title.Location = new Point(20, 18);
            Controls.Add(title);

            var subtitle = new Label();
            subtitle.AutoSize = false;
            subtitle.Size = new Size(ClientSize.Width - 40, 40);
            subtitle.ForeColor = DarkTheme.TextSecondary;
            subtitle.Location = new Point(20, 50);
            subtitle.Text = hasProject
                ? "\u201C" + System.IO.Path.GetFileName(projectPath) + "\u201D will be closed."
                : "No project is currently loaded.";
            Controls.Add(subtitle);

            int y = 100;
            Controls.Add(MakeButton("Save Project", y, true));
            y += 38;
            Controls.Add(MakeButton("Save Project As...", y, false));
            y += 38;
            Controls.Add(MakeButton("Close without saving", y, false));
            y += 38;
            Button cancel = MakeButton("Cancel", y, false);
            Controls.Add(cancel);

            AcceptButton = saveButton;
            CancelButton = cancel;
        }

        private Button saveButton;

        private Button MakeButton(string text, int y, bool accent)
        {
            Button b = new Button();
            b.Text = text;
            b.Size = new Size(ClientSize.Width - 40, 32);
            b.Location = new Point(20, y);
            b.TabStop = false;
            b.UseVisualStyleBackColor = false;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9f, FontStyle.Regular);

            if (accent)
            {
                b.BackColor = DarkTheme.Accent;
                b.ForeColor = Color.White;
                b.FlatAppearance.MouseOverBackColor = DarkTheme.AccentHover;
                b.FlatAppearance.MouseDownBackColor = DarkTheme.AccentPressed;
                b.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                b.Click += delegate { Choice = ExitChoice.Save; DialogResult = DialogResult.OK; Close(); };
                saveButton = b;
            }
            else
            {
                b.BackColor = DarkTheme.Surface2;
                b.ForeColor = DarkTheme.Text;
                b.FlatAppearance.BorderColor = DarkTheme.Border;
                b.FlatAppearance.MouseOverBackColor = DarkTheme.Surface3;
                b.FlatAppearance.MouseDownBackColor = DarkTheme.Selection;
            }

            if (text == "Save Project As...")
            {
                b.Click += delegate { Choice = ExitChoice.SaveAs; DialogResult = DialogResult.OK; Close(); };
            }
            else if (text == "Close without saving")
            {
                b.Click += delegate { Choice = ExitChoice.Discard; DialogResult = DialogResult.OK; Close(); };
            }
            else if (text == "Cancel")
            {
                b.Click += delegate { Choice = ExitChoice.Cancel; DialogResult = DialogResult.Cancel; Close(); };
            }

            return b;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Choice = ExitChoice.Cancel;
                DialogResult = DialogResult.Cancel;
                Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

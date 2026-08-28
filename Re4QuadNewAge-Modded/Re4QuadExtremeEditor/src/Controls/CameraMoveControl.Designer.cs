
namespace Re4QuadExtremeEditor.src.Controls
{
    partial class CameraMoveControl
    {
        /// <summary> 
        /// Variável de designer necessária.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Limpar os recursos que estão sendo usados.
        /// </summary>
        /// <param name="disposing">true se for necessário descartar os recursos gerenciados; caso contrário, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código gerado pelo Designer de Componentes

        /// <summary> 
        /// Método necessário para suporte ao Designer - não modifique 
        /// o conteúdo deste método com o editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelCamModeText = new System.Windows.Forms.Label();
            this.comboBoxCameraMode = new System.Windows.Forms.ComboBox();
            this.labelCamSpeedPercentage = new System.Windows.Forms.Label();
            this.trackBarCamSpeed = new System.Windows.Forms.TrackBar();
            this.buttonGet = new System.Windows.Forms.Button();
            this.buttonGrid = new System.Windows.Forms.Button();
            this.textBoxGridSize = new System.Windows.Forms.TextBox();
            this.labelFovValue = new System.Windows.Forms.Label();
            this.trackBarFov = new System.Windows.Forms.TrackBar();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarCamSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFov)).BeginInit();
            this.SuspendLayout();
            // 
            // labelCamModeText
            // 
            this.labelCamModeText.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCamModeText.Location = new System.Drawing.Point(128, 41);
            this.labelCamModeText.Name = "labelCamModeText";
            this.labelCamModeText.Size = new System.Drawing.Size(119, 13);
            this.labelCamModeText.TabIndex = 2;
            this.labelCamModeText.Text = "Camera Mode:";
            this.labelCamModeText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // comboBoxCameraMode
            // 
            this.comboBoxCameraMode.DisplayMember = "1";
            this.comboBoxCameraMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxCameraMode.Font = new System.Drawing.Font("Corbel", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxCameraMode.Items.AddRange(new object[] {
            "Fly",
            "Orbit",
            "Top",
            "Bottom",
            "Left",
            "Right",
            "Front",
            "Back"});
            this.comboBoxCameraMode.Location = new System.Drawing.Point(127, 56);
            this.comboBoxCameraMode.Name = "comboBoxCameraMode";
            this.comboBoxCameraMode.Size = new System.Drawing.Size(120, 22);
            this.comboBoxCameraMode.TabIndex = 3;
            this.comboBoxCameraMode.TabStop = false;
            this.comboBoxCameraMode.SelectedIndexChanged += new System.EventHandler(this.comboBoxCameraMode_SelectedIndexChanged);
            // 
            // labelCamSpeedPercentage
            // 
            this.labelCamSpeedPercentage.AutoSize = true;
            this.labelCamSpeedPercentage.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCamSpeedPercentage.Location = new System.Drawing.Point(120, 3);
            this.labelCamSpeedPercentage.Name = "labelCamSpeedPercentage";
            this.labelCamSpeedPercentage.Size = new System.Drawing.Size(112, 14);
            this.labelCamSpeedPercentage.TabIndex = 0;
            this.labelCamSpeedPercentage.Text = "Cam speed: 100%";
            this.labelCamSpeedPercentage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackBarCamSpeed
            // 
            this.trackBarCamSpeed.AutoSize = false;
            this.trackBarCamSpeed.LargeChange = 10;
            this.trackBarCamSpeed.Location = new System.Drawing.Point(120, 18);
            this.trackBarCamSpeed.Maximum = 100;
            this.trackBarCamSpeed.Name = "trackBarCamSpeed";
            this.trackBarCamSpeed.Size = new System.Drawing.Size(130, 20);
            this.trackBarCamSpeed.SmallChange = 5;
            this.trackBarCamSpeed.TabIndex = 1;
            this.trackBarCamSpeed.TabStop = false;
            this.trackBarCamSpeed.TickFrequency = 10;
            this.trackBarCamSpeed.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarCamSpeed.Value = 50;
            this.trackBarCamSpeed.Scroll += new System.EventHandler(this.trackBarCamSpeed_Scroll);
            // 
            // buttonGet
            // 
            this.buttonGet.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGet.Location = new System.Drawing.Point(127, 81);
            this.buttonGet.Name = "buttonGet";
            this.buttonGet.Size = new System.Drawing.Size(38, 22);
            this.buttonGet.TabIndex = 4;
            this.buttonGet.TabStop = false;
            this.buttonGet.Text = "Get";
            this.buttonGet.UseVisualStyleBackColor = true;
            this.buttonGet.Click += new System.EventHandler(this.buttonGet_Click);
            // 
            // buttonGrid
            // 
            this.buttonGrid.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonGrid.Location = new System.Drawing.Point(171, 81);
            this.buttonGrid.Name = "buttonGrid";
            this.buttonGrid.Size = new System.Drawing.Size(45, 22);
            this.buttonGrid.TabIndex = 5;
            this.buttonGrid.TabStop = false;
            this.buttonGrid.Text = "Grid";
            this.buttonGrid.UseVisualStyleBackColor = true;
            this.buttonGrid.Click += new System.EventHandler(this.buttonGrid_Click);
            // 
            // textBoxGridSize
            // 
            this.textBoxGridSize.BackColor = System.Drawing.SystemColors.Control;
            this.textBoxGridSize.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxGridSize.Location = new System.Drawing.Point(222, 82);
            this.textBoxGridSize.MaxLength = 4;
            this.textBoxGridSize.Name = "textBoxGridSize";
            this.textBoxGridSize.Size = new System.Drawing.Size(28, 20);
            this.textBoxGridSize.TabIndex = 5;
            this.textBoxGridSize.TabStop = false;
            this.textBoxGridSize.Text = "100";
            this.textBoxGridSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxGridSize.TextChanged += new System.EventHandler(this.textBoxGridSize_TextChanged);
            this.textBoxGridSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxGridSize_KeyPress);
            // 
            // labelFovValue
            // 
            this.labelFovValue.AutoSize = true;
            this.labelFovValue.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelFovValue.Location = new System.Drawing.Point(124, 106);
            this.labelFovValue.Name = "labelFovValue";
            this.labelFovValue.Size = new System.Drawing.Size(70, 14);
            this.labelFovValue.TabIndex = 6;
            this.labelFovValue.Text = "FOV";
            this.labelFovValue.DoubleClick += new System.EventHandler(this.labelFovValue_DoubleClick);
            // 
            // trackBarFov
            // 
            this.trackBarFov.AutoSize = false;
            this.trackBarFov.LargeChange = 10;
            this.trackBarFov.Location = new System.Drawing.Point(178, 104);
            this.trackBarFov.Maximum = 130;
            this.trackBarFov.Minimum = 20;
            this.trackBarFov.Name = "trackBarFov";
            this.trackBarFov.Size = new System.Drawing.Size(72, 20);
            this.trackBarFov.SmallChange = 1;
            this.trackBarFov.TabIndex = 7;
            this.trackBarFov.TabStop = false;
            this.trackBarFov.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trackBarFov.Value = 60;
            this.trackBarFov.Scroll += new System.EventHandler(this.trackBarFov_Scroll);
            // 
            // CameraMoveControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.trackBarFov);
            this.Controls.Add(this.labelFovValue);
            this.Controls.Add(this.textBoxGridSize);
            this.Controls.Add(this.buttonGet);
            this.Controls.Add(this.buttonGrid);
            this.Controls.Add(this.labelCamSpeedPercentage);
            this.Controls.Add(this.labelCamModeText);
            this.Controls.Add(this.comboBoxCameraMode);
            this.Controls.Add(this.trackBarCamSpeed);
            this.Name = "CameraMoveControl";
            this.Size = new System.Drawing.Size(250, 126);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarCamSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarFov)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelCamModeText;
        private System.Windows.Forms.ComboBox comboBoxCameraMode;
        private System.Windows.Forms.Label labelCamSpeedPercentage;
        private System.Windows.Forms.TrackBar trackBarCamSpeed;
        private System.Windows.Forms.Button buttonGet;
        private System.Windows.Forms.Button buttonGrid;
        private System.Windows.Forms.TextBox textBoxGridSize;
        private System.Windows.Forms.Label labelFovValue;
        private System.Windows.Forms.TrackBar trackBarFov;
    }
}

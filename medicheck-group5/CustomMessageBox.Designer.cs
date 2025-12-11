namespace medicheck_group5
{
    partial class CustomMessageBox
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.guna2CustomGradientPanel1 = new Guna.UI2.WinForms.Guna2CustomGradientPanel();
            this.labelCaption = new MediCheck_UI_.GradientTextLabel();
            this.guna2PictureBox1 = new Guna.UI2.WinForms.Guna2PictureBox();
            this.bttnOk = new Guna.UI2.WinForms.Guna2Button();
            this.guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(this.components);
            this.labelMessage = new MediCheck_UI_.GradientTextLabel();
            this.bttnClose = new Guna.UI2.WinForms.Guna2ControlBox();
            this.guna2ShadowForm1 = new Guna.UI2.WinForms.Guna2ShadowForm(this.components);
            this.guna2DragControl1 = new Guna.UI2.WinForms.Guna2DragControl(this.components);
            this.guna2CustomGradientPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // guna2CustomGradientPanel1
            // 
            this.guna2CustomGradientPanel1.Controls.Add(this.bttnClose);
            this.guna2CustomGradientPanel1.Controls.Add(this.labelCaption);
            this.guna2CustomGradientPanel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.guna2CustomGradientPanel1.FillColor = System.Drawing.Color.DarkCyan;
            this.guna2CustomGradientPanel1.FillColor2 = System.Drawing.Color.Aqua;
            this.guna2CustomGradientPanel1.FillColor3 = System.Drawing.Color.Aqua;
            this.guna2CustomGradientPanel1.FillColor4 = System.Drawing.Color.Teal;
            this.guna2CustomGradientPanel1.Location = new System.Drawing.Point(0, 0);
            this.guna2CustomGradientPanel1.Name = "guna2CustomGradientPanel1";
            this.guna2CustomGradientPanel1.Size = new System.Drawing.Size(533, 54);
            this.guna2CustomGradientPanel1.TabIndex = 0;
            // 
            // labelCaption
            // 
            this.labelCaption.AutoSize = true;
            this.labelCaption.BackColor = System.Drawing.Color.Transparent;
            this.labelCaption.Font = new System.Drawing.Font("Viga", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCaption.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelCaption.GradientStartColor = System.Drawing.Color.DarkSlateGray;
            this.labelCaption.Location = new System.Drawing.Point(15, 18);
            this.labelCaption.Name = "labelCaption";
            this.labelCaption.Size = new System.Drawing.Size(156, 20);
            this.labelCaption.TabIndex = 1;
            this.labelCaption.Text = "gradientTextLabel1";
            this.labelCaption.Click += new System.EventHandler(this.gradientTextLabel1_Click);
            // 
            // guna2PictureBox1
            // 
            this.guna2PictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.guna2PictureBox1.FillColor = System.Drawing.Color.Transparent;
            this.guna2PictureBox1.Image = global::medicheck_group5.Properties.Resources.MEDI_CHECK__7_1;
            this.guna2PictureBox1.ImageRotate = 0F;
            this.guna2PictureBox1.Location = new System.Drawing.Point(-1, 50);
            this.guna2PictureBox1.Name = "guna2PictureBox1";
            this.guna2PictureBox1.Size = new System.Drawing.Size(122, 111);
            this.guna2PictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.guna2PictureBox1.TabIndex = 3;
            this.guna2PictureBox1.TabStop = false;
            this.guna2PictureBox1.UseTransparentBackground = true;
            this.guna2PictureBox1.Click += new System.EventHandler(this.guna2PictureBox1_Click);
            // 
            // bttnOk
            // 
            this.bttnOk.BackColor = System.Drawing.Color.Transparent;
            this.bttnOk.BorderRadius = 2;
            this.bttnOk.Cursor = System.Windows.Forms.Cursors.Hand;
            this.bttnOk.DisabledState.BorderColor = System.Drawing.Color.DarkGray;
            this.bttnOk.DisabledState.CustomBorderColor = System.Drawing.Color.DarkGray;
            this.bttnOk.DisabledState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(169)))), ((int)(((byte)(169)))), ((int)(((byte)(169)))));
            this.bttnOk.DisabledState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(141)))), ((int)(((byte)(141)))), ((int)(((byte)(141)))));
            this.bttnOk.FillColor = System.Drawing.Color.LightSeaGreen;
            this.bttnOk.Font = new System.Drawing.Font("Viga", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bttnOk.ForeColor = System.Drawing.Color.Cyan;
            this.bttnOk.HoverState.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(130)))), ((int)(((byte)(126)))));
            this.bttnOk.Location = new System.Drawing.Point(209, 150);
            this.bttnOk.Name = "bttnOk";
            this.bttnOk.Size = new System.Drawing.Size(102, 42);
            this.bttnOk.TabIndex = 4;
            this.bttnOk.Text = "OK";
            this.bttnOk.Click += new System.EventHandler(this.guna2Button1_Click);
            // 
            // guna2Elipse1
            // 
            this.guna2Elipse1.BorderRadius = 10;
            this.guna2Elipse1.TargetControl = this;
            // 
            // labelMessage
            // 
            this.labelMessage.AutoSize = true;
            this.labelMessage.BackColor = System.Drawing.Color.Transparent;
            this.labelMessage.Font = new System.Drawing.Font("Viga", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelMessage.GradientEndColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.labelMessage.GradientStartColor = System.Drawing.Color.DarkSlateGray;
            this.labelMessage.Location = new System.Drawing.Point(110, 97);
            this.labelMessage.Name = "labelMessage";
            this.labelMessage.Size = new System.Drawing.Size(138, 20);
            this.labelMessage.TabIndex = 2;
            this.labelMessage.Text = "gradientTextLabel1";
            this.labelMessage.Click += new System.EventHandler(this.labelMessage_Click);
            // 
            // bttnClose
            // 
            this.bttnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.bttnClose.BackColor = System.Drawing.Color.Transparent;
            this.bttnClose.BorderRadius = 5;
            this.bttnClose.FillColor = System.Drawing.Color.DarkCyan;
            this.bttnClose.HoverState.FillColor = System.Drawing.Color.Teal;
            this.bttnClose.IconColor = System.Drawing.Color.White;
            this.bttnClose.Location = new System.Drawing.Point(499, 6);
            this.bttnClose.Name = "bttnClose";
            this.bttnClose.Size = new System.Drawing.Size(28, 26);
            this.bttnClose.TabIndex = 5;
            // 
            // guna2ShadowForm1
            // 
            this.guna2ShadowForm1.BorderRadius = 20;
            this.guna2ShadowForm1.ShadowColor = System.Drawing.Color.Teal;
            this.guna2ShadowForm1.TargetForm = this;
            // 
            // guna2DragControl1
            // 
            this.guna2DragControl1.DockIndicatorTransparencyValue = 0.6D;
            this.guna2DragControl1.TargetControl = this;
            this.guna2DragControl1.UseTransparentDrag = true;
            // 
            // CustomMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightCyan;
            this.ClientSize = new System.Drawing.Size(533, 211);
            this.Controls.Add(this.bttnOk);
            this.Controls.Add(this.guna2PictureBox1);
            this.Controls.Add(this.labelMessage);
            this.Controls.Add(this.guna2CustomGradientPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomMessageBox";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CustomMessageBox";
            this.Load += new System.EventHandler(this.CustomMessageBox_Load);
            this.guna2CustomGradientPanel1.ResumeLayout(false);
            this.guna2CustomGradientPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.guna2PictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MediCheck_UI_.GradientTextLabel labelCaption;
        private Guna.UI2.WinForms.Guna2CustomGradientPanel guna2CustomGradientPanel1;
        private Guna.UI2.WinForms.Guna2PictureBox guna2PictureBox1;
        private Guna.UI2.WinForms.Guna2Button bttnOk;
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;
        private MediCheck_UI_.GradientTextLabel labelMessage;
        private Guna.UI2.WinForms.Guna2ControlBox bttnClose;
        private Guna.UI2.WinForms.Guna2ShadowForm guna2ShadowForm1;
        private Guna.UI2.WinForms.Guna2DragControl guna2DragControl1;
    }
}
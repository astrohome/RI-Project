namespace WikiStats
{
    partial class MainForm
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
            this.bList = new System.Windows.Forms.Button();
            this.lbFiles = new System.Windows.Forms.ListBox();
            this.pbDownload = new System.Windows.Forms.ProgressBar();
            this.bProceed = new System.Windows.Forms.Button();
            this.ssBottom = new System.Windows.Forms.StatusStrip();
            this.tssStatic = new System.Windows.Forms.ToolStripStatusLabel();
            this.tssStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lProgressBar = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.ssBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.SuspendLayout();
            // 
            // bList
            // 
            this.bList.Location = new System.Drawing.Point(12, 12);
            this.bList.Name = "bList";
            this.bList.Size = new System.Drawing.Size(156, 23);
            this.bList.TabIndex = 0;
            this.bList.Text = "Download list of files";
            this.bList.UseVisualStyleBackColor = true;
            this.bList.Click += new System.EventHandler(this.bList_Click);
            // 
            // lbFiles
            // 
            this.lbFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbFiles.FormattingEnabled = true;
            this.lbFiles.Location = new System.Drawing.Point(12, 67);
            this.lbFiles.Name = "lbFiles";
            this.lbFiles.Size = new System.Drawing.Size(690, 251);
            this.lbFiles.TabIndex = 1;
            // 
            // pbDownload
            // 
            this.pbDownload.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbDownload.Location = new System.Drawing.Point(12, 343);
            this.pbDownload.Name = "pbDownload";
            this.pbDownload.Size = new System.Drawing.Size(690, 23);
            this.pbDownload.TabIndex = 2;
            this.pbDownload.Visible = false;
            // 
            // bProceed
            // 
            this.bProceed.Enabled = false;
            this.bProceed.Location = new System.Drawing.Point(174, 12);
            this.bProceed.Name = "bProceed";
            this.bProceed.Size = new System.Drawing.Size(156, 23);
            this.bProceed.TabIndex = 4;
            this.bProceed.Text = "Proceed with downloaded file";
            this.bProceed.UseVisualStyleBackColor = true;
            this.bProceed.Click += new System.EventHandler(this.bProceed_Click);
            // 
            // ssBottom
            // 
            this.ssBottom.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tssStatic,
            this.tssStatus});
            this.ssBottom.Location = new System.Drawing.Point(0, 379);
            this.ssBottom.Name = "ssBottom";
            this.ssBottom.Size = new System.Drawing.Size(714, 22);
            this.ssBottom.TabIndex = 5;
            this.ssBottom.Text = "statusStrip1";
            // 
            // tssStatic
            // 
            this.tssStatic.Name = "tssStatic";
            this.tssStatic.Size = new System.Drawing.Size(84, 17);
            this.tssStatic.Text = "Current status:";
            // 
            // tssStatus
            // 
            this.tssStatus.Name = "tssStatus";
            this.tssStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // lProgressBar
            // 
            this.lProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lProgressBar.AutoSize = true;
            this.lProgressBar.Location = new System.Drawing.Point(13, 324);
            this.lProgressBar.Name = "lProgressBar";
            this.lProgressBar.Size = new System.Drawing.Size(107, 13);
            this.lProgressBar.TabIndex = 6;
            this.lProgressBar.Text = "State of downloading";
            this.lProgressBar.Visible = false;
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(336, 12);
            this.trackBar1.Maximum = 1;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(366, 45);
            this.trackBar1.TabIndex = 8;
            this.trackBar1.Value = 1;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 401);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.lProgressBar);
            this.Controls.Add(this.ssBottom);
            this.Controls.Add(this.bProceed);
            this.Controls.Add(this.pbDownload);
            this.Controls.Add(this.lbFiles);
            this.Controls.Add(this.bList);
            this.Name = "MainForm";
            this.Text = "WikiStats";
            this.ssBottom.ResumeLayout(false);
            this.ssBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button bList;
        private System.Windows.Forms.ListBox lbFiles;
        private System.Windows.Forms.ProgressBar pbDownload;
        private System.Windows.Forms.Button bProceed;
        private System.Windows.Forms.StatusStrip ssBottom;
        private System.Windows.Forms.ToolStripStatusLabel tssStatic;
        private System.Windows.Forms.ToolStripStatusLabel tssStatus;
        private System.Windows.Forms.Label lProgressBar;
        private System.Windows.Forms.TrackBar trackBar1;
    }
}


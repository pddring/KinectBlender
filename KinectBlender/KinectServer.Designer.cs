namespace KinectBlender
{
    partial class KinectServer
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lstLog = new ListBox();
            grpLog = new GroupBox();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // lstLog
            // 
            lstLog.Dock = DockStyle.Fill;
            lstLog.FormattingEnabled = true;
            lstLog.ItemHeight = 15;
            lstLog.Location = new Point(3, 19);
            lstLog.Name = "lstLog";
            lstLog.Size = new Size(794, 428);
            lstLog.TabIndex = 0;
            // 
            // grpLog
            // 
            grpLog.Controls.Add(lstLog);
            grpLog.Dock = DockStyle.Fill;
            grpLog.Location = new Point(0, 0);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(800, 450);
            grpLog.TabIndex = 1;
            grpLog.TabStop = false;
            grpLog.Text = "Log:";
            // 
            // KinectServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(grpLog);
            Name = "KinectServer";
            Text = "Kinect Server";
            Load += KinectServer_Load;
            grpLog.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private ListBox lstLog;
        private GroupBox grpLog;
    }
}

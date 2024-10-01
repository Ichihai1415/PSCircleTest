namespace PSCircleTest
{
    partial class Form1
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
            components = new System.ComponentModel.Container();
            AutoExe = new System.Windows.Forms.Timer(components);
            PicBox = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)PicBox).BeginInit();
            SuspendLayout();
            // 
            // AutoExe
            // 
            AutoExe.Interval = 500;
            AutoExe.Tick += AutoExe_Tick;
            // 
            // PicBox
            // 
            PicBox.BackgroundImageLayout = ImageLayout.Zoom;
            PicBox.Dock = DockStyle.Fill;
            PicBox.Location = new Point(0, 0);
            PicBox.Name = "PicBox";
            PicBox.Size = new Size(1000, 1000);
            PicBox.TabIndex = 0;
            PicBox.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackgroundImageLayout = ImageLayout.Zoom;
            ClientSize = new Size(1000, 1000);
            Controls.Add(PicBox);
            Name = "Form1";
            Text = "距離マップ";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)PicBox).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Timer AutoExe;
        private PictureBox PicBox;
    }
}

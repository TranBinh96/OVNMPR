namespace PDFV5_forWin_for.UserControl
{
    partial class XtraUserCameraView
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pb_cam1 = new OpenCvSharp.UserInterface.PictureBoxIpl();
            this.groupControl2 = new DevExpress.XtraEditors.GroupControl();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnConvertCamera = new System.Windows.Forms.Button();
            this.btnConform = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.la_joutai = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.pb_cam1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).BeginInit();
            this.groupControl2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // pb_cam1
            // 
            this.pb_cam1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pb_cam1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pb_cam1.Location = new System.Drawing.Point(0, 0);
            this.pb_cam1.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.pb_cam1.Name = "pb_cam1";
            this.pb_cam1.Size = new System.Drawing.Size(988, 485);
            this.pb_cam1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pb_cam1.TabIndex = 1;
            this.pb_cam1.TabStop = false;
            // 
            // groupControl2
            // 
            this.groupControl2.Controls.Add(this.panel1);
            this.groupControl2.Controls.Add(this.label1);
            this.groupControl2.Controls.Add(this.la_joutai);
            this.groupControl2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.groupControl2.Location = new System.Drawing.Point(0, 485);
            this.groupControl2.Name = "groupControl2";
            this.groupControl2.Size = new System.Drawing.Size(988, 94);
            this.groupControl2.TabIndex = 2;
            this.groupControl2.Text = "Control";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnConvertCamera);
            this.panel1.Controls.Add(this.btnConform);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(459, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(369, 69);
            this.panel1.TabIndex = 19;
            // 
            // btnConvertCamera
            // 
            this.btnConvertCamera.Font = new System.Drawing.Font("Times New Roman", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConvertCamera.Location = new System.Drawing.Point(327, 3);
            this.btnConvertCamera.Name = "btnConvertCamera";
            this.btnConvertCamera.Size = new System.Drawing.Size(155, 53);
            this.btnConvertCamera.TabIndex = 19;
            this.btnConvertCamera.Text = "Đổi Camera";
            this.btnConvertCamera.UseVisualStyleBackColor = true;
            this.btnConvertCamera.Click += new System.EventHandler(this.btnConvertCamera_Click_1);
            // 
            // btnConform
            // 
            this.btnConform.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConform.Location = new System.Drawing.Point(7, 3);
            this.btnConform.Name = "btnConform";
            this.btnConform.Size = new System.Drawing.Size(316, 53);
            this.btnConform.TabIndex = 20;
            this.btnConform.Text = "XÁC NHẬN CAMERA";
            this.btnConform.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(2, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(457, 69);
            this.label1.TabIndex = 17;
            this.label1.Text = "Time Start";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // la_joutai
            // 
            this.la_joutai.Dock = System.Windows.Forms.DockStyle.Right;
            this.la_joutai.Font = new System.Drawing.Font("Times New Roman", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.la_joutai.Location = new System.Drawing.Point(828, 23);
            this.la_joutai.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.la_joutai.Name = "la_joutai";
            this.la_joutai.Size = new System.Drawing.Size(158, 69);
            this.la_joutai.TabIndex = 18;
            this.la_joutai.Text = "Recording 〇〇";
            this.la_joutai.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.pb_cam1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(988, 485);
            this.panel2.TabIndex = 2;
            // 
            // XtraUserCameraView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.groupControl2);
            this.Name = "XtraUserCameraView";
            this.Size = new System.Drawing.Size(988, 579);
            ((System.ComponentModel.ISupportInitialize)(this.pb_cam1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupControl2)).EndInit();
            this.groupControl2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private OpenCvSharp.UserInterface.PictureBoxIpl pb_cam1;
        private DevExpress.XtraEditors.GroupControl groupControl2;
        private System.Windows.Forms.Button btnConform;
        private System.Windows.Forms.Button btnConvertCamera;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label la_joutai;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
    }
}

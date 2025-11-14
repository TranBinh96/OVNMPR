namespace PDFV5_forWin_for.FormControl
{
    partial class Xtra_TemplateMarchine
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Xtra_TemplateMarchine));
            this.label1 = new System.Windows.Forms.Label();
            this.lb_information = new System.Windows.Forms.Label();
            this.txtTemplateMarchine = new DevExpress.XtraEditors.TextEdit();
            this.btnReset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.txtTemplateMarchine.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Tahoma", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(152, 89);
            this.label1.TabIndex = 1;
            this.label1.Text = "Mã code công dụng cụ";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_information
            // 
            this.lb_information.BackColor = System.Drawing.Color.Salmon;
            this.lb_information.Font = new System.Drawing.Font("Tahoma", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lb_information.ForeColor = System.Drawing.Color.White;
            this.lb_information.Location = new System.Drawing.Point(36, 126);
            this.lb_information.Name = "lb_information";
            this.lb_information.Size = new System.Drawing.Size(507, 70);
            this.lb_information.TabIndex = 1;
            this.lb_information.Text = "Mã code công dụng cụ";
            this.lb_information.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtTemplateMarchine
            // 
            this.txtTemplateMarchine.EditValue = "dsfdsfdsf";
            this.txtTemplateMarchine.Location = new System.Drawing.Point(172, 34);
            this.txtTemplateMarchine.Name = "txtTemplateMarchine";
            this.txtTemplateMarchine.Properties.Appearance.Font = new System.Drawing.Font("Tahoma", 33.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTemplateMarchine.Properties.Appearance.Options.UseFont = true;
            this.txtTemplateMarchine.Size = new System.Drawing.Size(246, 60);
            this.txtTemplateMarchine.TabIndex = 3;
            // 
            // btnReset
            // 
            this.btnReset.Font = new System.Drawing.Font("Tahoma", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReset.Location = new System.Drawing.Point(424, 32);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(146, 62);
            this.btnReset.TabIndex = 2;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // Xtra_TemplateMarchine
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(580, 205);
            this.ControlBox = false;
            this.Controls.Add(this.txtTemplateMarchine);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.lb_information);
            this.Controls.Add(this.label1);
            this.IconOptions.SvgImage = ((DevExpress.Utils.Svg.SvgImage)(resources.GetObject("Xtra_TemplateMarchine.IconOptions.SvgImage")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Xtra_TemplateMarchine";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Xác nhận sử dụng công dụng cụ";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.txtTemplateMarchine.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lb_information;
        private DevExpress.XtraEditors.TextEdit txtTemplateMarchine;
        private System.Windows.Forms.Button btnReset;
    }
}
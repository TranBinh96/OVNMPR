using DevExpress.XtraEditors;
using System;
using System.IO;
using System.Windows.Forms;

namespace PDFV5_forWin_for.FormControl
{
    public partial class Xtra_TemplateMarchine : XtraForm
    {
        public string EnteredValue { get; private set; }
        private readonly string _templateMachine;

        public Xtra_TemplateMarchine(string templateMachine)
        {
            InitializeComponent();
            _templateMachine = templateMachine;
            this.Shown += Xtra_TemplateMarchine_Shown;
            this.txtTemplateMarchine.KeyDown += TxtTemplateMarchine_KeyDown;
        }

        private void Xtra_TemplateMarchine_Shown(object sender, EventArgs e)
        {
            txtTemplateMarchine.Text = string.Empty;
            txtTemplateMarchine.Properties.Appearance.Options.UseTextOptions = true;
            txtTemplateMarchine.Properties.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            txtTemplateMarchine.Properties.AutoHeight = false;
            txtTemplateMarchine.Focus();

            lb_information.Text = "✔ Vui lòng quét hoặc nhập mã dụng cụ đúng.";
            lb_information.BackColor = System.Drawing.Color.FromArgb(80, 180, 80);
            lb_information.ForeColor = System.Drawing.Color.White;

        }

        private void TxtTemplateMarchine_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode != Keys.Enter) return;
                string input = txtTemplateMarchine.Text.Trim();

                if (string.Equals(input, _templateMachine, StringComparison.OrdinalIgnoreCase))
                {
                    lb_information.Text = "✅ Mã đúng — xử lý thành công!";
                    lb_information.BackColor = System.Drawing.Color.Green;
                    EnteredValue = input;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lb_information.Text = "❌ Sai dụng cụ — vui lòng kiểm tra lại!";
                    lb_information.BackColor = System.Drawing.Color.Red;
                    txtTemplateMarchine.Text = string.Empty;
                    txtTemplateMarchine.Focus();
                }
            }
            catch (Exception ex)
            {
                LogError("TxtTemplateMarchine_KeyDown", ex);
            }
        }

        private void LogError(string method, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, "runtime_error.txt");
                string message = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{method}] {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, message);
            }
            catch { }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtTemplateMarchine.Text = string.Empty;
            txtTemplateMarchine.Focus();
        }
    }
}

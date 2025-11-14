using DevExpress.XtraEditors;
using OVNMRepository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OVNMPR.FormControl
{

    public partial class XtraFormLogin : DevExpress.XtraEditors.XtraForm
    {
        readonly private UnitControl unitControl = new UnitControl();
        public XtraFormLogin()
        {
            InitializeComponent();

            if (Properties.Settings.Default.checkKeepSign == true)
            {
                checkKeepSign.Checked = true;
                txtUser.Text = Properties.Settings.Default.username;
                txtPass.Text = Properties.Settings.Default.password;
            }
            else
            {
                checkKeepSign.Checked = false;
                txtUser.Text = "";
                txtPass.Text = "";
            }   

        }


        public void AutoLogin()
        {
            Users user = new Users()
            {
                UserName = txtUser.Text.Trim(),
                PasswordHash = txtPass.Text.Trim()
            };

            // Gọi login
            string roleAdmin = unitControl.users.Login(user.UserName, user.PasswordHash, out string role);

            if (roleAdmin == null)
            {
                MessageBox.Show("Tên đăng nhập hoặc mật khẩu không đúng!",
                    "Đăng nhập thất bại",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // Ghi nhớ trạng thái đăng nhập
            if (checkKeepSign.Checked)
            {
                Properties.Settings.Default.checkKeepSign = true;
                Properties.Settings.Default.username = txtUser.Text.Trim();
                Properties.Settings.Default.password = txtPass.Text.Trim();
            }
            else
            {
                Properties.Settings.Default.checkKeepSign = false;
            }

            // Ghi nhớ quyền Admin
            Properties.Settings.Default.isAdmin = (roleAdmin == "Admin");

            // Lưu setting
            Properties.Settings.Default.Save();

            // Mở form chính
            RbMain rbMain = new RbMain();
            rbMain.Show();

            // Ẩn form login
            this.Hide();
        }

        private void btnSignIn_Click(object sender, EventArgs e)
        {
            AutoLogin();
        }


        

        private void txtPassword_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void txtPass_Properties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // 👉 Xử lý khi nhấn Enter
                AutoLogin();
            }
        }
    }
}
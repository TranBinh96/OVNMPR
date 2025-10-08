using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using OVNMPR.UserControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OVNMPR
{
    public partial class RbMain : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        public RbMain()
        {
            InitializeComponent();
        }


        private readonly Dictionary<string, XtraUserControl> _ucCache = new Dictionary<string, XtraUserControl>();

        private void ShowUserControl(string key, Func<XtraUserControl> factory)
        {
            panelMain.Controls.Clear();

            if (!_ucCache.ContainsKey(key))
            {
                var uc = factory();
                uc.Dock = System.Windows.Forms.DockStyle.Fill;
                _ucCache[key] = uc;
            }

            panelMain.Controls.Add(_ucCache[key]);
        }

        private void btnSelectComputerLine_ItemClick(object sender, ItemClickEventArgs e)
        {
            ShowUserControl("ComputerLine", () => new XtraUserComputerLine());
        }

        private void btnSelectVideoPasKit_ItemClick(object sender, ItemClickEventArgs e)
        {
            
           ShowUserControl("VideoPasKit", () => new XtraUserVideoKITPAS());
        }
    }
}
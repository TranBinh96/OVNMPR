using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Grid;
using ModifileDataOptas.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModifileDataOptas
{
    public partial class XtraFormMain : DevExpress.XtraEditors.XtraForm
    {
        private static ProductionReport productionReport = new ProductionReport();  
        public XtraFormMain()
        {
            InitializeComponent();
        }

        private void XtraFormMain_Load(object sender, EventArgs e)
        {
            
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var lstproductionReport = productionReport.GetProductionReports(txtKariID.Text, txtSeiban.Text);
            gridControlData.DataSource = lstproductionReport;

            txtSumKariID.Text= lstproductionReport.Select(x => x.KariID).Distinct().Count().ToString();
            txtSumStart.Text = lstproductionReport.Count(r => !string.IsNullOrEmpty(r.Start)).ToString();
            txtSumEnd.Text = lstproductionReport.Count(r => !string.IsNullOrEmpty(r.End)).ToString();


            // Lấy đối tượng GridView từ MainView của GridControl
            DevExpress.XtraGrid.Views.Grid.GridView gridView = gridControlData.MainView as DevExpress.XtraGrid.Views.Grid.GridView;

            if (gridView != null)
            {
                gridView.ExpandAllGroups();

                // Hoặc nếu bạn muốn dùng tên biến 'gridViewData' và nó đã được khai báo ở nơi khác:
                // Đảm bảo rằng gridViewData thực sự được khai báo là: 
                // DevExpress.XtraGrid.Views.Grid.GridView gridViewData;
            }
        }
    }
}
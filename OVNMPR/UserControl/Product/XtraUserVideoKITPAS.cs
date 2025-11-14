using DevExpress.XtraBars;
using DevExpress.XtraEditors;
using OVNMPR.UserControl.Helpers;
using OVNMRepository.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OVNMPR.UserControl
{

    public partial class XtraUserVideoKITPAS : DevExpress.XtraEditors.XtraUserControl
    {
        private static UnitControl unitControl = new UnitControl();
        private List<ComputerLine> dataComputerLine = new List<ComputerLine>();
        DevExpress.XtraBars.PopupMenu popupMenu;
        DevExpress.XtraBars.BarManager barManager;
        public XtraUserVideoKITPAS()
        {
            InitializeComponent();
            LoadData();
            var menuHelper = new GridViewMenuHelper(this.FindForm());

            menuHelper.CreateMenu(gridViewlVideoPas, new Dictionary<string, Action<DevExpress.XtraGrid.Views.Grid.GridView>>
            {
                { "Watch Video", gv => {
                    var row = gv.GetFocusedRow() as PasVideo;
                    if (row != null && !string.IsNullOrEmpty(row.AddressVideo))
                    {
                        if (System.IO.File.Exists(row.AddressVideo))
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = row.AddressVideo,
                                UseShellExecute = true
                            });
                        }
                        else
                        {
                            MessageBox.Show($"Không tìm thấy file AVI: {row.AddressVideo}");
                        }
                    }
                }},
                { "Open Folder", gv => {
                    var row = gv.GetFocusedRow() as PasVideo;
                    if (row != null && !string.IsNullOrEmpty(row.AddressVideo))
                    {
                        if (System.IO.File.Exists(row.AddressVideo))
                        {
                            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{row.AddressVideo}\"");
                        }
                        else
                        {
                            MessageBox.Show($"Không tìm thấy file: {row.AddressVideo}");
                        }
                    }
                }}
            });
        }

        private void CreateMenu(
            DevExpress.XtraGrid.Views.Grid.GridView gridView,
            Dictionary<string, Action<DevExpress.XtraGrid.Views.Grid.GridView>> menuItems)
        {
            var barManager = new DevExpress.XtraBars.BarManager();
            barManager.Form = this.FindForm();

            var popupMenu = new DevExpress.XtraBars.PopupMenu(barManager);

            foreach (var kvp in menuItems)
            {
                var item = new DevExpress.XtraBars.BarButtonItem(barManager, kvp.Key);
                popupMenu.AddItem(item);

                // Gắn sự kiện click
                item.ItemClick += (s, e) =>
                {
                    kvp.Value(gridView); // gọi callback, truyền vào gridView
                };
            }

            // Bắt sự kiện chuột phải
            gridView.PopupMenuShowing += (s, e) =>
            {
                if (e.HitInfo.InRow || e.HitInfo.InGroupRow)
                {
                    gridView.FocusedRowHandle = e.HitInfo.RowHandle;
                    popupMenu.ShowPopup(Control.MousePosition);
                }
            };
        }
        private void LoadData()
        {
            // Hôm qua
            DateTime startTime = DateTime.Today.AddDays(-1);              // 00:00 hôm qua
            DateTime endTime = DateTime.Today.AddSeconds(-1);           // 23:59:59 hôm qua

            var lstdata = unitControl.pasVideo.GetPasVideo(
                startTime: startTime,
                endTime: endTime
            );
            gridControlVideoPas.DataSource = lstdata;


            dataComputerLine = unitControl.computerLine.GetComputerLines().ToList();

            // Lấy danh sách HostName
            List<string> hostNames = dataComputerLine
                .Select(x => x.HostName) // hoặc x.HostName tùy đúng property
                .ToList();
            // Loại bỏ các giá trị null hoặc rỗng và lấy các giá trị duy nhất
            List<string> Stations = dataComputerLine
                .Select(x => x.Station).Where(h => !string.IsNullOrEmpty(h))
                .Distinct()
                .ToList();
            // Lấy danh sách HostName
            cmbHostName.Properties.Items.Clear();
            cmbHostName.Properties.Items.AddRange(hostNames);

            // chọn mặc định dòng đầu tiên nếu có
            if (cmbHostName.Properties.Items.Count > 0)
                cmbHostName.SelectedIndex = -1;
            // Lấy danh sách Station
            cmbLocationLine.Properties.Items.Clear();
            cmbLocationLine.Properties.Items.AddRange(Stations);

            // chọn mặc định dòng đầu tiên nếu có
            if (cmbLocationLine.Properties.Items.Count > 0)
                cmbLocationLine.SelectedIndex = -1;

        }

        private void cmbLocationLine_Properties_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedLocation = cmbLocationLine.SelectedItem as string;
            if (string.IsNullOrEmpty(selectedLocation)) return;

            // Lấy tất cả hostnames thuộc line được chọn
            var hostNames = dataComputerLine
                .Where(x => x.Station == selectedLocation)
                .Select(x => x.HostName)
                .Where(h => !string.IsNullOrEmpty(h))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Gán vào ComboBoxEdit
            cmbHostName.Properties.Items.Clear();
            cmbHostName.Properties.Items.AddRange(hostNames);

            // Chọn mặc định item đầu tiên nếu có
            if (cmbHostName.Properties.Items.Count > 0)
                cmbHostName.SelectedIndex = 0;
        }

        private void cmbHostName_Properties_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cmbHostName_Properties_SelectedValueChanged(object sender, EventArgs e)
        {
            List<string> hostNames = dataComputerLine
               .Select(x => x.HostName) // hoặc x.HostName tùy đúng property
               .ToList();
            // Lấy danh sách HostName
            cmbHostName.Properties.Items.Clear();
            cmbHostName.Properties.Items.AddRange(hostNames);

        }


        public void AutoSearch()
        {
            try
            {
                gridViewlVideoPas.ClearGrouping();
                gridControlVideoPas.DataSource = null;
                string selectedHostName = string.IsNullOrWhiteSpace(cmbHostName.SelectedItem as string) ? null : cmbHostName.SelectedItem as string;
                string selectedLocation = string.IsNullOrWhiteSpace(cmbLocationLine.SelectedItem as string) ? null : cmbLocationLine.SelectedItem as string;
                string kariID = string.IsNullOrWhiteSpace(txtKariID.Text) ? null : txtKariID.Text.Trim();
                string station = string.IsNullOrWhiteSpace(txtStation.Text) ? null : txtStation.Text.Trim();

                // kiểm tra ngày bắt đầu
                // kiểm tra ngày bắt đầu
                DateTime? startDate = null;
                if (dtStartTime.EditValue != null && DateTime.TryParse(dtStartTime.EditValue.ToString(), out DateTime sDate))
                {
                    startDate = sDate.Date; // 00:00:00
                }

                // kiểm tra ngày kết thúc
                DateTime? endDate = null;
                if (dtEndTime.EditValue != null && DateTime.TryParse(dtEndTime.EditValue.ToString(), out DateTime eDate))
                {
                    endDate = eDate.Date.AddDays(1).AddSeconds(-1); // 23:59:59
                }

                // fallback nếu chưa chọn ngày
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    startDate = DateTime.Today.AddDays(-10);                     // 365 ngày trước, từ 00:00:00
                    endDate = DateTime.Today.AddDays(1).AddSeconds(-1);         // hôm nay đến 23:59:59
                }

                // Gọi SP
                var lstdata = unitControl.pasVideo.GetPasVideo(
                    hostname: selectedHostName,
                    kariId: kariID,
                    station: string.IsNullOrEmpty(station) ? (int?)null : int.Parse(station),
                    startTime: startDate,
                    endTime: endDate
                );

                // ví dụ gán vào grid
                gridControlVideoPas.DataSource = SanitizePasVideoList(lstdata);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tìm kiếm: " + ex.Message);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            AutoSearch();
        }


        private List<PasVideo> SanitizePasVideoList(List<PasVideo> source)
        {
            if (source == null) return new List<PasVideo>();

            foreach (var item in source)
            {
                // fix null string
                item.KariId = item.KariId ?? string.Empty;
                item.Hostname = item.Hostname ?? string.Empty;
                item.AddressVideo = item.AddressVideo ?? string.Empty;
                item.FileName = item.FileName ?? string.Empty;

                // Station nếu null => gán 0
                if (item.Station == 0 && item.Station == null)
                    item.Station = 0;

                // StartTime nếu null => gán DateTime.MinValue
                if (item.StartTime == default(DateTime))
                    item.StartTime = DateTime.MinValue;
            }

            return source;
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtKariID.Text = "";
            txtStation.Text = "";   
            cmbHostName.SelectedIndex = -1; 
            cmbLocationLine.SelectedIndex = -1; 
            dtEndTime.EditValue = null; 
            dtStartTime.EditValue = null;
            gridControlVideoPas.DataSource = null;  

        }

        private void cmbHostName_Properties_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AutoSearch();   
            }
        }
    }
}

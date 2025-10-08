using DevExpress.Export;
using DevExpress.XtraEditors;
using DevExpress.XtraPrinting;
using OVNMRepository.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace OVNMPR.UserControl
{
    public partial class XtraUserComputerLine : DevExpress.XtraEditors.XtraUserControl
    {
        private static UnitControl unitControl = new UnitControl();
        private static ComputerLine selectedComputerLine = null;
        private List<ComputerLine> dataComputerLine = new List<ComputerLine>();

        public XtraUserComputerLine()
        {
            InitializeComponent();
            LoadData();
            if (Properties.Settings.Default.isAdmin)
            {
                btnDelete.Enabled = false;  
                btnSave.Enabled = false;  
                btnDelete.Enabled = false;
                btnReset.Enabled = false;
                txtSerinumber.ReadOnly = true;
                txtHostName.ReadOnly = true;


            }


        }

        private void LoadData()
        {
            try
            {
                var data = unitControl.computerLine.GetComputerLines().ToList();
                gridControlComputerLine.DataSource = data;


                dataComputerLine = unitControl.computerLine.GetComputerLines().ToList();

                // Lấy danh sách LineName
                List<string> Lines = dataComputerLine
                    .Select(x => x.LineName).Distinct() // hoặc x.LineName tùy đúng property
                    .ToList();
                // Loại bỏ các giá trị null hoặc rỗng và lấy các giá trị duy nhất
                List<string> Units = dataComputerLine
                    .Select(x => x.UnitName).Where(h => !string.IsNullOrEmpty(h))
                    .Distinct()
                    .ToList();
                // Lấy danh sách Units
                cmbLineName.Properties.Items.Clear();
                cmbLineName.Properties.Items.AddRange(Lines);

                // chọn mặc định dòng đầu tiên nếu có
                if (cmbUnitName.Properties.Items.Count > 0)
                    cmbUnitName.SelectedIndex = -1;

                // Lấy danh sách Units
                cmbUnitName.Properties.Items.Clear();
                cmbUnitName.Properties.Items.AddRange(Units);

                // chọn mặc định dòng đầu tiên nếu có
                if (cmbUnitName.Properties.Items.Count > 0)
                    cmbUnitName.SelectedIndex = -1;


                gridViewComputerLine.ExpandAllGroups();

            }
            catch (Exception ex)
            {
                XtraMessageBox.Show($"Lỗi tải dữ liệu: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridViewComputerLine_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            var gv = sender as DevExpress.XtraGrid.Views.Grid.GridView;
            if (gv == null || e.FocusedRowHandle < 0) return;

            // cách 1: lấy object model trực tiếp
            var row = gv.GetFocusedRow() as ComputerLine;
            if (row != null)
            {
                // Gán vào biến static (nếu cần dùng bên ngoài)
                selectedComputerLine = new ComputerLine
                {
                    STT = row.STT,
                    HostName = row.HostName,
                    Station = row.Station,
                    LineName = row.LineName,
                    UnitName = row.UnitName,
                    AddressIP = row.AddressIP,
                    Information = row.Information,
                    UpdateDate = row.UpdateDate,
                    SerialNumber = row.SerialNumber,

                };

                // Đẩy dữ liệu ra control UI
                txtHostName.Text = row.HostName ?? "";
                txtStation.Text = row.Station ?? "";
                cmbLineName.Text = row.LineName ?? "";
                cmbUnitName.Text = row.UnitName ?? "";
                txtAddressIP.Text = row.AddressIP ?? "";
                txtSerinumber.Text = row.SerialNumber ?? "";
            }
            else
            {
                ClearDetail();
            }
        }

        private void ClearDetail()
        {
            txtHostName.Text = "";
            txtStation.Text = "";
            cmbLineName.Text = "";
            cmbUnitName.Text = "";
            txtAddressIP.Text = "";
            txtSerinumber.Text = "";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var computerLine = new ComputerLine
            {
                HostName = txtHostName.Text.Trim(),
                Station = txtStation.Text.Trim(),
                LineName = cmbLineName.Text.Trim(),
                UnitName = cmbUnitName.Text.Trim(),
                AddressIP = txtAddressIP.Text.Trim(),
                SerialNumber = txtSerinumber.Text.Trim(),
                UpdateDate = DateTime.Now
            };

            bool success = unitControl.computerLine.UpsertComputerLine(computerLine);

            if (success)
            {
                XtraMessageBox.Show("Cập nhật thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Lưu lại key để tìm row
                string keyHost = computerLine.HostName;
                string keySerial = computerLine.SerialNumber;

                // Reload lại dữ liệu
                LoadData();
                ClearDetail();

                // Sau khi reload, tìm row vừa cập nhật
                var gv = gridViewComputerLine;
                for (int i = 0; i < gv.RowCount; i++)
                {
                    var row = gv.GetRow(i) as ComputerLine;
                    if (row != null &&
                        row.HostName == keyHost &&
                        row.SerialNumber == keySerial)
                    {
                        gv.FocusedRowHandle = i; // focus về đúng row
                        gv.SelectRow(i);         // chọn row
                        break;
                    }
                }
            }
            else
            {
                XtraMessageBox.Show("Không có thay đổi nào được lưu!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string serialNumber = txtSerinumber.Text.Trim();
            string hostName = txtHostName.Text.Trim();

            if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(hostName))
            {
                XtraMessageBox.Show("Vui lòng chọn dòng dữ liệu cần xóa.", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Hỏi xác nhận
            var confirm = XtraMessageBox.Show(
                $"Bạn có chắc chắn muốn xóa ComputerLine:\nSerial: {serialNumber}\nHostName: {hostName} ?",
                "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    bool result = unitControl.computerLine.DeleteComputerLine(serialNumber, hostName);

                    if (result)
                    {
                        XtraMessageBox.Show("Xóa thành công.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Load lại dữ liệu grid
                        LoadData();
                        ClearDetail();
                    }
                    else
                    {
                        XtraMessageBox.Show("Không tìm thấy bản ghi để xóa.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi khi xóa: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            string serialNumber = txtSerinumber.Text.Trim();
            string hostName = txtHostName.Text.Trim();

            if (string.IsNullOrEmpty(serialNumber) || string.IsNullOrEmpty(hostName))
            {
                XtraMessageBox.Show("Vui lòng nhập SerialNumber và HostName",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = XtraMessageBox.Show(
                $"Bạn có chắc chắn muốn reset về mặc định cho:\nSerial: {serialNumber}\nHostName: {hostName}?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    bool result = unitControl.computerLine.ResetDefaultComputerLine(serialNumber, hostName);
                    if (result)
                    {
                        XtraMessageBox.Show("Reset thành công!", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearDetail();
                    }
                    else
                    {
                        XtraMessageBox.Show("Không tìm thấy bản ghi để reset.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("Lỗi khi reset: " + ex.Message, "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearDetail();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            var computerLine = new ComputerLine
            {
                HostName = txtHostName.Text.Trim(),
                Station = txtStation.Text.Trim(),
                LineName = cmbLineName.Text.Trim(),
                UnitName = cmbUnitName.Text.Trim(),
                AddressIP = txtAddressIP.Text.Trim(),
                SerialNumber = txtSerinumber.Text.Trim(),
                UpdateDate = DateTime.Now
            };

            var data = unitControl.computerLine.GetComputerLines().ToList();


            var results = data.Where(x =>
                (string.IsNullOrEmpty(computerLine.HostName) || x.HostName.Equals(computerLine.HostName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(computerLine.Station) || x.Station.Equals(computerLine.Station, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(computerLine.LineName) || x.LineName.Equals(computerLine.LineName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(computerLine.UnitName) || x.UnitName.Equals(computerLine.UnitName, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(computerLine.AddressIP) || x.AddressIP.Equals(computerLine.AddressIP, StringComparison.OrdinalIgnoreCase)) &&
                (string.IsNullOrEmpty(computerLine.SerialNumber) || x.SerialNumber.Equals(computerLine.SerialNumber, StringComparison.OrdinalIgnoreCase))
            ).ToList();

            gridControlComputerLine.DataSource = results;
            gridViewComputerLine.ExpandAllGroups();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            //using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            //{
            //    saveFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";
            //    saveFileDialog.Title = "Xuất dữ liệu ra Excel";
            //    saveFileDialog.FileName = "Export_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";

            //    if (saveFileDialog.ShowDialog() == DialogResult.OK)
            //    {
            //        // 👉 Bỏ group trong GridView trước khi export
            //        gridViewComputerLine.ClearGrouping();
            //        gridViewComputerLine.OptionsView.ShowGroupPanel = false;

            //        // Ẩn tạm 2 cột trước khi export
            //        var colInfo = gridViewComputerLine.Columns["Information"];
            //        var colUpdate = gridViewComputerLine.Columns["UpdateDate"];
            //        bool infoVisible = colInfo.Visible;
            //        bool updateVisible = colUpdate.Visible;

            //        colInfo.Visible = false;
            //        colUpdate.Visible = false;

            //        // Cấu hình export
            //        var options = new XlsxExportOptionsEx
            //        {
            //            ExportType = ExportType.WYSIWYG, // xuất giữ nguyên format
            //            AllowGrouping = DevExpress.Utils.DefaultBoolean.False, // ❌ không xuất group
            //            ShowGroupSummaries = DevExpress.Utils.DefaultBoolean.False, // ❌ không xuất tổng group
            //            SheetName = "ComputerLine"
            //        };

            //        // Export ra Excel
            //        gridViewComputerLine.ExportToXlsx(saveFileDialog.FileName, options);

            //        // Khôi phục lại hiển thị cột
            //        colInfo.Visible = infoVisible;
            //        colUpdate.Visible = updateVisible;

            //        MessageBox.Show("Xuất Excel thành công!\nFile: " + saveFileDialog.FileName,
            //                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //        Process.Start(saveFileDialog.FileName);
            //    }
            //}
        }

    }
}
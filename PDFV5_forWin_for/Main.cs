using DevExpress.XtraEditors.SyntaxEditor;
using PdfiumViewer;
using PDFV5_forWin_for.Initializer;
using PDFV5_forWin_for.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PDFV5_forWin_for
{
    public partial class Main : DevExpress.XtraEditors.XtraForm
    {
        // -------------------------------
        // 🧱 BIẾN THÀNH VIÊN TOÀN CỤC
        // -------------------------------
        private PdfDocument _pdfDocument;   // Lưu trữ tài liệu PDF đang mở
        private int _currentPage = 0;       // Trang PDF hiện tại
        private string _pdfPath = string.Empty; // Đường dẫn file PDF
        private List<String> pdfFiles = null;   //  // Lấy toàn bộ danh sách file PDF
        private StationName StationName = null;

        // Timer để cập nhật
        private static Timer timerClock;    // Timer để cập nhật thời gian thực (hiển thị trên Label)

        // -------------------------------
        // ⚙️ KHỞI TẠO FORM CHÍNH
        // -------------------------------
        public Main()
        {
            InitializeComponent(); // Khởi tạo các control trên form
        }


        // -------------------------------
        // 🪟 SỰ KIỆN FORM LOAD
        // -------------------------------
        private void Main_Load(object sender, EventArgs e)
        {

            ValidateInputs();
            //Khởi tạo Even
            ActionEvent();

            // Đường dẫn PDF mặc định khi form khởi động
            _pdfPath = "5326_06_Beginner";


            string latestFile = AppInitializer.GetLatestPdfByKeyword(_pdfPath);
            // Gọi hàm load PDF và hiển thị trang đầu tiên
            LoadPdf(latestFile);
            ShowPage(0);

            // Bắt đầu khởi động đồng hồ thời gian thực
            InitClock();
        }

        private void ActionEvent()
        {
            tbUnitID.TextChanged += (s, ev) => ValidateInputs();
            tbWorkerID.TextChanged += (s, ev) => ValidateInputs();
            cbStation.SelectedIndexChanged += (s, ev) => ValidateInputs();
            cbProficiency.SelectedIndexChanged += (s, ev) => ValidateInputs();
        }



        private bool ValidateInputs()
        {
           
            dxErrorProvider1.ClearErrors(); // Xóa lỗi cũ

            // 🧱 Tạo đối tượng từ dữ liệu nhập
            StationName = new StationName
            {
                UnitID = tbUnitID.Text?.Trim(),
                KariID = tbWorkerID.Text?.Trim(),
                Station = cbStation.Text?.Trim(),
                Proficiency = cbProficiency.Text?.Trim(),
                Digits =  10
            };

            // ⚙️ Gọi Validate logic trong model
            if (!StationName.Validate(out string error))
            {
                lblStatus.Text = $"❌ {error}";
                lblStatus.ForeColor = Color.Red;

                // 🚨 Hiển thị lỗi cụ thể tại control tương ứng
                if (string.IsNullOrEmpty(StationName.UnitID))
                    dxErrorProvider1.SetError(tbUnitID, "UnitID không được để trống.");
                else if (StationName.UnitID.Length != StationName.Digits)
                    dxErrorProvider1.SetError(tbUnitID, $"UnitID phải có {StationName.Digits} ký tự.");
                else if (!StationName.UnitID.All(char.IsDigit))
                    dxErrorProvider1.SetError(tbUnitID, "UnitID chỉ được phép chứa số.");

                if (string.IsNullOrWhiteSpace(StationName.KariID))
                    dxErrorProvider1.SetError(tbWorkerID, "WorkerID không được để trống.");
                else if (StationName.KariID.Length < 7)
                    dxErrorProvider1.SetError(tbWorkerID, "WorkerID phải có đủ 7 ký tự.");

                if (string.IsNullOrWhiteSpace(StationName.Station))
                    dxErrorProvider1.SetError(cbStation, "Station không được để trống.");

                if (string.IsNullOrWhiteSpace(StationName.Proficiency))
                    dxErrorProvider1.SetError(cbProficiency, "Proficiency không được để trống.");
        
                return false;
            }

            // ✅ Dữ liệu hợp lệ
            lblStatus.Text = $"✅ Dữ liệu hợp lệ: {StationName.UnitID}";
            lblStatus.ForeColor = Color.Green;

            return true;
        }




        // -------------------------------
        // 📂 HÀM LOAD FILE PDF
        // -------------------------------
        private void LoadPdf(string path)
        {
            // Giải phóng tài nguyên PDF cũ nếu có
            _pdfDocument?.Dispose();

            // Nạp PDF mới
            _pdfDocument = PdfDocument.Load(path);

            // Hiển thị thông tin lên tiêu đề form
            this.Text = $"PDF Loaded: {_pdfDocument.PageCount} trang";
        }

        // -------------------------------
        // 🖼️ HÀM HIỂN THỊ TRANG PDF
        // -------------------------------
        private void ShowPage(int pageIndex)
        {
            // Kiểm tra hợp lệ
            if (_pdfDocument == null) return;
            if (pageIndex < 0 || pageIndex >= _pdfDocument.PageCount) return;

            // Giải phóng ảnh cũ nếu có
            im_pdf.Image?.Dispose();
            im_pdf.Image = null;

            // Render PDF ra ảnh bitmap với độ phân giải cao
            const int dpi = 300; // DPI cao giúp ảnh nét hơn
            using (var bmp = _pdfDocument.Render(pageIndex, dpi, dpi, true))
            {
                im_pdf.Image = (Bitmap)bmp.Clone();
            }

            // Ghi nhớ trang hiện tại
            _currentPage = pageIndex;

            // Cập nhật tiêu đề form
            this.Text = $"Trang {_currentPage + 1}/{_pdfDocument.PageCount} - {dpi} DPI";
        }

        
        // -------------------------------
        // ⏭️ NÚT NEXT (Trang kế tiếp)
        // -------------------------------
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (_pdfDocument == null) return;

            // Nếu chưa đến trang cuối thì tăng +1
            if (_currentPage < _pdfDocument.PageCount - 1)
                ShowPage(_currentPage + 1);
        }

        // -------------------------------
        // ⏮️ NÚT BACK (Trang trước)
        // -------------------------------
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_pdfDocument == null) return;

            // Nếu chưa ở trang đầu thì lùi -1
            if (_currentPage > 0)
                ShowPage(_currentPage - 1);
        }





        // =========================================================
        // 🕒 TIMER CẬP NHẬT NGÀY GIỜ THỰC TẾ
        // =========================================================
        public void InitClock()
        {
            // Tạo timer mới, chạy mỗi 1 giây
            timerClock = new Timer();
            timerClock.Interval = 1000; // 1 giây

            // Gán sự kiện Tick cho timer
            timerClock.Tick += TimerClock_Tick;

            // Bắt đầu chạy timer
            timerClock.Start();
        }

        // ---------------------------------------------------------
        // ⏰ SỰ KIỆN KHI TIMER "TICK" (CỨ 1 GIÂY CHẠY MỘT LẦN)
        // ---------------------------------------------------------
        private void TimerClock_Tick(object sender, EventArgs e)
        {
            // 🟢 Cập nhật label thời gian trên form (nếu có label tên lblTime)
            // Ví dụ:
            // lblTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

            // Hoặc chỉ in ra Console để debug
            la_new_time.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        // =========================================================
        // 🧹 GIẢI PHÓNG TÀI NGUYÊN KHI ĐÓNG FORM
        // =========================================================
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Dừng timer nếu đang chạy
            timerClock?.Stop();

            // Giải phóng PDF nếu có
            _pdfDocument?.Dispose();
        }

        private void groupBox8_Enter(object sender, EventArgs e)
        {

        }
    }
}

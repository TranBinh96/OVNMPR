using PdfiumViewer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFV5_forWin_for
{
    public partial class Form1 : Form
    {
        private PdfDocument _pdfDocument;
        private int _currentPage = 0;
        private string _pdfPath = string.Empty;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            _pdfPath = @"C:\PDFviewer_ForWin_PDF\SR30_HSF_SE_2027_01_Beginner_VER001.pdf";
            LoadPdf(_pdfPath);
            ShowPage(0);
        }

        private void LoadPdf(string path)
        {
            // Giải phóng PDF cũ nếu có
            _pdfDocument?.Dispose();
            _pdfDocument = PdfDocument.Load(path);
            this.Text = $"PDF Loaded: {_pdfDocument.PageCount} trang";
        }

        private void ShowPage(int pageIndex)
        {
            if (_pdfDocument == null) return;
            if (pageIndex < 0 || pageIndex >= _pdfDocument.PageCount) return;

            // Giải phóng ảnh cũ nếu có
            im_pdf.Image?.Dispose();
            im_pdf.Image = null;

            // Render PDF theo DPI cao (200–300 DPI để rõ nét)
            const int dpi = 300;  // tăng lên 300 nếu vẫn mờ
            using (var bmp = _pdfDocument.Render(pageIndex, dpi, dpi, true))
            {
                im_pdf.Image = (Bitmap)bmp.Clone();
            }

            _currentPage = pageIndex;
            this.Text = $"Trang {_currentPage + 1}/{_pdfDocument.PageCount} - {dpi} DPI";
        }

    }
}

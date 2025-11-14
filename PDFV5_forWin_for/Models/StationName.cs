using System;
using System.Linq;

namespace PDFV5_forWin_for.Models
{
    public class StationName
    {
        public string UnitID { get; set; } = string.Empty;
        public string WorkerID { get; set; } = string.Empty;
        public string Station { get; set; } = string.Empty;
        public string Proficiency { get; set; } = string.Empty;
        public int Digits { get; set; } = 10;

        public StationName() { }

        /// <summary>
        /// Kiểm tra hợp lệ dữ liệu Line.
        /// (Chỉ xử lý logic, không liên quan giao diện)
        /// </summary>
        /// <param name="errorMessage">Trả về lỗi nếu có</param>
        /// <returns>True nếu hợp lệ, False nếu có lỗi</returns>
        public bool Validate(out string errorMessage)
        {
            // Chuẩn hoá dữ liệu
            UnitID = UnitID?.Trim() ?? "";
            WorkerID = WorkerID?.Trim() ?? "";
            Station = Station?.Trim() ?? "";
            Proficiency = Proficiency?.Trim() ?? "";

           

            // --- Kiểm tra WorkerID ---
            if (string.IsNullOrWhiteSpace(WorkerID))
            {
                errorMessage = "WorkerID không được để trống.";
                return false;
            }

            if (WorkerID.Length < 7)
            {
                errorMessage = "WorkerID phải có đủ 7 ký tự.";
                return false;
            }

            if (string.IsNullOrEmpty(Station))
            {
                errorMessage = "Station không được để trống.";
                return false;
            }

            if (string.IsNullOrEmpty(Proficiency))
            {
                errorMessage = "Proficiency không được để trống.";
                return false;
            }

            // --- Kiểm tra độ dài UnitID ---
            if (UnitID.Length != Digits)
            {
                errorMessage = $"UnitID phải có {Digits} ký tự.";
                return false;
            }

            // --- Kiểm tra chỉ chứa số ---
            if (!UnitID.All(char.IsDigit))
            {
                errorMessage = "UnitID chỉ được phép chứa ký tự số.";
                return false;
            }

            // ✅ Dữ liệu hợp lệ
            errorMessage = string.Empty;
            return true;
        }
    }
}

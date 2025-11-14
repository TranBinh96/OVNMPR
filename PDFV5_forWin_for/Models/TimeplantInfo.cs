using ClosedXML.Excel;
using Dapper;
using MySqlConnector;
using PDFV5_forWin_for.Initializer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace PDFV5_forWin_for.Models
{
    /// <summary>
    /// Đại diện cho thông tin kế hoạch sản xuất (Timeplan)
    /// </summary>
    public class TimeplantInfo
    {
        public string Line { get; set; }
        public string Unit { get; set; }
        public int Kari { get; set; }

        [Column("unit_name")]
        public string UnitName { get; set; }

        public int St { get; set; }

        [Column("production_qty")]
        public int ProductionQty { get; set; }

        [Column("production_qty_edit")]
        public int ProductionQtyEdit { get; set; }

        // ========================
        // ⚙️ Kiểm tra mạng
        // ========================
        private static bool IsNetworkAvailable()
        {
            try { return NetworkInterface.GetIsNetworkAvailable(); }
            catch { return false; }
        }

        // ========================
        // 🧾 Ghi log lỗi
        // ========================
        private static void LogError(string method, Exception ex = null, string msg = "")
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, $"timeplan_error_{DateTime.Now:yyyy-MM-dd}.txt");
                using (var sw = new StreamWriter(logFile, true))
                {
                    sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{method}] {msg}");
                    if (ex != null)
                    {
                        sw.WriteLine($"Lỗi: {ex.Message}");
                        if (ex.InnerException != null)
                            sw.WriteLine($"Chi tiết: {ex.InnerException.Message}");
                        sw.WriteLine(ex.StackTrace);
                    }
                    sw.WriteLine("-------------------------------------------");
                }
            }
            catch { /* tránh crash */ }
        }

        // ========================
        // 🔧 Lấy chuỗi kết nối MySQL
        // ========================
        private static string GetConnectionString()
        {
            try
            {
                var config = ConfigHelper.ReadConfig();
                string connStr = config.ContainsKey("db_line") ? config["db_line"] : "";
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new Exception("Không tìm thấy cấu hình DB (db_line) trong file config.ini!");
                return connStr;
            }
            catch (Exception ex)
            {
                LogError("GetConnectionString", ex);
                return null;
            }
        }

        // ========================
        // 🔍 Lấy danh sách Timeplan theo Kari
        // ========================
        public List<TimeplantInfo> GetTimeplantByKari(int kari,bool dbcheck)
        {
            var list = new List<TimeplantInfo>();
            string connStr = GetConnectionString();
            bool fallbackToLocal = false;

            if (string.IsNullOrEmpty(connStr))
            {
                LogError("GetTimeplantByKari", msg: "Không tìm thấy chuỗi kết nối DB.");
                fallbackToLocal = true;
            }
            else if (!IsNetworkAvailable())
            {
                LogError("GetTimeplantByKari", msg: "⚠️ Mạng không khả dụng, chuyển sang đọc local.");
                fallbackToLocal = true;
            }

            // ========================
            // 1️⃣ Ưu tiên đọc từ DB MySQL
            // ========================
            if (dbcheck)
            {
                try
                {
                    using (var connection = new MySqlConnection(connStr))
                    {
                        connection.Open();

                        var result = connection.Query<TimeplantInfo>(
                            "GetTimeplantByKariStation",
                            new { p_kari = kari },
                            commandType: CommandType.StoredProcedure
                        );

                        list = result.ToList();
                        ExcelExporter.SyncToExcel(new Dictionary<string, object>
                        {
                            { "Timeplant", list }
                        });

                        // ✅ Ghi log
                        LogError("GetTimeplantByKari", msg: $"Đọc {list.Count} dòng từ MySQL thành công (Kari={kari}).");
                    }
                }
                catch (MySqlException ex)
                {
                    LogError("GetTimeplantByKari-MySQL", ex);
                    fallbackToLocal = true;
                }
                catch (Exception ex)
                {
                    LogError("GetTimeplantByKari-General", ex);
                    fallbackToLocal = true;
                }
            }

            // ========================
            // 2️⃣ Fallback đọc file Excel khi mất DB
            // ========================
            if (!dbcheck)
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataMaster", "TemplateMachine.xlsx");

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("❌ Không tìm thấy file DataMaster\\TemplateMachine.xlsx  để đọc local.",
                        "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LogError("FallbackExcel", msg: "File Excel TemplateMachine.xlsx  không tồn tại.");
                    return list;
                }

                try
                {
                    using (var wb = new XLWorkbook(filePath))
                    {
                        var ws = wb.Worksheets.FirstOrDefault(s => s.Name == "Timeplant") ?? wb.Worksheet(1);
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                        for (int r = 2; r <= lastRow; r++)
                        {
                            var item = new TimeplantInfo
                            {
                                Line = ws.Cell(r, 1).GetString(),
                                Unit = ws.Cell(r, 2).GetString(),
                                Kari = int.TryParse(ws.Cell(r, 3).GetString(), out int k) ? k : 0,
                                UnitName = ws.Cell(r, 4).GetString(),
                                St = int.TryParse(ws.Cell(r, 5).GetString(), out int s) ? s : 0,
                                ProductionQty = int.TryParse(ws.Cell(r, 6).GetString(), out int pq) ? pq : 0,
                                ProductionQtyEdit = int.TryParse(ws.Cell(r, 7).GetString(), out int pqe) ? pqe : 0
                            };

                            if (item.Kari == kari)
                                list.Add(item);
                        }

                        LogError("FallbackExcel", msg: $"Đọc {list.Count} dòng từ file local (Kari={kari}).");
                    }
                }
                catch (Exception ex)
                {
                    LogError("ReadLocalExcel", ex);
                }
            }

            return list;
        }
    }
}

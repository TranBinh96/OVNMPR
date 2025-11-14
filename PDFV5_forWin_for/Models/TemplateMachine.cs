using ClosedXML.Excel;
using Dapper;
using MySqlConnector;
using PDFV5_forWin_for.Initializer;
using PDFV5_forWin_for.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PDFV5_forWin_for.Models
{
    [Table("template_machine")]
    public class TemplateMachine
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("machine_id")]
        [MaxLength(50)]
        public string MachineId { get; set; }

        [Column("station")]
        [MaxLength(20)]
        public string Station { get; set; }

        public int Page { get; set; }

        [Column("value_default")]
        [MaxLength(255)]
        public string ValueDefault { get; set; }

        [Column("user_name")]
        [MaxLength(100)]
        public string UserName { get; set; }

        public string Pcname { get; set; }

        [Column("update_time")]
        public DateTime? UpdateTime { get; set; }

        [Column("create_time")]
        public DateTime? CreateTime { get; set; }

        // ============================================
        // 🔍 LẤY DỮ LIỆU THEO MACHINE_ID + STATION
        // ============================================
        public static List<TemplateMachine> GetByMachineAndStation(string machineId, string station, bool dbCheck)
        {
            var list = new List<TemplateMachine>();

            if (dbCheck)
            {
                try
                {
                    var config = ConfigHelper.ReadConfig();
                    string connStr = config.ContainsKey("db_line") ? config["db_line"] : "";

                    if (string.IsNullOrEmpty(connStr))
                        throw new Exception("Không tìm thấy cấu hình DB (db_line)!");

                    using (var conn = new MySqlConnection(connStr))
                    {
                        conn.Open();

                        var result = conn.Query<TemplateMachine>(
                            "GetTemplateByMachineAndStation",
                            new { p_machine_id = machineId, p_station = station },
                            commandType: CommandType.StoredProcedure
                        );

                        list = result.ToList();

                        // 🧾 Lưu ra file local để dùng offline
                        ExcelExporter.SyncToExcel(new Dictionary<string, object>
                    {
                        { "TemplateMachine", list }
                    });

                        LogInfo($"Đã tải {list.Count} dòng từ MySQL (machine={machineId}, station={station})");
                    }
                }
                catch (Exception ex)
                {
                    LogError("GetByMachineAndStation", ex);
                    MessageBox.Show($"⚠ Không kết nối được MySQL. Đang đọc dữ liệu local...\n{ex.Message}",
                        "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }


            }
            else
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataMaster", "TemplateMachine.xlsx");

                if (!File.Exists(filePath))
                {
                    MessageBox.Show("❌ Không tìm thấy file DataMaster\\TemplateMachine.xlsx để đọc local.",
                        "Thiếu dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return list;
                }

                try
                {
                    using (var wb = new XLWorkbook(filePath))
                    {
                        var ws = wb.Worksheets.FirstOrDefault(s => s.Name == "TemplateMachine") ?? wb.Worksheet(1);
                        int lastRow = ws.LastRowUsed()?.RowNumber() ?? 1;

                        for (int r = 2; r <= lastRow; r++)
                        {
                            var tm = new TemplateMachine
                            {
                                Id = int.TryParse(ws.Cell(r, 1).GetString(), out int idVal) ? idVal : 0,
                                MachineId = ws.Cell(r, 2).GetString(),
                                Station = ws.Cell(r, 3).GetString(),
                                Page = int.TryParse(ws.Cell(r, 4).GetString(), out int pgVal) ? pgVal : 0,
                                ValueDefault = ws.Cell(r, 5).GetString(),
                                UserName = ws.Cell(r, 6).GetString(),
                                Pcname = ws.Cell(r, 7).GetString(),
                                UpdateTime = DateTime.TryParse(ws.Cell(r, 8).GetString(), out DateTime upd) ? upd : (DateTime?)null,
                                CreateTime = DateTime.TryParse(ws.Cell(r, 9).GetString(), out DateTime cre) ? cre : (DateTime?)null
                            };

                            // Lọc theo machine & station
                            if (tm.MachineId.Equals(machineId, StringComparison.OrdinalIgnoreCase) &&
                                tm.Station.Equals(station, StringComparison.OrdinalIgnoreCase))
                            {
                                list.Add(tm);
                            }
                        }

                        LogInfo($"Đọc {list.Count} dòng từ file local (machine={machineId}, station={station})");
                    }
                }
                catch (Exception ex)
                {
                    LogError("ReadLocalExcel", ex);
                    MessageBox.Show($"❌ Lỗi khi đọc file local: {ex.Message}",
                        "Lỗi Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            return list;
        }

        // ==================================================
        // 🧾 GHI LOG LỖI / THÔNG TIN
        // ==================================================
        private static void LogError(string method, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, "template_machine_error.txt");

                string msg =
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] [{method}]\n" +
                    $"Message: {ex.Message}\n" +
                    $"StackTrace: {ex.StackTrace}\n" +
                    new string('-', 100) + "\n";

                File.AppendAllText(logPath, msg);
            }
            catch
            {
                // Không để crash ứng dụng
            }
        }

        private static void LogInfo(string message)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, "template_machine_info.txt");

                string msg = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [INFO] {message}\n";
                File.AppendAllText(logPath, msg);
            }
            catch
            {
                // Không để crash ứng dụng
            }
        }
    }
}

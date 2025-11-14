using System;
using System.Data;
using System.IO;
using Npgsql;
using ClosedXML.Excel;

namespace DailyReport
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                // 1️⃣ Chuỗi kết nối PostgreSQL (vô hạn timeout)
                string connStr =
                    "Host=10.216.28.11;Port=5432;Database=optas;Username=ovnm;Password=ovnm;" +
                    "CommandTimeout=0;Timeout=0;KeepAlive=1;SearchPath=ovnm;Pooling=True;SslMode=Disable";

                using (var conn = new NpgsqlConnection(connStr))
                {
                    conn.Open();
                    Console.WriteLine("✅ Đã kết nối database.");

                    // 2️⃣ Câu SQL 1 (Sheet 1)
                    string sql1 = @"
                    SELECT 
                        d.""ライン名"" AS line,
                        b.""明細番号"" AS station,
                        b.""品番コード"" AS kari,
                        COUNT(b.""作業開始日時"") AS count_start,
                        COUNT(b.""作業終了日時"") AS count_end,
                        COUNT(*) AS total
                    FROM ""p_作業実績"" b
                    JOIN ""p_工程進捗"" a ON a.""指図seq番号""::text = b.""指図seq番号""::text
                    JOIN ""p_仮id_製造オーダー"" c ON a.""仮番コード""::text = c.""仮id""::text
                    JOIN ""p_ラインマスタ"" d ON d.""ラインコード""::text = a.""ラインコード""::text
                    WHERE b.""作業開始日時"" >= CURRENT_DATE
                      AND b.""作業開始日時"" < CURRENT_DATE + INTERVAL '1 day'
                    GROUP BY d.""ライン名"", b.""明細番号"", b.""品番コード""
                    ORDER BY d.""ライン名"", b.""品番コード"", b.""明細番号"";";

                    // 3️⃣ Câu SQL 2 (Sheet 2)
                    string sql2 = @"
                    SELECT
                        d.""ライン名"" AS line,
                        b.""明細番号"" AS station,
                        a.""仮番コード"" AS kari,
                        a.""指図seq番号"" AS ProdOrderNo,
                        MIN(b.""作業開始日時"") AS start_time,
                        MAX(b.""作業終了日時"") AS end_time,
                        CASE WHEN COUNT(*) > 0 THEN TRUE ELSE FALSE END AS Result,
                        split_part(b.host_name, ';', 1) AS HostName,
                        b.""作業開始日時""::date AS date
                    FROM ""p_作業実績"" b
                    JOIN ""p_工程進捗"" a
                        ON a.""指図seq番号""::text = b.""指図seq番号""::text
                    JOIN ""p_仮id_製造オーダー"" c
                        ON a.""仮番コード""::text = c.""仮id""::text
                    JOIN ""p_ラインマスタ"" d
                        ON d.""ラインコード""::text = a.""ラインコード""::text
                    WHERE b.""作業開始日時"" >= CURRENT_DATE - INTERVAL '7 day'
                      AND b.""作業開始日時"" < CURRENT_DATE + INTERVAL '1 day'
                    GROUP BY
                        d.""ライン名"",
                        b.""明細番号"",
                        a.""仮番コード"",
                        a.""指図seq番号"",
                        b.""作業開始日時""::date,
                        split_part(b.host_name, ';', 1)
                    ORDER BY
                        b.""作業開始日時""::date DESC,
                        d.""ライン名"",
                        a.""仮番コード"",
                        b.""明細番号"";";

                    // 4️⃣ Đọc dữ liệu từng bảng
                    DataTable dt1 = new DataTable();
                    using (var cmd = new NpgsqlCommand(sql1, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt1.Load(reader);
                        }
                    }

                    DataTable dt2 = new DataTable();
                    using (var cmd = new NpgsqlCommand(sql2, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            dt2.Load(reader);
                        }
                    }

                    // 5️⃣ Tạo thư mục Output (gốc)
                    string baseOutputDir = Path.Combine(@"\\10.216.28.26\VNserver1\04. KTSX\DailyReportOPTAS");
                    Directory.CreateDirectory(baseOutputDir);

                    // 📅 Thêm thư mục con theo ngày (vd: 2025-10-31)
                    string today = DateTime.Now.ToString("yyyy-MM-dd");
                    string outputDir = Path.Combine(baseOutputDir, today);

                    // 🔧 Tự tạo nếu chưa tồn tại
                    Directory.CreateDirectory(outputDir);

                    // 📄 Tạo file Excel theo ngày
                    string fileName = $"WorkSummary_{today}.xlsx";
                    string filePath = Path.Combine(outputDir, fileName);

                    // 6️⃣ Xuất ra Excel có 2 sheet
                    using (var wb = new XLWorkbook())
                    {
                        var ws1 = wb.Worksheets.Add("By品番コード");
                        ws1.Cell(1, 1).InsertTable(dt1, "Sheet1", true);
                        ws1.Columns().AdjustToContents();

                        var ws2 = wb.Worksheets.Add("By仮番コード");
                        ws2.Cell(1, 1).InsertTable(dt2, "Sheet2", true);
                        ws2.Columns().AdjustToContents();

                        wb.SaveAs(filePath);
                    }

                    Console.WriteLine($"📊 Đã tạo file Excel: {filePath}");
                }
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DailyReport_Error.log");
                File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ❌ {ex}\n");
                Console.WriteLine($"❌ Lỗi: {ex.Message}");
            }
        }
    }
}

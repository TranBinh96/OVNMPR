using ClosedXML.Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PDFV5_forWin_for.Initializer
{
    internal static class ExcelExporter
    {
        /// <summary>
        /// Đồng bộ nhiều danh sách (List&lt;Class&gt;) vào cùng 1 file Excel.
        /// Mỗi danh sách là 1 sheet. Sau khi ghi xong, trả về tất cả dữ liệu đã ghi (gộp list).
        /// Tự động ghi log vào logs/export_excel_log.txt.
        /// </summary>
        public static List<object> SyncToExcel(Dictionary<string, object> dataSets)
        {
            var allData = new List<object>();
            string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
            Directory.CreateDirectory(logDir);
            string logPath = Path.Combine(logDir, "export_excel_log.txt");

            if (dataSets == null || dataSets.Count == 0)
            {
                Log(logPath, "⚠ Không có dữ liệu nào để xuất Excel.");
                return allData;
            }

            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataMaster");
            Directory.CreateDirectory(dir);
            string excelPath = Path.Combine(dir, "TemplateMachine.xlsx");

            try
            {
                using (var wb = File.Exists(excelPath) ? new XLWorkbook(excelPath) : new XLWorkbook())
                {
                    foreach (var kv in dataSets)
                    {
                        string sheetName = kv.Key;
                        var listObj = kv.Value as IEnumerable;
                        if (listObj == null)
                        {
                            Log(logPath, $"❌ Sheet [{sheetName}] không hợp lệ (không phải danh sách).");
                            continue;
                        }

                        var list = listObj.Cast<object>().ToList();
                        if (list.Count == 0)
                        {
                            Log(logPath, $"⚠ Sheet [{sheetName}] không có dữ liệu.");
                            continue;
                        }

                        allData.AddRange(list);
                        Type type = list.First().GetType();
                        PropertyInfo[] props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                        var ws = wb.Worksheets.FirstOrDefault(s => s.Name == sheetName)
                                 ?? wb.AddWorksheet(sheetName);

                        // Xóa dữ liệu cũ
                        ws.Clear();

                        // Header
                        for (int c = 0; c < props.Length; c++)
                        {
                            ws.Cell(1, c + 1).Value = props[c].Name;
                            ws.Cell(1, c + 1).Style.Font.Bold = true;
                            ws.Cell(1, c + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            ws.Cell(1, c + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        // Dữ liệu
                        int row = 2;
                        foreach (var item in list)
                        {
                            for (int c = 0; c < props.Length; c++)
                            {
                                var val = props[c].GetValue(item);
                                ws.Cell(row, c + 1).Value = val?.ToString() ?? "";
                            }
                            row++;
                        }

                        ws.Columns().AdjustToContents();

                        Log(logPath, $"✅ Sheet [{sheetName}] đã ghi {list.Count} dòng.");
                    }

                    wb.SaveAs(excelPath);
                }

                Log(logPath, $"🎯 Hoàn tất xuất {allData.Count} dòng ({dataSets.Count} sheet) vào {excelPath}");
            }
            catch (Exception ex)
            {
                Log(logPath, $"❌ Lỗi khi ghi Excel: {ex.Message}\n{ex.StackTrace}");
            }

            return allData;
        }

        // === Ghi log ra file ===
        private static void Log(string logPath, string message)
        {
            string line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            File.AppendAllText(logPath, line + Environment.NewLine);
            Console.WriteLine(line);
        }
    }
}

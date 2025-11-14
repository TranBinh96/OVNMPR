using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDFV5_forWin_for.Data
{
    public class CTTK_PDF
    {
        public string FileName { get; set; }
        public string FileLink { get; set; }
        public string FileType { get; set; }
        public string Line { get; set; }
        public string Unit { get; set; }
        public string Kari { get; set; }
        public string isLast { get; set; }
        public string Proficiency { get; set; }
        public int Station { get; set; }
        public string Version { get; set; }
        
        public DateTime LastModified { get; set; }

        static bool IsDigits(string s) => Regex.IsMatch(s ?? "", @"^\d+$");
        static bool IsAlphaToken(string s) => Regex.IsMatch(s ?? "", @"^[A-Za-z0-9\-]+$"); // Line/Unit thường là chữ/số
        static bool IsFlagToken(string s)
        {
            // Ký hiệu cờ trong vị trí flag: E/SE/Q/S/M... mở rộng nếu cần
            if (string.IsNullOrWhiteSpace(s)) return false;
            var t = s.Trim().ToUpperInvariant();
            return t == "E" || t == "SE" || t == "Q" || t == "S" || t == "M";
        }

        public  List<CTTK_PDF> GetPdfFiles()
        {
            string pdfDir = @"C:\PDFviewer_ForWin_PDF";
            Directory.CreateDirectory(pdfDir);

            var pdfList = new List<CTTK_PDF>();
            var files = Directory.GetFiles(pdfDir, "*.pdf", SearchOption.TopDirectoryOnly)
                                 .OrderBy(f => f)
                                 .ToList();

            Console.WriteLine($"📂 Đang quét thư mục: {pdfDir}");
            Console.WriteLine($"🔍 Tìm thấy {files.Count} file PDF.\n");

            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var fileName = Path.GetFileNameWithoutExtension(file);
                var parts = fileName.Split('_');

                var item = new CTTK_PDF
                {
                    FileName = fileName,
                    FileLink = file,
                    LastModified = info.LastWriteTime
                };

                // ========= PHÂN LOẠI =========
                // Dạng FULL: Line, Unit, Flag, Kari, Station, Proficiency, [Version?]
                bool looksFull =
                    parts.Length >= 6 &&
                    IsAlphaToken(parts[0]) &&          // Line
                    IsAlphaToken(parts[1]) &&          // Unit
                    IsFlagToken(parts[2]) &&           // Flag
                    IsDigits(parts[3]);                // Kari

                // Dạng SHORT: Flag, Kari, Station, Proficiency, [Version?]
                bool looksShort =
                    parts.Length >= 4 &&
                    IsFlagToken(parts[0]) &&           // Flag
                    IsDigits(parts[1]);                // Kari

                if (looksFull)
                {
                    // FULL có hoặc không có Version
                    item.Line = parts[0].ToUpper();
                    item.Unit = parts[1].ToUpper();
                    item.isLast = parts[2]; // giữ nguyên ký hiệu (E/SE/Q...)
                    item.Kari = parts[3];
                    item.Station = int.TryParse(parts[4], out int s1) ? s1 : 0;
                    item.Proficiency = parts[5].ToUpper();
                    item.Version = (parts.Length >= 7 && parts[6].StartsWith("VER", StringComparison.OrdinalIgnoreCase))
                                   ? parts[6]
                                   : "VER0";
                    item.FileType = "FULL";

                    Console.WriteLine($"✅ [FULL] {fileName}");
                }
                else if (looksShort)
                {
                    // SHORT có hoặc không có Version
                    item.Line = "";
                    item.Unit = "";
                    item.isLast = parts[0]; // E/SE/Q...
                    item.Kari = parts[1];
                    item.Station = int.TryParse(parts[2], out int s2) ? s2 : 0;
                    item.Proficiency = parts[3].ToUpper();
                    item.Version = (parts.Length >= 5 && parts[4].StartsWith("VER", StringComparison.OrdinalIgnoreCase))
                                   ? parts[4]
                                   : "VER0";
                    item.FileType = "SHORT";
                }
                else
                {
                    Console.WriteLine($"⚠️ [SKIP] {fileName} (không khớp mẫu FULL/SHORT)");
                    continue;
                }

                pdfList.Add(item);
            }

            Console.WriteLine($"\n📄 Hoàn tất. Tổng cộng đọc được {pdfList.Count} file hợp lệ.\n");
            return pdfList;
        }





        //public static string GetLatestPdfByKeyword(string keyword)
        //{
        //    var files = GetPdfFiles()
        //        .Where(f => Path.GetFileName(f).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
        //        .ToList();

        //    if (files.Count == 0)
        //        return null;

        //    var regex = new Regex(@"VER(\d+)", RegexOptions.IgnoreCase);

        //    var matchedFiles = files
        //        .Select(f => new
        //        {
        //            FilePath = f,
        //            Ver = regex.Match(Path.GetFileName(f)).Success
        //                ? int.Parse(regex.Match(Path.GetFileName(f)).Groups[1].Value)
        //                : 0
        //        })
        //        .OrderByDescending(x => x.Ver)
        //        .ToList();

        //    return matchedFiles.First().FilePath;
        //}
    }
}

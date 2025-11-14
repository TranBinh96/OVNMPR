using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace PDFV5_forWin_for.Initializer
{
    public static class AppInitializer
    {
        private static readonly string _pdfDir = @"C:\PDFviewer_ForWin_PDF";

        /// <summary>
        /// Lấy danh sách đường dẫn đầy đủ của tất cả file PDF trong thư mục.
        /// </summary>
        public static List<string> GetPdfFiles()
        {
            Directory.CreateDirectory(_pdfDir);
            return Directory.GetFiles(_pdfDir, "*.pdf", SearchOption.TopDirectoryOnly).ToList();
        }

        /// <summary>
        /// Tìm file PDF có chứa keyword (ví dụ "2027_01_Beginner") và có "VERxxx",
        /// trả về đường dẫn đầy đủ của file có version cao nhất.
        /// </summary>
        public static string GetLatestPdfByKeyword(string keyword)
        {
            var files = GetPdfFiles()
                .Where(f => Path.GetFileName(f).IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            if (files.Count == 0)
                return null;

            var regex = new Regex(@"VER(\d+)", RegexOptions.IgnoreCase);

            var matchedFiles = files
                .Select(f => new
                {
                    FilePath = f,
                    Ver = regex.Match(Path.GetFileName(f)).Success
                        ? int.Parse(regex.Match(Path.GetFileName(f)).Groups[1].Value)
                        : 0
                })
                .OrderByDescending(x => x.Ver)
                .ToList();

            return matchedFiles.First().FilePath;
        }


        public static string GetMacAddress()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.OperationalStatus == OperationalStatus.Up &&
                                  nic.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                                  nic.NetworkInterfaceType != NetworkInterfaceType.Unknown)
                    .Select(nic => nic.GetPhysicalAddress()?.ToString())
                    .FirstOrDefault(mac => !string.IsNullOrWhiteSpace(mac))
                    ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }
    }
}

using MySqlConnector;
using PDFV5_forWin_for.Initializer;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;

namespace PDFV5_forWin_for.Utils
{
    /// <summary>
    /// Kết quả kiểm tra kết nối DB.
    /// </summary>
    public sealed class DbCheckResult
    {
        public bool IsOk { get; set; }                 // ✅ Kết nối thành công
        public string Message { get; set; }            // 🧾 Mô tả ngắn gọn kết quả
        public double ConnectMs { get; set; }          // ⏱ Thời gian kết nối (ms)
        public double PingMs { get; set; }             // 📶 Thời gian ping host (ms)
        public string ServerVersion { get; set; }      // ⚙️ Phiên bản server MySQL
        public string Host { get; set; }               // 🖥 Host trong connection string
        public uint Port { get; set; }                 // 🔌 Port MySQL
        public string Database { get; set; }           // 📂 Database name
    }

    /// <summary>
    /// Tiện ích kiểm tra khả năng kết nối MySQL nhanh (timeout mặc định 2 giây).
    /// Tự đọc connection string từ config.ini (db_line).
    /// </summary>
    public static class DbConnectivityChecker
    {
        private static string GetConnectionString()
        {
            try
            {
                var config = ConfigHelper.ReadConfig();
                if (!config.ContainsKey("db_line"))
                    throw new Exception("Không tìm thấy cấu hình 'db_line' trong file config.ini!");

                string connStr = config["db_line"];
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new Exception("Giá trị 'db_line' trong config.ini bị trống!");

                return connStr;
            }
            catch (Exception ex)
            {
                LogError("GetConnectionString", ex);
                return string.Empty;
            }
        }

        /// <summary>
        /// Kiểm tra khả năng kết nối DB trong timeoutSeconds (mặc định 2s).
        /// Có thể ping trước để xác định mạng.
        /// </summary>
        public static DbCheckResult Check(int timeoutSeconds = 2, bool doPing = true)
        {
            var result = new DbCheckResult
            {
                IsOk = false,
                Message = "",
                ConnectMs = -1,
                PingMs = -1
            };

            string connStr = GetConnectionString();
            if (string.IsNullOrEmpty(connStr))
            {
                result.Message = "Không tìm thấy chuỗi kết nối DB.";
                return result;
            }

            MySqlConnectionStringBuilder builder;
            try
            {
                builder = new MySqlConnectionStringBuilder(connStr)
                {
                    ConnectionTimeout = (uint)Math.Max(1, timeoutSeconds)
                };
                result.Host = builder.Server;
                result.Port = builder.Port;
                result.Database = builder.Database;
            }
            catch (Exception ex)
            {
                result.Message = $"Lỗi phân tích chuỗi kết nối: {ex.Message}";
                return result;
            }

            // 1️⃣ Ping trước (nếu cần)
            if (doPing)
            {
                try
                {
                    using (var p = new Ping())
                    {
                        var sw = Stopwatch.StartNew();
                        var reply = p.Send(builder.Server, timeoutSeconds * 1000);
                        sw.Stop();

                        if (reply != null && reply.Status == IPStatus.Success)
                            result.PingMs = sw.Elapsed.TotalMilliseconds;
                        else
                            result.PingMs = -1;
                    }
                }
                catch
                {
                    result.PingMs = -1;
                }
            }

            // 2️⃣ Thử mở kết nối
            try
            {
                using (var conn = new MySqlConnection(builder.ConnectionString))
                {
                    var sw = Stopwatch.StartNew();
                    conn.Open();
                    sw.Stop();

                    result.ConnectMs = sw.Elapsed.TotalMilliseconds;
                    result.ServerVersion = conn.ServerVersion;
                    result.IsOk = true;
                    result.Message = $"Kết nối thành công trong {result.ConnectMs:0}ms.";
                }
            }
            catch (MySqlException ex)
            {
                result.IsOk = false;
                result.Message = $"MySQL lỗi: {ex.Message}";
                LogError("DbConnectivityChecker-MySQL", ex);
            }
            catch (Exception ex)
            {
                result.IsOk = false;
                result.Message = $"Lỗi hệ thống: {ex.Message}";
                LogError("DbConnectivityChecker-General", ex);
            }

            return result;
        }

        /// <summary>
        /// 🧾 Ghi log lỗi ra thư mục logError
        /// </summary>
        private static void LogError(string method, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                Directory.CreateDirectory(logDir);
                string logFile = Path.Combine(logDir, $"db_check_{DateTime.Now:yyyy-MM-dd}.log");

                File.AppendAllText(logFile,
                    $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{method}] {ex.Message}\n{ex.StackTrace}\n---------------------------------\n");
            }
            catch { /* tránh crash */ }
        }
    }
}

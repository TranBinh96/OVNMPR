using DocumentFormat.OpenXml.InkML;
using Npgsql;
using PDFV5_forWin_for.Models;
using System;
using System.Globalization;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;

namespace PDFV5_forWin_for.Data
{
    /// <summary>
    /// Class xử lý giao tiếp OPTAS: kiểm tra KariID và upload CSV.
    /// </summary>
    public static class Optas
    {
        private static string _startTimestamp = "ERR";
        private static string _startMemo = "ERR";

        // =============================================
        // 🧩 1️⃣ HÀM UPLOAD DỮ LIỆU CSV LÊN SERVER OPTAS
        // =============================================
        public static string Upload(
        string station,
        string kariId,
        string macAddress,
        string workerId,
        string optasUrl,
        string sOrE,
        bool lastST)
        {
            try
            {
                string machineName = Environment.MachineName;
                DateTime now = DateTime.Now;

                // 📁 Các thư mục cục bộ
                string baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csv_upload");
                string ngDir = Path.Combine(baseDir, "OPTAS_Upload_NG");
                string okDir = Path.Combine(baseDir, "OPTAS_Upload_OK");
                Directory.CreateDirectory(ngDir);
                Directory.CreateDirectory(okDir);

                // 📄 Tạo tên file
                string prefix = kariId.Length >= 4 ? kariId.Substring(0, 4) : kariId;
                string fileName = $"_AU_{now:yyyyMMddHHmmss}_{kariId}_{sOrE}_{machineName}.csv";

                // 🧾 Dòng CSV
                string csvLine = string.Empty;


                OptasModel optasmodel = new OptasModel
                {
                    Unit = prefix,
                    Station = station,
                    WorkerId = workerId,
                    UnitId = kariId,
                    Hostname = machineName,
                    CreateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    UpdateAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                };

                // 🔹 Nếu là START
                if (!lastST && sOrE == "S")
                {
                    string timeStamp = now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    csvLine = $"\"{prefix}\",{station},\"{workerId}\",\"{kariId}\",#{timeStamp}#,\"{machineName}\",\"{macAddress}\"";
                    _startTimestamp = $"#{timeStamp}#,"; // lưu thời gian bắt đầu theo chuẩn
                    _startMemo = timeStamp; // luôn lưu theo định dạng ISO chuẩn

                    optasmodel.StartAt = timeStamp;
                    optasmodel.CodeId = csvLine;    
                }
                // 🔹 Nếu là END
                else
                {
                    DateTime start;

                    // Bảo vệ Parse tránh lỗi format
                    if (!DateTime.TryParseExact(_startMemo, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out start))
                    {
                        // fallback nếu dữ liệu cũ không đúng format
                        start = now;
                        LogError("Upload", new FormatException($"Invalid _startMemo format: {_startMemo}"));
                    }

                    int seconds = (int)Math.Round((now - start).TotalSeconds);
                    string endStamp = now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    csvLine = $"\"{prefix}\",{station},\"{workerId}\",\"{kariId}\",{_startTimestamp}#{endStamp}#,{seconds},\"{machineName}\",\"{macAddress}\",{(lastST ? 1 : 0)}";

                    optasmodel.EndAt = endStamp;
                    optasmodel.CodeId = csvLine;

                }   

                // ⚠️ Nếu mã giả có chứa "999999" → bỏ qua
                if (HasSixNines(kariId))
                    return "skip";

                // 📝 Ghi file CSV vào thư mục NG
                string localCsv = Path.Combine(ngDir, fileName);
                File.WriteAllText(localCsv, csvLine);

                // ☁️ Copy lên server (optasUrl)
                string destCsv = Path.Combine(optasUrl, fileName);
                Directory.CreateDirectory(optasUrl);
                File.Copy(localCsv, destCsv, true);

                // ⏳ Chờ xuất hiện & đổi tên
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(1000);
                    if (File.Exists(destCsv))
                    {
                        string renamed = $"AU_{now:yyyyMMddHHmmss}_{kariId}_{sOrE}_{machineName}.csv";
                        File.Move(destCsv, Path.Combine(optasUrl, renamed));
                        File.Move(localCsv, Path.Combine(okDir, renamed));
                        break;
                    }
                }

                optasmodel.FileName = $"AU_{now:yyyyMMddHHmmss}_{kariId}_{sOrE}_{machineName}.csv";
                optasmodel.Insert();

                // 🔁 Upload lại các file còn sót trong NG
                foreach (string file in Directory.GetFiles(ngDir, "*.csv", SearchOption.AllDirectories))
                {
                    string fileNameOnly = Path.GetFileName(file);
                    string destPath = Path.Combine(optasUrl, fileNameOnly);

                    if (!HasSixNines(kariId))
                        File.Copy(file, destPath, true);

                    Thread.Sleep(1000);

                    if (File.Exists(destPath))
                    {
                        string newName = fileNameOnly.TrimStart('_');
                        File.Move(destPath, Path.Combine(optasUrl, newName));
                        File.Move(file, Path.Combine(okDir, newName));
                    }
                }

                return "ok";
            }
            catch (Exception ex)
            {
                LogError("Upload", ex);
                return $"Error: {ex.Message}";
            }
        }
        // =============================================
        // 🧩 2️⃣ HÀM KIỂM TRA KARIID TRONG OPTAS DATABASE
        // =============================================
        public static bool kariidCheck(string kariid, string st)
        {
            int count = 0;
            string host = "10.216.28.11";
            string dateLimit = DateTime.Now.AddDays(-365).ToString("yyyy-MM-dd");

            string connStr =
                $"Server={host};Port=5432;Database=optas;UserId=ovnm;Password=ovnm;SearchPath=ovnm;Pooling=True;Timeout=5;CommandTimeout=5;";

            string sql = $@"
                SELECT 1
                FROM p_工程進捗 A
                WHERE A.会社コード = 'CP015'
                  AND A.開始日時 >= '{dateLimit}'
                  AND A.仮番コード = '{kariid}'
                  AND EXISTS (
                      SELECT 1 FROM p_作業実績 B
                      WHERE B.会社コード = A.会社コード
                        AND B.明細番号 = {st}
                        AND B.指図seq番号 = A.指図seq番号
                  );";

            try
            {
                // 🧩 Ping kiểm tra mạng
                using (var ping = new Ping())
                {
                    var reply = ping.Send(host, 1000);
                    if (reply.Status != IPStatus.Success)
                    {
                        MessageBox.Show($"⚠ Không thể ping tới server {host}.", "Thông báo",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                // 🧩 Truy vấn OPTAS
                using (var conn = new NpgsqlConnection(connStr))
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read()) count++;
                }

                return count > 0;;
            }
            catch (Exception ex)
            {
                LogError("kariidCheck", ex);
                return false;
            }
        }

        // =============================================
        // 🧩 HÀM HỖ TRỢ CHUNG
        // =============================================
        private static bool HasSixNines(string input)
        {
            return !string.IsNullOrEmpty(input) && input.Contains("999999");
        }

        private static void LogError(string source, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, "optas_error.txt");
                string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{source}] {ex.Message}\n{ex.StackTrace}\n";
                File.AppendAllText(logPath, logLine);
            }
            catch { /* bỏ qua lỗi ghi log */ }
        }
    }
}

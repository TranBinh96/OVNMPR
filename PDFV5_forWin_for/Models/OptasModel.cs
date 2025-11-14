using MySqlConnector;
using PDFV5_forWin_for.Initializer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace PDFV5_forWin_for.Models
{
    public class OptasModel
    {
        public int Id { get; set; }
        public string Unit { get; set; }
        public string UnitName { get; set; }
        public string Station { get; set; }
        public string WorkerId { get; set; }
        public string UnitId { get; set; }
        public string StartAt { get; set; }
        public string EndAt { get; set; }
        public string Hostname { get; set; }
        public string CodeId { get; set; }
        public string FileName { get; set; }
        public string CreateAt { get; set; }
        public string UpdateAt { get; set; }

        // ========================
        // 🔗 Lấy chuỗi kết nối từ config.ini
        // ========================
        private static string GetConnectionString()
        {
            try
            {
                var config = ConfigHelper.ReadConfig();
                string connStr = config.ContainsKey("db_line") ? config["db_line"] : "";
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new Exception("Không tìm thấy cấu hình DB (db_line)");
                return connStr;
            }
            catch (Exception ex)
            {
                LogError("Lỗi đọc config.ini", ex);
                return null;
            }
        }

        // ========================
        // 🧩 Hàm thử kết nối
        // ========================
        private static bool CanConnect()
        {
            string connStr = GetConnectionString();
            if (string.IsNullOrEmpty(connStr)) return false;

            try
            {
                using (var conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                LogError("Không thể kết nối MySQL", ex);
                return false;
            }
        }

        // ========================
        // 📁 Ghi log lỗi vào logError\
        // ========================
        private static void LogError(string message, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logError");
                if (!Directory.Exists(logDir))
                    Directory.CreateDirectory(logDir);

                string logFile = Path.Combine(logDir, $"db_error_{DateTime.Now:yyyy-MM-dd}.txt");

                using (StreamWriter sw = new StreamWriter(logFile, true))
                {
                    sw.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}");
                    sw.WriteLine($"Lỗi: {ex.Message}");
                    if (ex.InnerException != null)
                        sw.WriteLine($"Chi tiết: {ex.InnerException.Message}");
                    sw.WriteLine("---------------------------------------------------");
                }
            }
            catch
            {
                // Nếu lỗi khi ghi log thì bỏ qua, không dừng chương trình
            }
        }

        // ========================
        // 🟢 INSERT
        // ========================
        public void Insert()
        {
            if (!CanConnect())
            {
                Console.WriteLine("⚠️ [MySQL OFFLINE] Bỏ qua thao tác Insert, chương trình vẫn tiếp tục.");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(GetConnectionString()))
                using (var cmd = new MySqlCommand("sp_optas_insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_unit", Unit);
                    cmd.Parameters.AddWithValue("p_unit_name", UnitName);
                    cmd.Parameters.AddWithValue("p_station", Station);
                    cmd.Parameters.AddWithValue("p_worker_id", WorkerId);
                    cmd.Parameters.AddWithValue("p_unit_id", UnitId);
                    cmd.Parameters.AddWithValue("p_start_at", StartAt);
                    cmd.Parameters.AddWithValue("p_end_at", EndAt);
                    cmd.Parameters.AddWithValue("p_hostname", Hostname);
                    cmd.Parameters.AddWithValue("p_code_id", CodeId);
                    cmd.Parameters.AddWithValue("p_file_name", FileName);
                    cmd.Parameters.AddWithValue("p_create_at", CreateAt);
                    cmd.Parameters.AddWithValue("p_update_at", UpdateAt);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("✅ Insert OK.");
            }
            catch (Exception ex)
            {
                LogError("Lỗi khi Insert dữ liệu", ex);
                Console.WriteLine("⚠️ Bỏ qua Insert, lỗi đã ghi vào logError.");
            }
        }

        // ========================
        // 🔄 UPSERT
        // ========================
        public void Upsert()
        {
            if (!CanConnect())
            {
                Console.WriteLine("⚠️ [MySQL OFFLINE] Bỏ qua thao tác Upsert, chương trình vẫn chạy.");
                return;
            }

            try
            {
                using (var conn = new MySqlConnection(GetConnectionString()))
                using (var cmd = new MySqlCommand("sp_optas_upsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_unit", Unit);
                    cmd.Parameters.AddWithValue("p_unit_name", UnitName);
                    cmd.Parameters.AddWithValue("p_station", Station);
                    cmd.Parameters.AddWithValue("p_worker_id", WorkerId);
                    cmd.Parameters.AddWithValue("p_unit_id", UnitId);
                    cmd.Parameters.AddWithValue("p_start_at", StartAt);
                    cmd.Parameters.AddWithValue("p_end_at", EndAt);
                    cmd.Parameters.AddWithValue("p_hostname", Hostname);
                    cmd.Parameters.AddWithValue("p_code_id", CodeId);
                    cmd.Parameters.AddWithValue("p_file_name", FileName);
                    cmd.Parameters.AddWithValue("p_create_at", CreateAt);
                    cmd.Parameters.AddWithValue("p_update_at", UpdateAt);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("✅ Upsert OK.");
            }
            catch (Exception ex)
            {
                LogError("Lỗi khi Upsert dữ liệu", ex);
                Console.WriteLine("⚠️ Bỏ qua Upsert, lỗi đã ghi vào logError.");
            }
        }

        // ========================
        // 🔍 SEARCH
        // ========================
        public static List<OptasModel> Search(string unitId = null, string station = null)
        {
            var list = new List<OptasModel>();

            if (!CanConnect())
            {
                Console.WriteLine("⚠️ [MySQL OFFLINE] Không thể Search, trả về danh sách rỗng.");
                return list;
            }

            try
            {
                using (var conn = new MySqlConnection(GetConnectionString()))
                using (var cmd = new MySqlCommand("sp_optas_search", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_unit_id", (object)unitId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("p_station", (object)station ?? DBNull.Value);

                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(Read(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                LogError("Lỗi khi Search dữ liệu", ex);
                Console.WriteLine("⚠️ Bỏ qua Search, lỗi đã ghi vào logError.");
            }

            return list;
        }

        // ⚙️ Helper: Map Reader → Object
        private static OptasModel Read(MySqlDataReader r)
        {
            return new OptasModel
            {
                Id = Convert.ToInt32(r["id"]),
                Unit = r["unit"]?.ToString(),
                UnitName = r["unit_name"]?.ToString(),
                Station = r["station"]?.ToString(),
                WorkerId = r["worker_id"]?.ToString(),
                UnitId = r["unit_id"]?.ToString(),
                StartAt = r["start_at"]?.ToString(),
                EndAt = r["end_at"]?.ToString(),
                Hostname = r["hostname"]?.ToString(),
                CodeId = r["code_id"]?.ToString(),
                FileName = r["file_name"]?.ToString(),
                CreateAt = r["create_at"]?.ToString(),
                UpdateAt = r["update_at"]?.ToString()
            };
        }
    }
}

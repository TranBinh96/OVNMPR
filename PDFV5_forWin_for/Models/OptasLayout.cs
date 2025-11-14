using MySqlConnector;
using PDFV5_forWin_for.Initializer;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;

namespace PDFV5_forWin_for.Models
{
    public class OptasLayout
    {
        // ========================
        // 🔹 Thuộc tính
        // ========================
        public int Id { get; set; }
        public string KariId { get; set; }
        public int Station { get; set; }
        public string WorkerId { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int TotalTime { get; set; }
        public bool IsActive { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }

        // ========================
        // 🔗 Lấy chuỗi kết nối MySQL
        // ========================
        private static string GetConnectionString()
        {
            try
            {
                var config = ConfigHelper.ReadConfig();
                string connStr = config.ContainsKey("db_line") ? config["db_line"] : "";
                if (string.IsNullOrWhiteSpace(connStr))
                    throw new Exception("Không tìm thấy cấu hình DB (db_line)!");
                return connStr;
            }
            catch (Exception ex)
            {
                LogError("Lỗi đọc cấu hình DB", ex);
                return null;
            }
        }

        // ========================
        // ⚙️ Kiểm tra mạng và kết nối
        // ========================
        private static bool IsNetworkAvailable()
        {
            try
            {
                return NetworkInterface.GetIsNetworkAvailable();
            }
            catch
            {
                return false;
            }
        }

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
        private static void LogError(string message, Exception ex = null)
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
                    if (ex != null)
                    {
                        sw.WriteLine($"Lỗi: {ex.Message}");
                        if (ex.InnerException != null)
                            sw.WriteLine($"Chi tiết: {ex.InnerException.Message}");
                    }
                    sw.WriteLine("--------------------------------------------");
                }
            }
            catch
            {
                // tránh crash nếu lỗi ghi file
            }
        }

        // ========================
        // 🔄 Hàm chạy an toàn (retry + log lỗi)
        // ========================
        private static void ExecuteSafe(Action<MySqlConnection> action)
        {
            if (!IsNetworkAvailable())
            {
                LogError("⚠️ Mạng không khả dụng, bỏ qua thao tác DB.");
                return;
            }

            if (!CanConnect())
            {
                LogError("⚠️ Không thể kết nối MySQL, bỏ qua thao tác DB.");
                return;
            }

            int retry = 0;
            while (retry < 3)
            {
                try
                {
                    using (var conn = new MySqlConnection(GetConnectionString()))
                    {
                        conn.Open();
                        action(conn);
                        return; // ✅ thành công
                    }
                }
                catch (MySqlException ex)
                {
                    retry++;
                    LogError($"⚠️ Lỗi MySQL (lần {retry})", ex);
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    LogError("❌ Lỗi không xác định khi thao tác DB", ex);
                    return;
                }
            }

            LogError("❌ Thao tác DB thất bại sau 3 lần thử.");
        }

        // ========================
        // 🟢 INSERT
        // ========================
        public void Insert()
        {
            ExecuteSafe(conn =>
            {
                using (var cmd = new MySqlCommand("sp_optas_layout_insert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_kari_id", KariId);
                    cmd.Parameters.AddWithValue("p_station", Station);
                    cmd.Parameters.AddWithValue("p_worker_id", WorkerId);
                    cmd.Parameters.AddWithValue("p_current_page", CurrentPage);
                    cmd.Parameters.AddWithValue("p_total_pages", TotalPages);
                    cmd.Parameters.AddWithValue("p_total_time", TotalTime);
                    cmd.Parameters.AddWithValue("p_isActive", IsActive);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // ========================
        // 🟠 UPDATE
        // ========================
        public void Update()
        {
            ExecuteSafe(conn =>
            {
                using (var cmd = new MySqlCommand("sp_optas_layout_update", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_id", Id);
                    cmd.Parameters.AddWithValue("p_kari_id", KariId);
                    cmd.Parameters.AddWithValue("p_station", Station);
                    cmd.Parameters.AddWithValue("p_worker_id", WorkerId);
                    cmd.Parameters.AddWithValue("p_current_page", CurrentPage);
                    cmd.Parameters.AddWithValue("p_total_pages", TotalPages);
                    cmd.Parameters.AddWithValue("p_total_time", TotalTime);
                    cmd.Parameters.AddWithValue("p_isActive", IsActive);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // ========================
        // 🔴 DELETE
        // ========================
        public static void Delete(int id)
        {
            ExecuteSafe(conn =>
            {
                using (var cmd = new MySqlCommand("sp_optas_layout_delete", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_id", id);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // ========================
        // 🔍 SEARCH
        // ========================
        public static List<OptasLayout> Search(string kariId = null, int? station = null, bool? isActive = null)
        {
            var list = new List<OptasLayout>();

            ExecuteSafe(conn =>
            {
                using (var cmd = new MySqlCommand("sp_optas_layout_search", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_kari_id", (object)kariId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("p_station", (object)station ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("p_isActive", (object)isActive ?? DBNull.Value);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            list.Add(Read(reader));
                    }
                }
            });

            return list;
        }

        // ========================
        // 🔄 UPSERT
        // ========================
        public void Upsert()
        {
            ExecuteSafe(conn =>
            {
                using (var cmd = new MySqlCommand("sp_optas_layout_upsert", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("p_kari_id", KariId);
                    cmd.Parameters.AddWithValue("p_station", Station);
                    cmd.Parameters.AddWithValue("p_worker_id", WorkerId);
                    cmd.Parameters.AddWithValue("p_current_page", CurrentPage);
                    cmd.Parameters.AddWithValue("p_total_pages", TotalPages);
                    cmd.Parameters.AddWithValue("p_total_time", TotalTime);
                    cmd.Parameters.AddWithValue("p_isActive", IsActive);
                    cmd.ExecuteNonQuery();
                }
            });
        }

        // ========================
        // ⚙️ Map Reader → Object
        // ========================
        private static OptasLayout Read(MySqlDataReader r)
        {
            return new OptasLayout
            {
                Id = Convert.ToInt32(r["id"]),
                KariId = r["kari_id"]?.ToString(),
                Station = Convert.ToInt32(r["station"]),
                WorkerId = r["worker_id"]?.ToString(),
                CurrentPage = Convert.ToInt32(r["current_page"]),
                TotalPages = Convert.ToInt32(r["total_pages"]),
                TotalTime = Convert.ToInt32(r["total_time"]),
                IsActive = Convert.ToBoolean(r["isActive"]),
                UpdateDate = Convert.ToDateTime(r["update_date"]),
                CreateDate = Convert.ToDateTime(r["create_date"])
            };
        }
    }
}

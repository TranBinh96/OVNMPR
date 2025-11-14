using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PDFV5_forWin_for.Initializer
{
    public class ConfigHelper
    {
        private static string configPath = "config.ini";

        // 🧩 Hàm kiểm tra/tạo file config
        public static void EnsureConfigExists(string path = null)
        {
            if (path != null) configPath = path;

            try
            {
                if (!File.Exists(configPath))
                {
                    Console.WriteLine("⚠️ config.ini chưa tồn tại. Đang tạo mới...");
                    using (StreamWriter sw = File.CreateText(configPath))
                    {
                        sw.WriteLine("[Setting]");
                        sw.WriteLine("OptasUrl=Server=10.216.28.11;Port=5432;Database=optas;UserId=ovnm;SearchPath=ovnm;Password=ovnm;Pooling=True");
                        sw.WriteLine("db_line=Server=10.216.28.11;Database=db_pas_prod;User ID=ovnm;Password=P@ssw0rd;CharSet=utf8mb4;");
                        sw.WriteLine("optas_forder_url=\\\\10.216.28.11\\optas\\work");
                        sw.WriteLine("[Master]");
                        sw.WriteLine("UnitID=");
                        sw.WriteLine("WorkerID=VN20000");
                        sw.WriteLine("Station=1");
                        sw.WriteLine("Proficiency=Beginner");
                        sw.WriteLine("TotalPages=0");
                        sw.WriteLine("CurrentPage=0");
                        sw.WriteLine("SL=0");
                        sw.WriteLine("Today=0");
                    }
                }
                else
                {
                    Console.WriteLine("✅ config.ini đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi kiểm tra hoặc tạo config.ini: {ex.Message}");
            }
        }

        // 📖 Hàm đọc config trả về Dictionary
        public static Dictionary<string, string> ReadConfig(string path = null)
        {
            if (path != null) configPath = path;

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (!File.Exists(configPath))
            {
                Console.WriteLine("⚠️ Không tìm thấy file config.ini.");
                return result;
            }

            foreach (var line in File.ReadAllLines(configPath))
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("["))
                    continue;

                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                    result[parts[0].Trim()] = parts[1].Trim();
            }

            return result;
        }

        public static string ReadValue(string key, string path = "config.ini")
        {
            var data = ReadConfig(path);
            return data.ContainsKey(key) ? data[key] : string.Empty;
        }

        public static void UpdateConfigValue(string key, string value, string path = "config.ini")
        {
            try
            {
                // Đảm bảo file tồn tại
                EnsureConfigExists(path);

                // Đọc toàn bộ nội dung file
                var lines = File.ReadAllLines(path).ToList();
                bool updated = false;

                for (int i = 0; i < lines.Count; i++)
                {
                    // Bỏ qua dòng section như [Master]
                    if (lines[i].StartsWith("["))
                        continue;

                    if (lines[i].StartsWith(key + "=", StringComparison.OrdinalIgnoreCase))
                    {
                        lines[i] = $"{key}={value}";
                        updated = true;
                        break;
                    }
                }

                // Nếu chưa có key → thêm mới ở cuối
                if (!updated)
                    lines.Add($"{key}={value}");

                // Ghi lại file
                File.WriteAllLines(path, lines);
                Console.WriteLine($"✅ Đã cập nhật {key}={value} trong config.ini");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi cập nhật config.ini: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        public static void SaveMaster(string unitID, string workerID, string station, string proficiency, string path = "config.ini")
        {
            try
            {
                // Nếu file chưa tồn tại → tạo mới
                EnsureConfigExists(path);

                // Đọc tất cả dòng trong file
                var lines = File.ReadAllLines(path).ToList();

                // Các key cần cập nhật
                var updates = new Dictionary<string, string>
                {
                    { "UnitID", unitID },
                    { "WorkerID", workerID },
                    { "Station", station },
                    { "Proficiency", proficiency }
                };

                // Cập nhật hoặc thêm mới
                foreach (var kv in updates)
                {
                    bool found = false;
                    for (int i = 0; i < lines.Count; i++)
                    {
                        if (lines[i].StartsWith(kv.Key + "=", StringComparison.OrdinalIgnoreCase))
                        {
                            lines[i] = $"{kv.Key}={kv.Value}";
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        lines.Add($"{kv.Key}={kv.Value}");
                }

                // Ghi lại file
                File.WriteAllLines(path, lines);
                Console.WriteLine("✅ Đã lưu config.ini thành công!");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi ghi config.ini: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


    }
}

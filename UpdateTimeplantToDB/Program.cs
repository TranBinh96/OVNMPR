using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.OleDb;

namespace PDFV5_forWin_for.Sync
{
    public class TimePlanSync
    {
        // 🔧 Connection strings
        private static string accessPath = @"C:\Time_plan\TimePlan.mdb";
        private static string accessConnStr =
            $@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={accessPath};Persist Security Info=False;";
        private static string mysqlConnStr =
            "Server=10.216.28.11;Database=db_pas_prod;User ID=ovnm;Password=P@ssw0rd";

        // ================================
        // 🔹 1️⃣ Đồng bộ bảng 機種別シフト表 → MySQL.timeplant
        // ================================
        public static void Sync()
        {
            try
            {
                Console.WriteLine("🔄 Starting sync from Access → MySQL (timeplant)...");

                using (OleDbConnection accessConn = new OleDbConnection(accessConnStr))
                {
                    accessConn.Open();
                    string query = "SELECT * FROM 機種別シフト表";
                    using (OleDbCommand cmd = new OleDbCommand(query, accessConn))
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        using (MySqlConnection mysqlConn = new MySqlConnection(mysqlConnStr))
                        {
                            mysqlConn.Open();

                            foreach (DataRow row in dt.Rows)
                            {
                                string machine_id = row["機種ID"]?.ToString() ?? "";
                                int st = Convert.ToInt32(row["ST"] ?? 0);
                                int production_qty = Convert.ToInt32(row["日産台数"] ?? 0);
                                int progress_tolerance = Convert.ToInt32(row["進捗許容誤差"] ?? 0);
                                string model_name = row["製番"]?.ToString() ?? "";
                                int shift_id = Convert.ToInt32(row["勤務シフトID"] ?? 0);
                                string structure_name = row["構成名"]?.ToString() ?? "";

                                string start_time = ParseDate(row["開始時刻"]);
                                string end_time = ParseDate(row["終了時刻"]);

                                // --- Check existing ---
                                string checkSql = "SELECT production_qty, progress_tolerance, model_name, shift_id, structure_name, start_time, end_time FROM timeplant WHERE machine_id=@machine_id AND st=@st";
                                bool exists = false;
                                bool needsUpdate = false;

                                using (MySqlCommand checkCmd = new MySqlCommand(checkSql, mysqlConn))
                                {
                                    checkCmd.Parameters.AddWithValue("@machine_id", machine_id);
                                    checkCmd.Parameters.AddWithValue("@st", st);
                                    using (var reader = checkCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            exists = true;
                                            if (
                                                Convert.ToInt32(reader["production_qty"]) != production_qty ||
                                                Convert.ToInt32(reader["progress_tolerance"]) != progress_tolerance ||
                                                reader["model_name"].ToString() != model_name ||
                                                Convert.ToInt32(reader["shift_id"]) != shift_id ||
                                                reader["structure_name"].ToString() != structure_name ||
                                                reader["start_time"].ToString() != start_time ||
                                                reader["end_time"].ToString() != end_time
                                               )
                                            {
                                                needsUpdate = true;
                                            }
                                        }
                                    }
                                }

                                if (!exists)
                                {
                                    string insertSql = @"
INSERT INTO timeplant (machine_id, st, production_qty, progress_tolerance, model_name, shift_id, structure_name, start_time, end_time)
VALUES (@machine_id, @st, @production_qty, @progress_tolerance, @model_name, @shift_id, @structure_name, @start_time, @end_time)";
                                    using (MySqlCommand insertCmd = new MySqlCommand(insertSql, mysqlConn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@machine_id", machine_id);
                                        insertCmd.Parameters.AddWithValue("@st", st);
                                        insertCmd.Parameters.AddWithValue("@production_qty", production_qty);
                                        insertCmd.Parameters.AddWithValue("@progress_tolerance", progress_tolerance);
                                        insertCmd.Parameters.AddWithValue("@model_name", model_name);
                                        insertCmd.Parameters.AddWithValue("@shift_id", shift_id);
                                        insertCmd.Parameters.AddWithValue("@structure_name", structure_name);
                                        insertCmd.Parameters.AddWithValue("@start_time", start_time);
                                        insertCmd.Parameters.AddWithValue("@end_time", end_time);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                    Console.WriteLine($"🟢 Inserted: {machine_id} - ST:{st}");
                                }
                                else if (needsUpdate)
                                {
                                    string updateSql = @"
UPDATE timeplant SET
    production_qty=@production_qty,
    progress_tolerance=@progress_tolerance,
    model_name=@model_name,
    shift_id=@shift_id,
    structure_name=@structure_name,
    start_time=@start_time,
    end_time=@end_time
WHERE machine_id=@machine_id AND st=@st;";
                                    using (MySqlCommand updateCmd = new MySqlCommand(updateSql, mysqlConn))
                                    {
                                        updateCmd.Parameters.AddWithValue("@machine_id", machine_id);
                                        updateCmd.Parameters.AddWithValue("@st", st);
                                        updateCmd.Parameters.AddWithValue("@production_qty", production_qty);
                                        updateCmd.Parameters.AddWithValue("@progress_tolerance", progress_tolerance);
                                        updateCmd.Parameters.AddWithValue("@model_name", model_name);
                                        updateCmd.Parameters.AddWithValue("@shift_id", shift_id);
                                        updateCmd.Parameters.AddWithValue("@structure_name", structure_name);
                                        updateCmd.Parameters.AddWithValue("@start_time", start_time);
                                        updateCmd.Parameters.AddWithValue("@end_time", end_time);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                    Console.WriteLine($"🟡 Updated: {machine_id} - ST:{st}");
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("✅ Sync (timeplant) completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during Sync: {ex.Message}");
            }
        }

        // ================================
        // 🔹 2️⃣ Đồng bộ bảng 機種マスタ → MySQL.machine_list
        // ================================
        public static void Sync_KariLineUnit()
        {
            try
            {
                Console.WriteLine("🔄 Starting sync from Access → MySQL (machine_list)...");

                using (OleDbConnection accessConn = new OleDbConnection(accessConnStr))
                {
                    accessConn.Open();
                    string query = "SELECT 機種ID, 機種名, ユニット名 FROM 機種マスタ";
                    using (OleDbCommand cmd = new OleDbCommand(query, accessConn))
                    using (OleDbDataAdapter adapter = new OleDbDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        using (MySqlConnection mysqlConn = new MySqlConnection(mysqlConnStr))
                        {
                            mysqlConn.Open();

                            foreach (DataRow row in dt.Rows)
                            {
                                string kari = row["機種ID"]?.ToString()?.Trim() ?? "";
                                string line = row["機種名"]?.ToString()?.Trim() ?? "";
                                string unitName = row["ユニット名"]?.ToString()?.Trim() ?? "";

                                string checkSql = "SELECT Line, UnitName FROM machine_list WHERE Kari = @kari";
                                bool exists = false;
                                bool needsUpdate = false;

                                using (MySqlCommand checkCmd = new MySqlCommand(checkSql, mysqlConn))
                                {
                                    checkCmd.Parameters.AddWithValue("@kari", kari);
                                    using (var reader = checkCmd.ExecuteReader())
                                    {
                                        if (reader.Read())
                                        {
                                            exists = true;
                                            string currentLine = reader["Line"]?.ToString()?.Trim() ?? "";
                                            string currentUnit = reader["UnitName"]?.ToString()?.Trim() ?? "";
                                            if (currentLine != line || currentUnit != unitName)
                                                needsUpdate = true;
                                        }
                                    }
                                }

                                if (!exists)
                                {
                                    string insertSql = "INSERT INTO machine_list (Kari, Line, UnitName) VALUES (@kari, @line, @unitName)";
                                    using (MySqlCommand insertCmd = new MySqlCommand(insertSql, mysqlConn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@kari", kari);
                                        insertCmd.Parameters.AddWithValue("@line", line);
                                        insertCmd.Parameters.AddWithValue("@unitName", unitName);
                                        insertCmd.ExecuteNonQuery();
                                    }
                                    Console.WriteLine($"🟢 Inserted: {kari} - {line}");
                                }
                                else if (needsUpdate)
                                {
                                    string updateSql = "UPDATE machine_list SET Line=@line, UnitName=@unitName WHERE Kari=@kari";
                                    using (MySqlCommand updateCmd = new MySqlCommand(updateSql, mysqlConn))
                                    {
                                        updateCmd.Parameters.AddWithValue("@kari", kari);
                                        updateCmd.Parameters.AddWithValue("@line", line);
                                        updateCmd.Parameters.AddWithValue("@unitName", unitName);
                                        updateCmd.ExecuteNonQuery();
                                    }
                                    Console.WriteLine($"🟡 Updated: {kari} - {line}");
                                }
                            }
                        }
                    }
                }

                Console.WriteLine("✅ Sync_KariLineUnit completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error during Sync_KariLineUnit: {ex.Message}");
            }
        }

        // Helper
        private static string ParseDate(object value)
        {
            if (value == null || value == DBNull.Value) return null;
            if (DateTime.TryParse(value.ToString(), out DateTime dt))
                return dt.ToString("yyyy-MM-dd HH:mm:ss");
            return null;
        }
    }

    // ================================
    // 🔸 MAIN TEST
    // ================================
    internal class Program
    {
        static void Main(string[] args)
        {
            TimePlanSync.Sync();              // Đồng bộ timeplant
            TimePlanSync.Sync_KariLineUnit(); // Đồng bộ machine_list
        }
    }
}

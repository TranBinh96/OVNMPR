using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModifileDataOptas.Models
{
    public class ProductionReport
    {
        // ===========================================
        // 🛠️ MÔ HÌNH DỮ LIỆU (PROPERTIES)
        // Đã thay đổi Start và End thành string
        // ===========================================

        public int STT { get; set; }
        public string Seiban { get; set; }
        public string ProductionOrderNumber { get; set; }
        public string Seq { get; set; }    
        public string Kari { get; set; }
        public string KariID { get; set; }
        public string WorkerID { get; set; }
        public string Station { get; set; }
        public string Start { get; set; } // Đã chuyển sang String
        public string End { get; set; }   // Đã chuyển sang String


        // ===========================================
        // 🔗 LOGIC TRUY VẤN (METHODS)
        // ===========================================

        private readonly string _connectionString =
           "Server=10.216.28.11;Port=5432;Database=optas;User Id=ovnm;Password=ovnm;SearchPath=ovnm;Pooling=true;";

        /// <summary>
        /// Lấy báo cáo sản xuất, lọc dữ liệu trong 2 tháng gần nhất và chấp nhận tham số lọc linh hoạt.
        /// </summary>
        public List<ProductionReport> GetProductionReports(string kariID, string seiban)
        {
            var reports = new List<ProductionReport>();

            // 1. Tính toán ngày bắt đầu lọc (4 tháng trước từ ngày hiện tại)
            DateTime filterDate = DateTime.Now.AddMonths(-2).Date; // Dựa trên mã bạn cung cấp

            // 2. Định nghĩa SQL Query
            string query = @"
                SELECT
                    ROW_NUMBER() OVER (ORDER BY T.製造オーダー, T.kariid, T.station, T.""Start"") AS STT,
                    T.製造オーダー AS seiban,
                    T.指図番号 AS ProductionOrderNumber,
                    T.seq,
                    T.kari,
                    T.kariid,
                    T.workerid,
                    T.station,
                    T.""Start"",
                    T.""End""
                FROM v_report_se_check T
                WHERE 
                    T.""Start"" >= @filterDate 
                    AND
                    (@kariID IS NULL OR @kariID = '' OR T.kariid = @kariID)
                    AND
                    (@seiban IS NULL OR @seiban = '' OR T.製造オーダー = @seiban)
                ORDER BY 
                    T.製造オーダー, T.kariid, T.station, T.""Start"" DESC;";

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 60; // Khắc phục Timeout

                    // --- 🛠️ THIẾT LẬP THAM SỐ ---
                    object kariIDValue = string.IsNullOrEmpty(kariID) ? (object)DBNull.Value : kariID;
                    object seibanValue = string.IsNullOrEmpty(seiban) ? (object)DBNull.Value : seiban;

                    cmd.Parameters.Add("kariID", NpgsqlTypes.NpgsqlDbType.Varchar).Value = kariIDValue;
                    cmd.Parameters.Add("seiban", NpgsqlTypes.NpgsqlDbType.Varchar).Value = seibanValue;
                    cmd.Parameters.AddWithValue("filterDate", NpgsqlTypes.NpgsqlDbType.Timestamp, filterDate);

                    // --- Thực thi và Đọc Dữ liệu ---
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var report = new ProductionReport
                            {
                                STT = reader.GetInt32(reader.GetOrdinal("STT")),
                                Seiban = reader["seiban"]?.ToString(),
                                Seq = reader["seq"]?.ToString(),
                                ProductionOrderNumber = reader["ProductionOrderNumber"]?.ToString(),
                                Kari = reader["kari"]?.ToString(),
                                KariID = reader["kariid"]?.ToString(),
                                WorkerID = reader["workerid"]?.ToString(),
                                Station = reader["station"]?.ToString(),

                                // Xử lý NULL và chuyển đổi sang String với định dạng ngày giờ đầy đủ
                                Start = reader.IsDBNull(reader.GetOrdinal("Start"))
                                        ? string.Empty // Gán chuỗi rỗng nếu là NULL
                                        : reader.GetDateTime(reader.GetOrdinal("Start")).ToString("dd/MM/yyyy HH:mm:ss"),

                                // Xử lý NULL và chuyển đổi sang String với định dạng ngày giờ đầy đủ
                                End = reader.IsDBNull(reader.GetOrdinal("End"))
                                      ? string.Empty // Gán chuỗi rỗng nếu là NULL
                                      : reader.GetDateTime(reader.GetOrdinal("End")).ToString("dd/MM/yyyy HH:mm:ss")
                            };
                            reports.Add(report);
                        }
                    }
                }
            }

            return reports;
        }
    }
}
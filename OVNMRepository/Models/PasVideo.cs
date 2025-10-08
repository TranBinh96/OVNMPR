using MySql.Data.MySqlClient;
using OVNMRepository.DataContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVNMRepository.Models
{
    public class PasVideo
    {
        public int STT { get; set; }
        public string KariId { get; set; }
        public int Station { get; set; }
        public string Hostname { get; set; }
        public string AddressVideo { get; set; }
        public string FileName { get; set; }
        public DateTime StartTime { get; set; }
        public List<PasVideo> GetPasVideo(
        string hostname = null,
        string kariId = null,
        int? station = null,
        DateTime? startTime = null,
        DateTime? endTime = null)
        {
            var parameters = new Dictionary<string, object>
            {
                {"p_Hostname", (object)hostname ?? DBNull.Value},
                {"p_KariId", (object)kariId ?? DBNull.Value},
                {"p_Station", (object)station ?? DBNull.Value},
                {"p_StartTime", startTime.HasValue ? FormatDateForMySQL(startTime.Value) : (object)DBNull.Value},
                {"p_EndTime", endTime.HasValue ? FormatDateForMySQL(endTime.Value) : (object)DBNull.Value}
            };

            DataTable dt = DatabaseConnections.dbMySQL.ExecuteQuery(
                "GetPasVideo", parameters, isStoredProc: true);

            var list = new List<PasVideo>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PasVideo
                {
                    STT = row.GetValueOrDefault<int>("STT"),
                    KariId = row.GetValueOrDefault<string>("KariId"),
                    Station = row.GetValueOrDefault<int>("Station"),
                    Hostname = row.GetValueOrDefault<string>("Hostname"),
                    AddressVideo = row.GetValueOrDefault<string>("AddressVideo"),
                    FileName = row.GetValueOrDefault<string>("FileName"),
                    StartTime = row.GetValueOrDefault<DateTime>("StartTime", DateTime.MinValue)
                });
            }

            return list;
        }

        private string FormatDateForMySQL(DateTime dt)
        {
            // Định dạng giống SQL test: 2025-09-25_10-48-00
            return dt.ToString("yyyy-MM-dd_HH-mm-ss");
        }



    }
}

using OVNMRepository.DataContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVNMRepository.Models
{

    public class ComputerLine
    {
        public string txtSerinumber;

        public int STT { get; set; }
        public int Id { get; set; }
        public string LineName { get; set; }
        public string UnitName { get; set; }
        public string Station { get; set; }
        public string SerialNumber { get; set; }
        public string Information { get; set; }
        public string HostName { get; set; }
        public string AddressIP { get; set; }
        public string FullName { get; set; }
        public string DomainName { get; set; }
        public string Rage { get; set; }
        public bool OperatingStatus { get; set; }
        public string PersonCharge { get; set; }
        public string Note { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string HostnameHistory { get; set; }
        public int? Running { get; set; }

        public List<ComputerLine> GetComputerLines()
        {
            string sql = @"
            SELECT 
            ROW_NUMBER() OVER(
                ORDER BY 
                    CASE WHEN line_name = 'IT' THEN 1 ELSE 0 END, 
                    line_name
            ) AS STT,
            id,
            line_name,
            unit_name,
            station,
            serial_number,
            information_pc,
            hostname,
            address_ip,
            full_name,
            domain_name,
            rage,
            operating_status,
            person_charge,
            note,
            create_date,
            update_date,
            hostname_history,
            Running
            FROM p_line_computer
            WHERE domain_name = 'No'
            ORDER BY 
            CASE WHEN line_name = 'IT' THEN 1 ELSE 0 END, 
            line_name;";

            DataTable dt = DatabaseConnections.dbMSSQL.ExecuteQuery(sql);

            var list = new List<ComputerLine>();
            foreach (DataRow row in dt.Rows)
            {
                var item = new ComputerLine
                {
                    STT = Convert.ToInt32(row["STT"]),
                    Id = row["id"] == DBNull.Value ? 0 : Convert.ToInt32(row["id"]),
                    LineName = row["line_name"] == DBNull.Value ? null : row["line_name"].ToString(),
                    UnitName = row["unit_name"] == DBNull.Value ? null : row["unit_name"].ToString(),
                    Station = row["station"] == DBNull.Value ? null : row["station"].ToString(),
                    SerialNumber = row["serial_number"] == DBNull.Value ? null : row["serial_number"].ToString(),
                    Information = row["information_pc"] == DBNull.Value ? null : row["information_pc"].ToString(),
                    HostName = row["hostname"] == DBNull.Value ? null : row["hostname"].ToString(),
                    AddressIP = row["address_ip"] == DBNull.Value ? null : row["address_ip"].ToString(),
                    FullName = row["full_name"] == DBNull.Value ? null : row["full_name"].ToString(),
                    DomainName = row["domain_name"] == DBNull.Value ? null : row["domain_name"].ToString(),
                    Rage = row["rage"] == DBNull.Value ? null : row["rage"].ToString(),
                    OperatingStatus = row["operating_status"] != DBNull.Value && Convert.ToBoolean(row["operating_status"]),
                    PersonCharge = row["person_charge"] == DBNull.Value ? null : row["person_charge"].ToString(),
                    Note = row["note"] == DBNull.Value ? null : row["note"].ToString(),
                    CreateDate = row["create_date"] == DBNull.Value ? null : (DateTime?)row["create_date"],
                    UpdateDate = row["update_date"] == DBNull.Value ? null : (DateTime?)row["update_date"],
                    HostnameHistory = row["hostname_history"] == DBNull.Value ? null : row["hostname_history"].ToString(),
                    Running = row["Running"] == DBNull.Value ? null : (int?)Convert.ToInt32(row["Running"])
                };
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        /// Upsert: Nếu chưa có (serial_number + hostname) thì Insert, có rồi thì Update
        /// </summary>
        public bool UpsertComputerLine(ComputerLine line)
        {
            string sql = @"
                IF EXISTS (SELECT 1 FROM p_line_computer WHERE serial_number = @SerialNumber AND hostname = @HostName)
                BEGIN
                    UPDATE p_line_computer
                    SET line_name = @LineName,
                        unit_name = @UnitName,
                        station = @Station,
                        address_ip = @AddressIP,
                        update_date = GETDATE()
                    WHERE serial_number = @SerialNumber AND hostname = @HostName
                END
                ELSE
                BEGIN
                    INSERT INTO p_line_computer (line_name, unit_name, station, hostname, address_ip, serial_number, update_date)
                    VALUES (@LineName, @UnitName, @Station, @HostName, @AddressIP, @SerialNumber, GETDATE())
                END";

                    var parameters = new Dictionary<string, object>
            {
                {"@LineName", line.LineName},
                {"@UnitName", line.UnitName},
                {"@Station", line.Station},
                {"@HostName", line.HostName},
                {"@AddressIP", line.AddressIP},
                {"@SerialNumber", line.SerialNumber}
            };

            int rows = DatabaseConnections.dbMSSQL.ExecuteNonQuery(sql, parameters);
            return rows > 0; // true nếu có update/insert
        }



        /// <summary>
        /// Xóa theo SerialNumber + HostName
        /// </summary>
        public bool DeleteComputerLine(string serialNumber, string hostName)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@serial_number", serialNumber},
                {"@hostname", hostName}
            };

            DataTable dt = DatabaseConnections.dbMSSQL.ExecuteQuery("sp_DeleteByKey_p_line_computer", parameters, isStoredProc: true);

            return dt.Rows.Count > 0;
        }

        public bool ResetDefaultComputerLine(string serialNumber, string hostName)
        {
            try
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@SerialNumber", serialNumber},
                    {"@HostName", hostName}
                };

                int rowsAffected = DatabaseConnections.dbMSSQL.ExecuteNonQuery(
                    "ResetDefaultComputerLine",
                    parameters,
                    isStoredProc: true);

                Console.WriteLine($"Rows affected = {rowsAffected}");
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi reset default: " + ex.Message, ex);
            }
        }




    }
}

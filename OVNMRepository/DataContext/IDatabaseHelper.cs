using MySql.Data.MySqlClient;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVNMRepository.DataContext
{
    public interface IDatabaseHelper
    {
        int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false);
        DataTable ExecuteQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false);
    }

    // ---------------- MSSQL ----------------
    public class SqlServerHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public SqlServerHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(commandText, conn))
            {
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        



        public DataTable ExecuteQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(commandText, conn))
            {
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

                using (var da = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }

    // ---------------- MySQL ----------------
    public class MySqlHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public MySqlHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false)
        {
            using (var conn = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(commandText, conn))
            {
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public DataTable ExecuteQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false)
        {
            using (var conn = new MySqlConnection(_connectionString))
            using (var cmd = new MySqlCommand(commandText, conn))
            {
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

                using (var da = new MySqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }

    // ---------------- PostgreSQL ----------------
    public class PostgresHelper : IDatabaseHelper
    {
        private readonly string _connectionString;

        public PostgresHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        public DataTable ExecuteQuery(string commandText, Dictionary<string, object> parameters = null, bool isStoredProc = false)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            using (var cmd = new NpgsqlCommand(commandText, conn))
            {
                cmd.CommandType = isStoredProc ? CommandType.StoredProcedure : CommandType.Text;
                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);

                using (var da = new NpgsqlDataAdapter(cmd)) // ⚠️ NpgsqlDataAdapter cần cài package NpgsqlDataAdapter
                {
                    var dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }
    }

    // ---------------- Factory ----------------
    public static class DatabaseFactory
    {
        public static IDatabaseHelper Create(string dbType, string connectionString)
        {
            switch (dbType.ToUpper())
            {
                case "MSSQL":
                    return new SqlServerHelper(connectionString);
                case "MYSQL":
                    return new MySqlHelper(connectionString);
                case "POSTGRES":
                    return new PostgresHelper(connectionString);
                default:
                    throw new ArgumentException("Unsupported database type");
            }
        }
    }
}

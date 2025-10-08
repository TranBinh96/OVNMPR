using OVNMRepository.DataContext;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OVNMRepository.Models
{
    public class Users
    {
        public int UserId { get; set; }            // Khóa chính
        public string UserName { get; set; }       // Tên đăng nhập
        public string PasswordHash { get; set; }   // Mật khẩu (hash/salted)
        public string FullName { get; set; }       // Họ và tên
        public string Email { get; set; }          // Email
        public string Role { get; set; }           // Quyền: Admin/User/Viewer
        public bool IsActive { get; set; }         // Trạng thái tài khoản
        public DateTime CreateDate { get; set; }   // Ngày tạo
        public DateTime? LastLogin { get; set; }   // Lần đăng nhập cuối

        public Users CreateUser(Users user)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@Action", "CREATE"},
                {"@UserId", DBNull.Value},
                {"@UserName", user.UserName},
                {"@PasswordHash", user.PasswordHash},
                {"@FullName", user.FullName},
                {"@Email", user.Email},
                {"@Role", user.Role},
                {"@IsActive", user.IsActive}
            };

            var dt = DatabaseConnections.dbMSSQL.ExecuteQuery("Users_CRUD", parameters, true);
            return MapToUsers(dt).FirstOrDefault();
        }

        // READ
        public Users GetUserById(int userId)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@Action", "READ"},
                {"@UserId", userId}
            };

            var dt = DatabaseConnections.dbMSSQL.ExecuteQuery("Users_CRUD", parameters, true);
            return MapToUsers(dt).FirstOrDefault();
        }

        // UPDATE
        public Users UpdateUser(Users user)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@Action", "UPDATE"},
                {"@UserId", user.UserId},
                {"@UserName", user.UserName},
                {"@PasswordHash", user.PasswordHash},
                {"@FullName", user.FullName},
                {"@Email", user.Email},
                {"@Role", user.Role},
                {"@IsActive", user.IsActive}
            };

            var dt = DatabaseConnections.dbMSSQL.ExecuteQuery("Users_CRUD", parameters, true);
            return MapToUsers(dt).FirstOrDefault();
        }

        // DELETE
        public bool DeleteUser(Users user)
        {
            var parameters = new Dictionary<string, object>
            {
                {"@Action", "DELETE"},
                {"@UserId", user.UserId},
                {"@UserName", user.UserName}
            };

            int rows = DatabaseConnections.dbMSSQL.ExecuteNonQuery("Users_CRUD", parameters, true);
            return rows > 0;
        }

        // LIST
        public List<Users> GetAllUsers()
        {
            var parameters = new Dictionary<string, object>
            {
                {"@Action", "LIST"}
            };

            var dt = DatabaseConnections.dbMSSQL.ExecuteQuery("Users_CRUD", parameters, true);
            return MapToUsers(dt);
        }

        // MAP từ DataTable -> List<Users>
        private List<Users> MapToUsers(DataTable dt)
        {
            var list = new List<Users>();
            foreach (DataRow row in dt.Rows)
            {
                var u = new Users
                {
                    UserId = row["UserId"] == DBNull.Value ? 0 : Convert.ToInt32(row["UserId"]),
                    UserName = row["UserName"]?.ToString(),
                    PasswordHash = row["PasswordHash"]?.ToString(),
                    FullName = row["FullName"]?.ToString(),
                    Email = row["Email"]?.ToString(),
                    Role = row["Role"]?.ToString(),
                    IsActive = row["IsActive"] != DBNull.Value && Convert.ToBoolean(row["IsActive"]),
                    LastLogin = row["LastLogin"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(row["LastLogin"])
                };
                list.Add(u);
            }
            return list;
        }


        public string Login(string username, string password, out string role)
        {
            role = string.Empty;
            var parameters = new Dictionary<string, object>
                {
                    {"@UserName", username},
                    {"@Password", password} // ⚠️ sau này đổi thành PasswordHash
                };

            DataTable dt = DatabaseConnections.dbMSSQL.ExecuteQuery(
                "UserLogin", parameters, isStoredProc: true);

            if (dt.Rows.Count > 0)
            {
                role = dt.Rows[0]["Role"].ToString();
                return role;
            }
            return null;
        }


    }
}

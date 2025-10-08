using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OVNMRepository.DataContext
{
    public static class DataRowExtensions
    {
        /// <summary>
        /// Lấy giá trị từ DataRow, nếu NULL thì trả về defaultValue
        /// </summary>
        public static T GetValueOrDefault<T>(this DataRow row, string columnName, T defaultValue = default)
        {
            if (!row.Table.Columns.Contains(columnName) || row[columnName] == DBNull.Value)
                return defaultValue;

            return (T)Convert.ChangeType(row[columnName], typeof(T));
        }
    }
}

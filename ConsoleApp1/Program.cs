using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Data.OleDb;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
        kari();
    }



    public static void kari()
    {
        string folderPath = @"\\10.216.28.26\ovnm_pe\新しいフォルダー\";

        // Lấy tất cả file .csv
        string[] files = Directory.GetFiles(folderPath, "*.csv", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            Console.WriteLine("Đang xử lý: " + file);
            string[] lines = File.ReadAllLines(file, Encoding.UTF8);

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] cols = SplitCsv(line);

                // nếu có ít nhất 2 cột
                if (cols.Length > 1)
                {
                    // nếu cột 2 = "11" → đổi thành "12"
                    if (cols[1].Trim() == "13")
                    {
                        cols[1] = "12";
                    }

                    // ghép lại
                    lines[i] = string.Join(",", cols);
                }
            }

            // Ghi đè lại file
            File.WriteAllLines(file, lines, Encoding.UTF8);
        }

        Console.WriteLine("Hoàn thành!");
        Console.ReadLine();
    }

    // Hàm tách CSV chuẩn (giữ nguyên dấu , trong dấu ")
    static string[] SplitCsv(string input)
    {
        var result = new List<string>();
        bool insideQuotes = false;
        StringBuilder current = new StringBuilder();

        foreach (char c in input)
        {
            if (c == '"')
            {
                insideQuotes = !insideQuotes;
            }
            else if (c == ',' && !insideQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result.ToArray();
    }
}

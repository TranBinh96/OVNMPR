using System;
using System.IO;

namespace TransactionProcessing
{
    internal class Program
    {
        static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // ===== Read config.txt =====
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.txt");
            if (!File.Exists(configPath))
            {
                Console.WriteLine("config.txt not found!");
                return;
            }

            string SRC = "";
            string DEST = "";

            foreach (var line in File.ReadAllLines(configPath))
            {
                if (line.StartsWith("SRC="))
                    SRC = line.Substring(4).Trim();
                if (line.StartsWith("DEST="))
                    DEST = line.Substring(5).Trim();
            }

            if (!Directory.Exists(SRC))
            {
                Console.WriteLine("SRC folder not found: " + SRC);
                return;
            }

            if (!Directory.Exists(DEST))
                Directory.CreateDirectory(DEST);

            Console.WriteLine("SRC  = " + SRC);
            Console.WriteLine("DEST = " + DEST);
            Console.WriteLine("============================================");

            // ===== ALL IMAGE EXTENSIONS =====
            string[] extensions = { "*.jpg", "*.jpeg", "*.png" };

            foreach (var ext in extensions)
            {
                var files = Directory.GetFiles(SRC, ext);

                foreach (var fullFile in files)
                {
                    string fileName = Path.GetFileName(fullFile);

                    // Lấy Date Modified
                    DateTime lastWrite = File.GetLastWriteTime(fullFile);

                    // ===== Folder theo tháng: YYYY_MM =====
                    string monthFolderName = lastWrite.ToString("yyyy_MM");
                    string MONTH_FOLDER = Path.Combine(DEST, monthFolderName);
                    if (!Directory.Exists(MONTH_FOLDER))
                        Directory.CreateDirectory(MONTH_FOLDER);

                    // ===== Folder theo ngày: dd =====
                    string dayFolderName = lastWrite.ToString("dd");
                    string DAY_FOLDER = Path.Combine(MONTH_FOLDER, dayFolderName);
                    if (!Directory.Exists(DAY_FOLDER))
                        Directory.CreateDirectory(DAY_FOLDER);

                    Console.WriteLine(
                        $"File: {fileName}  Modified: {lastWrite:yyyy-MM-dd HH:mm:ss}  -> {monthFolderName}\\{dayFolderName}");

                    // ===== Log file =====
                    string logName = $"log_{lastWrite:yyyy_MM_dd}.txt";
                    string logPath = Path.Combine(DAY_FOLDER, logName);

                    // ===== Destination =====
                    string destPath = Path.Combine(DAY_FOLDER, fileName);

                    try
                    {
                        if (File.Exists(destPath))
                            File.Delete(destPath);  // overwrite

                        File.Move(fullFile, destPath);
                    }
                    catch (Exception ex)
                    {
                        File.AppendAllText(logPath,
                            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR moving {fileName}: {ex.Message}\r\n");
                        continue;
                    }

                    // ===== Log success =====
                    File.AppendAllText(logPath,
                        $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MOVED {fileName} -> {destPath}\r\n");
                }
            }

            Console.WriteLine("============================================");
            Console.WriteLine("DONE. ALL FILES MOVED SUCCESSFULLY.");
            Console.WriteLine("============================================");
        }
    }
}

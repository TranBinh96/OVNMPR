using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace PDFV5_forWin_for
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "Global\\PDFV5_forWin_for_Mutex", out createdNew))
            {
                if (!createdNew)
                {
                    // Nếu app đã chạy, focus lên cửa sổ cũ
                    foreach (var process in Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName))
                    {
                        if (process.Id != Process.GetCurrentProcess().Id)
                        {
                            NativeMethods.ShowWindow(process.MainWindowHandle, 9);
                            NativeMethods.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // 🚀 Kiểm tra cập nhật
                AutoUpdater.CheckForUpdate();

                Application.Run(new XtraMain());
            }
        }

        private static class NativeMethods
        {
            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool SetForegroundWindow(IntPtr hWnd);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        }
    }

    // ================================================
    // 🚀 CLASS TỰ ĐỘNG CẬP NHẬT PHẦN MỀM
    // ================================================
    public static class AutoUpdater
    {
        // 📁 Đường dẫn đến thư mục chứa bản cập nhật trên server
        private static readonly string serverFolder = @"\\10.216.28.11\okipevn\APP_PAS\PDFV5_forWin_for";
        private static readonly string versionFile = Path.Combine(serverFolder, "version.txt");
        private static readonly string exeFile = Path.Combine(serverFolder, "PDFV5_forWin_for.exe");

        public static void CheckForUpdate()
        {
            try
            {
                if (!File.Exists(exeFile) || !File.Exists(versionFile))
                    return; // Không có file cập nhật

                string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                string latestVersion = ParseVersionFromFile(versionFile);

                if (IsNewerVersion(latestVersion, currentVersion))
                {
                    string notes = ParseNotesFromFile(versionFile);
                    var msg = $"🔄 Có bản cập nhật mới ({latestVersion}) so với bản hiện tại ({currentVersion})." +
                              $"\n\n{notes}\n\nBạn có muốn cập nhật không?";

                    if (MessageBox.Show(msg, "Cập nhật phần mềm", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        PerformUpdate();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"⚠ Lỗi kiểm tra cập nhật: {ex.Message}",
                    "Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string ParseVersionFromFile(string path)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("version=", StringComparison.OrdinalIgnoreCase))
                    return line.Split('=')[1].Trim();
            }
            return "0.0.0";
        }

        private static string ParseNotesFromFile(string path)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("notes=", StringComparison.OrdinalIgnoreCase))
                    return line.Substring(6).Trim();
            }
            return "";
        }

        private static bool IsNewerVersion(string remote, string local)
        {
            Version vRemote = new Version(remote);
            Version vLocal = new Version(local);
            return vRemote > vLocal;
        }

        private static void PerformUpdate()
        {
            try
            {
                string localPath = Application.ExecutablePath;
                string tempPath = localPath + ".new";
                string backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");

                Directory.CreateDirectory(backupDir);
                string backupPath = Path.Combine(backupDir, $"PDFV5_forWin_for_{DateTime.Now:yyyyMMdd_HHmmss}.exe");
                File.Copy(localPath, backupPath, true);

                // Copy file mới từ server về
                File.Copy(exeFile, tempPath, true);

                // Dùng CMD để thay thế file sau khi thoát
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C timeout 1 && del \"{localPath}\" && ren \"{tempPath}\" \"{Path.GetFileName(localPath)}\" && start \"\" \"{localPath}\"",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(psi);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi cập nhật: {ex.Message}",
                    "Updater Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}

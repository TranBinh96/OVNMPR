using OpenCvSharp;
using PDFV5_forWin_for.Data;
using PDFV5_forWin_for.Initializer;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Point = OpenCvSharp.Point;

namespace PDFV5_forWin_for.UserControl
{
    public partial class XtraUserCameraView : DevExpress.XtraEditors.XtraUserControl
    {
        // ===== Win32 API hỗ trợ hiển thị cửa sổ =====
        private const int SW_RESTORE = 9;
        [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;

        // ===== Biến dùng chung =====
        private VideoWriter writer;
        private VideoCapture currentCapture;
        private Thread cameraThread;
        private readonly object cameraLock = new object();
        private bool isRunning = false;
        private System.Windows.Forms.Timer timerCheckTime;

        public bool Syuuryou = false;
        public string start_time, end_time, device_camera_name, namevideo;
        public string SerialNumber = string.Empty;
        public int devices_camera_id = -1;
        public int MaenoFun = -1;

        public IniFile ini_top = new IniFile(@"C:\PasOpencv+\config.ini");

        // ===== Khởi tạo =====
        public XtraUserCameraView()
        {
            InitializeComponent();
            LoadStart();

            string primaryEthernetMac = GetPrimaryEthernetMac();
            SerialNumber = string.IsNullOrWhiteSpace(GetSerialNumber().Trim()) || GetSerialNumber().StartsWith("Unknown")
                ? primaryEthernetMac
                : GetSerialNumber();
            SerialNumber = SerialNumber.Replace(":", "");
        }

        private void LoadStart()
        {
            SetupTimer();

            start_time = ini_top["TIME", "start"];
            end_time = ini_top["TIME", "end"];
            device_camera_name = ini_top["DEVICE", "devices_camera_id"];
            devices_camera_id = GetCameraIndex(device_camera_name);

            Directory.CreateDirectory(@"C:\PasOpencv+");
            Directory.CreateDirectory(@"C:\PasOpencv+\rec");
        }

        // ===== Timer kiểm tra ghi hình mỗi giây =====
        private void SetupTimer()
        {
            timerCheckTime = new System.Windows.Forms.Timer();
            timerCheckTime.Interval = 1000;
            timerCheckTime.Tick += TimerCheckTime_Tick;
            timerCheckTime.Start();
        }

        private void TimerCheckTime_Tick(object sender, EventArgs e)
        {
            if (isRunning) return;

            isRunning = true;
            cameraThread = new Thread(() =>
            {
                RokugaKaisi();
                isRunning = false;
            })
            {
                IsBackground = true
            };
            cameraThread.Start();
        }

        // ======== CHỨC NĂNG CHÍNH ========
        public void RokugaKaisi()
        {
            int width = 1280, height = 720;
            int camId = devices_camera_id;
            int backupCam = camId == 0 ? 1 : 0;

            lock (cameraLock)
            {
                using (VideoCapture videoCapture = new VideoCapture())
                using (Mat mat = new Mat())
                {
                    currentCapture = videoCapture;

                    try
                    {
                        CameraHelper.CloseCamera(backupCam);

                        // 🎥 Mở camera
                        // 🎥 Mở camera
                        videoCapture.Open(camId);

                        if (!videoCapture.IsOpened())
                        {
                            Console.WriteLine($"⚠️ Camera {camId} không khả dụng, thử {backupCam}...");
                            videoCapture.Open(backupCam);
                        }
                        if (!videoCapture.IsOpened())
                        {
                            SafeUpdateLabel(la_joutai, "❌ Không thể mở camera", Color.Red);
                            return;
                        }

                        videoCapture.FrameWidth = width;
                        videoCapture.FrameHeight = height;

                        // 🔥 Làm nóng camera
                        for (int i = 0; i < 3; i++)
                        {
                            videoCapture.Read(mat);
                            Thread.Sleep(50);
                        }

                        SafeUpdateLabel(la_joutai, "✔ CHECK RECORDING", Color.Green);

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();
                        MaenoFun = -1;
                        int retryCount = 0;

                        while (!Syuuryou)
                        {
                            if (ini_top["DEVICE", "recoding"].ToLower() != "on")
                            {
                                SafeUpdateLabel(la_joutai, "Recording OFF", Color.Gray);
                                Thread.Sleep(500);
                                continue;
                            }

                            DateTime now = DateTime.Now;

                            // 🔁 Tạo file video mới mỗi phút
                            if (MaenoFun != now.Minute)
                            {
                                namevideo = ini_top["DEVICE", "namevideo"];
                                string seri = SerialNumber;
                                string fileName = $"{now:yyyy-MM-dd_HH-mm-ss}_{namevideo}_{seri}_{Environment.MachineName.ToUpper()}.avi";
                                string filePath = Path.Combine(@"C:\PasOpencv+\rec\", fileName.Replace("_null", ""));

                                writer?.Dispose();
                                writer = new VideoWriter(filePath, FourCC.XVID, 10.0, new OpenCvSharp.Size(width / 2, height / 2));

                                SafeUpdateLabel(label1, namevideo?.ToUpper().Replace("_", " ") ?? "NO NAME", Color.DarkBlue);
                                MaenoFun = now.Minute;
                                stopwatch.Restart();
                                retryCount = 0;
                            }

                            if (!videoCapture.Read(mat) || mat.Empty())
                            {
                                retryCount++;
                                if (retryCount > 100)
                                {
                                    Console.WriteLine("⚠️ Camera không có tín hiệu, dừng ghi hình.");
                                    break;
                                }
                                Thread.Sleep(50);
                                continue;
                            }

                            retryCount = 0;
                            Cv2.PutText(mat, now.ToString("dd-MM-yy HH:mm:ss"), new Point(10, 40),
                                        HersheyFonts.HersheySimplex, 1, Scalar.White, 2, LineTypes.AntiAlias);
                            Cv2.Resize(mat, mat, new OpenCvSharp.Size(640, 360));

                            SafeUpdateCameraImage(pb_cam1, mat);
                            writer?.Write(mat);

                            if (stopwatch.Elapsed.TotalSeconds >= 60)
                            {
                                writer?.Dispose();
                                writer = null;
                                MaenoFun = -1;
                                stopwatch.Restart();
                            }

                            Thread.Sleep(10);
                        }

                        SafeUpdateLabel(la_joutai, "✔ CHECK STOPPED", Color.Orange);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Error during recording: {ex.Message}");
                        SafeUpdateLabel(la_joutai, "❌ Recording Error", Color.Red);
                    }
                    finally
                    {
                        try
                        {
                            writer?.Dispose();
                            videoCapture.Release();
                            mat.Dispose();
                        }
                        catch { }
                        currentCapture = null;
                    }
                }
            }
        }

        // ====== HÀM HỖ TRỢ ======
        private void SafeUpdateLabel(Label lbl, string text, Color color)
        {
            if (lbl == null) return;
            if (lbl.InvokeRequired)
                lbl.Invoke(new Action(() => { lbl.Text = text; lbl.ForeColor = color; }));
            else
            {
                lbl.Text = text;
                lbl.ForeColor = color;
            }
        }

        private void SafeUpdateCameraImage(OpenCvSharp.UserInterface.PictureBoxIpl pb, Mat mat)
        {
            if (pb == null || mat == null || mat.Empty()) return;
            if (pb.InvokeRequired)
                pb.Invoke(new Action(() => pb.ImageIpl = mat));
            else
                pb.ImageIpl = mat;
        }

        public string GetSerialNumber()
        {
            try
            {
                var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    var serial = obj["SerialNumber"]?.ToString();
                    return serial == "Default string" ? "Unknown" : serial;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            return "Unknown";
        }

        // ===== NÚT ĐỔI CAMERA =====
        private void btnConvertCamera_Click_1(object sender, EventArgs e)
        {
            SafeUpdateLabel(la_joutai, "🔄 Đang đổi camera, vui lòng chờ...", Color.Orange);

            new Thread(() =>
            {
                try
                {
                    // 🛑 Dừng camera hiện tại
                    Syuuryou = true;
                    Console.WriteLine("🛑 Đang dừng camera hiện tại...");

                    // ⏳ Chờ thread cũ kết thúc (tối đa 2s)
                    if (cameraThread != null && cameraThread.IsAlive)
                    {
                        int waited = 0;
                        while (cameraThread.IsAlive && waited < 2000)
                        {
                            Thread.Sleep(100);
                            waited += 100;
                        }
                    }

                    // 🚫 Giải phóng camera cũ
                    lock (cameraLock)
                    {
                        currentCapture?.Release();
                        currentCapture?.Dispose();
                        currentCapture = null;
                    }

                    Thread.Sleep(500); // đợi driver camera ổn định

                    // 🔁 Đổi camera ID
                    devices_camera_id = devices_camera_id == 0 ? 1 : 0;
                    ini_top["DEVICE", "devices_camera_id"] = devices_camera_id.ToString();
                    Console.WriteLine($"🔁 Đổi sang camera ID = {devices_camera_id}");

                    // 🔄 Khởi động lại camera
                    Syuuryou = false;
                    isRunning = false;

                    cameraThread = new Thread(() =>
                    {
                        Console.WriteLine("🎥 Khởi động camera mới...");
                        RokugaKaisi();
                    })
                    { IsBackground = true };
                    cameraThread.Start();

                    // ✅ Kiểm tra xem camera có thật sự trả frame không
                    int retry = 0;
                    while (retry < 20) // tối đa 2 giây
                    {
                        Thread.Sleep(100);
                        if (pb_cam1.ImageIpl != null)
                        {
                            SafeUpdateLabel(la_joutai, $"✅ Đã chuyển sang camera {devices_camera_id}", Color.Green);
                            return;
                        }
                        retry++;
                    }

                    // ❌ Nếu không thấy hình sau 2 giây
                    SafeUpdateLabel(la_joutai, "❌ Không thấy tín hiệu camera sau khi đổi", Color.Red);
                }
                catch (Exception ex)
                {
                    SafeUpdateLabel(la_joutai, $"❌ Lỗi khi đổi camera: {ex.Message}", Color.Red);
                    Console.WriteLine("❌ " + ex);
                }
            })
            { IsBackground = true }.Start();
        }


        // ===== HÀM HỖ TRỢ KHÁC =====
        public static string GetPrimaryEthernetMac()
        {
            try
            {
                var nics = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic => nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet &&
                                  nic.OperationalStatus == OperationalStatus.Up &&
                                  !nic.Description.ToLower().Contains("virtual") &&
                                  !nic.Description.ToLower().Contains("wifi") &&
                                  !nic.Description.ToLower().Contains("wireless") &&
                                  !nic.Description.ToLower().Contains("bluetooth") &&
                                  nic.SupportsMulticast &&
                                  !nic.IsReceiveOnly)
                    .OrderByDescending(nic => nic.Speed)
                    .FirstOrDefault();

                return nics != null
                    ? string.Join(":", nics.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")))
                    : "UNKNOWN_MAC";
            }
            catch (Exception ex)
            {
                return "ERROR_" + ex.Message;
            }
        }

        public int GetCameraIndex(string cameraName)
        {
            return int.TryParse(cameraName, out int id) ? id : 0;
        }

        public void RokugaSyuuryou()
        {
            Syuuryou = true;
            Thread.Sleep(1000);
            try { writer?.Dispose(); } catch { }
        }
    }
}

using DevExpress.Utils.Animation;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.SyntaxEditor;
using DevExpress.XtraTab;
using PdfiumViewer;
using PDFV5_forWin_for.Data;
using PDFV5_forWin_for.FormControl;
using PDFV5_forWin_for.Initializer;
using PDFV5_forWin_for.Models;
using PDFV5_forWin_for.UserControl;
using PDFV5_forWin_for.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Collections.Specialized.BitVector32;
using DateTime = System.DateTime;

namespace PDFV5_forWin_for
{
    public partial class XtraMain : DevExpress.XtraEditors.XtraForm
    {
        private PdfDocument _pdfDocument;   
        private int _currentPage = 0;        
        private StationName StationName = null;
        private bool isEnd = false;
        private int NowTarget = 0;
        private List<CTTK_PDF> lstCTTK = null;
        private CTTK_PDF CTTK_PDF = new CTTK_PDF(); 
        private static Timer timerClock;
        private bool isBypass = false;
        private bool isLayout = false;
        private bool isdbCheck = false;
        TimeplantInfo timeplantInfo = new TimeplantInfo();
        List<TimeplantInfo> lstTimeplant = new List<TimeplantInfo>();
        List<TemplateMachine> lstTemplateMachine = new List<TemplateMachine>();
        private System.Windows.Forms.Timer workTimer;
        private DateTime startTime;
        private IniFile iniROKUGA = new IniFile(@"C:\PasOpencv+\config.ini");
        private int Worktarget = 0;
        private string M_ipaddress = string.Empty;

        private void StartWorkTimer()
        {
            startTime = DateTime.Now;

            workTimer = new System.Windows.Forms.Timer();
            workTimer.Interval = 1000; // cập nhật mỗi giây
            workTimer.Tick += (s, e) =>
            {
                TimeSpan elapsed = DateTime.Now - startTime;
                la_time.Text = elapsed.ToString(@"hh\:mm\:ss"); 
            };
            workTimer.Start();
        }

        private void StopWorkTimer()
        {
            if (workTimer != null)
            {
                workTimer.Stop();
                workTimer.Dispose();
            }
        }

        private void SwitchTab(XtraTabPage nextPage)
        {
            // Hiệu ứng mờ mượt (Fade)
            transitionManager1.StartTransition(xtraTabControl1);
            try
            {
                xtraTabControl1.SelectedTabPage = nextPage;
                  

            }
            finally
            {

                transitionManager1.EndTransition();
                tbUnitID.Focus();
            }
        }


        public XtraMain()
        {
            InitializeComponent();
        }
        private void Main_Load(object sender, EventArgs e)
        {
            ConfigHelper.EnsureConfigExists();
            LoadUserControls();
            RoadMaster();
            lstCTTK = CTTK_PDF.GetPdfFiles();
            ValidateInputs();
            ActionEvent();
            InitClock();
            la_duplicateid.Visible = false;
            la_duplicateid_2.Visible = false;
            SwitchTab(tab_nyuuryoku);
            this.M_ipaddress = this.ip_get();

            
        }

        private void LoadUserControls()
        {
            panelCameraView.Controls.Clear();
            XtraUserCameraView xtraUserCameraView = new XtraUserCameraView();
            xtraUserCameraView.Dock = DockStyle.Fill;   
            panelCameraView.Controls.Add(xtraUserCameraView);

        }

        private void StartProgramIfNotRunning(string processName, string appPath)
        {
            try
            {
                KillProcess();
                bool isRunning = Process.GetProcessesByName(processName).Length > 0;
                if (!isRunning)
                {
                    if (File.Exists(appPath))
                    {
                        Process.Start(appPath);
                    }
                    else
                    {
                        MessageBox.Show($"Không tìm thấy file: {appPath}", "Lỗi",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi động chương trình:\n{ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void GenLogger(string key)
        {
            try
            {
                string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string unitPrefix = !string.IsNullOrEmpty(StationName.UnitID) && StationName.UnitID.Length >= 4
                    ? StationName.UnitID.Substring(0, 4)
                    : StationName.UnitID ?? "";

                bool isStart = key.Equals("S", StringComparison.OrdinalIgnoreCase);
                string typeText = isStart ? "START" : "END";
                string filePrefix = isStart ? "_AU_S_" : "_AU_E_";

                string logFile = $@"log\{filePrefix}{unitPrefix}_{StationName.Station}_{M_ipaddress}.csv";

                string content = $"\"{M_ipaddress}\",\"{StationName.UnitID}\",\"{StationName.Station}\",\"{StationName.WorkerID}\",#{now}#,\"{typeText}\"";

                Directory.CreateDirectory(Path.GetDirectoryName(logFile));

                using (StreamWriter sw = new StreamWriter(logFile, true, Encoding.UTF8))
                {
                    sw.WriteLine(content);
                }
            }
            catch (Exception ex)
            {
            }
        }


        private void HideAllSTLabels()
        {
            for (int i = 1; i <= 16; i++)
            {
                Control ctrl = this.Controls.Find($"tbST{i}", true).FirstOrDefault();
                if (ctrl != null)
                {
                    ctrl.Visible = false;
                }
            }
        }
        private void ShowSTLabels(List<int> stationList)
        {
            HideAllSTLabels();

            // Hiển thị theo danh sách station
            foreach (int st in stationList)
            {
                Control ctrl = this.Controls.Find($"tbST{st}", true).FirstOrDefault();
                if (ctrl != null)
                {
                    ctrl.Visible = true;
                }
            }
        }

        private void RoadMaster()
        {
            var config = ConfigHelper.ReadConfig();
            string unitID = config.ContainsKey("UnitID") ? config["UnitID"] : "";
            string workerID = config.ContainsKey("WorkerID") ? config["WorkerID"] : "";
            string station = config.ContainsKey("Station") ? config["Station"] : "";
            string proficiency = config.ContainsKey("Proficiency") ? config["Proficiency"] : "";
            string SL_Today = config.ContainsKey("Today") ? config["Today"] : "";            
            lblVersion.Text = "";
            iniROKUGA["DEVICE", "namevideo"] = "null";
            // Gán giá trị vào các TextBox/ComboBox tương ứng
            tbUnitID.Text = unitID;
            tbWorkerID.Text = workerID;
            cbStation.Text = station;
            cbProficiency.Text = proficiency;
            this.Text = $"PDF For Win 3.65";

            string todayString = DateTime.Now.ToString("yyyy-MM-dd");
            if(SL_Today != todayString)
            {
                ConfigHelper.UpdateConfigValue("Today", todayString);
                ConfigHelper.UpdateConfigValue("SL", "0");
            }
                
        }

        private void ActionEvent()
        {
            tbUnitID.TextChanged += (s, ev) => ValidateInputs();
            tbWorkerID.TextChanged += (s, ev) => ValidateInputs();
            cbStation.SelectedIndexChanged += (s, ev) => ValidateInputs();
            cbProficiency.SelectedIndexChanged += (s, ev) => ValidateInputs();
        }

        public static string NormalizeUnitID(string s)
        {
            return string.IsNullOrWhiteSpace(s)
                ? ""
                : s.Trim().Normalize(NormalizationForm.FormKD).ToLower();
        }
        private bool ValidateInputs()
        {
            // 🧹 Xóa lỗi cũ
            dxErrorProvider1.ClearErrors();

            // 🧾 Gán giá trị từ giao diện
            StationName = new StationName
            {
                UnitID = NormalizeUnitID(tbUnitID.Text?.Trim()),
                WorkerID = NormalizeUnitID(tbWorkerID.Text?.Trim().ToUpper()),
                Station = cbStation.Text?.Trim(),
                Proficiency = cbProficiency.Text?.Trim(),
                Digits = 10
            };

            // 🚧 1️⃣ Kiểm tra bypass
            if (StationName.UnitID.Equals("DUPLIOK", StringComparison.OrdinalIgnoreCase))
            {
                isBypass = true;
                lblStatus.Text = "⚙️ BYPASS chế độ kiểm tra (UnitID = DUPLIOK)";
                lblStatus.ForeColor = Color.OrangeRed;

                this.BeginInvoke(new Action(() =>
                {
                    // Reset form
                    tbUnitID.Text = "";

                    SwitchTab(tab_nyuuryoku);

                    // Tạo timer focus lại sau 200ms
                    var timer = new System.Windows.Forms.Timer { Interval = 200 };
                    timer.Tick += (s, e) =>
                    {
                        timer.Stop();
                        tbUnitID.Focus();
                        tbUnitID.Select();
                    };
                    timer.Start();
                })); 
                return true;
            }

            // 🚫 2️⃣ Kiểm tra tính hợp lệ thông thường
            if (!StationName.Validate(out string error))
            {
                lblStatus.Text = $"❌ {error}";
                lblStatus.ForeColor = Color.Red;

                // UnitID
                if (string.IsNullOrEmpty(StationName.UnitID))
                    dxErrorProvider1.SetError(tbUnitID, "UnitID không được để trống.");
                else if (StationName.UnitID.Length != StationName.Digits)
                    dxErrorProvider1.SetError(tbUnitID, $"UnitID phải có {StationName.Digits} ký tự.");
                else if (!StationName.UnitID.All(char.IsDigit))
                    dxErrorProvider1.SetError(tbUnitID, "UnitID chỉ được phép chứa số.");

                // WorkerID
                if (string.IsNullOrWhiteSpace(StationName.WorkerID))
                    dxErrorProvider1.SetError(tbWorkerID, "WorkerID không được để trống.");
                else if (StationName.WorkerID.Length < 7)
                    dxErrorProvider1.SetError(tbWorkerID, "WorkerID phải có đủ 7 ký tự.");

                // Station
                if (string.IsNullOrWhiteSpace(StationName.Station))
                    dxErrorProvider1.SetError(cbStation, "Station không được để trống.");

                // Proficiency
                if (string.IsNullOrWhiteSpace(StationName.Proficiency))
                    dxErrorProvider1.SetError(cbProficiency, "Proficiency không được để trống.");
                if(isBypass)
                    lblStatus.Text = "⚙️ BYPASS chế độ làm hàng lỗi";
                return false;
            }

            // ✅ 3️⃣ Hợp lệ → Lưu thông tin và xử lý tiếp
            lblStatus.Text = $"✅ Dữ liệu hợp lệ: {StationName.UnitID}";
            lblStatus.ForeColor = Color.Green;

            ConfigHelper.SaveMaster(
                StationName.UnitID,
                StationName.WorkerID.ToUpper(),
                StationName.Station,
                StationName.Proficiency
            );
            id_check();
            return true;
        }


        private void LoadPdf(string path)
        {
            _pdfDocument?.Dispose();
            _pdfDocument = PdfDocument.Load(path);
        }

        private void ShowPage(int pageIndex)
        {
            if (_pdfDocument == null) return;
            if (pageIndex < 0 || pageIndex >= _pdfDocument.PageCount) return;
            im_pdf.Image?.Dispose();
            im_pdf.Image = null;
            const int dpi = 300; 
            using (var bmp = _pdfDocument.Render(pageIndex, dpi, dpi, true))
            {
                im_pdf.Image = (Bitmap)bmp.Clone();
            }
            _currentPage = pageIndex;
            
        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            try
            {
                la_duplicateid_2.Visible = false;
                la_duplicateid.Visible = false ;
                isBypass = false;
                // Nếu tài liệu PDF chưa load hoặc không có trang
                if (_pdfDocument == null || _pdfDocument.PageCount == 0)
                {
                    MessageBox.Show("⚠️ Chưa có file PDF nào được mở.", "Thông báo",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                // Nếu chưa đến trang cuối → sang trang tiếp theo
                if (_currentPage + 1 < _pdfDocument.PageCount)
                {
                    _currentPage++;
                   
                    ShowPage(_currentPage);                   
                    UpdatePageStatus();
                    GetTemplateMachine(isdbCheck);
                    

                }
                else
                {
                    //if (isEnd)
                    //{
                    //    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    //    string fileName = $"E_{StationName.UnitID}_{StationName.Station}_{timestamp}.csv";
                    //    string uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csv_upload", "Upload_NG");
                    //    Directory.CreateDirectory(uploadDir);
                    //    string fullPath = Path.Combine(uploadDir, fileName);
                    //    File.Create(fullPath).Close();
                    //}
                    Optas.Upload(StationName.Station, StationName.UnitID, AppInitializer.GetMacAddress(), StationName.WorkerID, ConfigHelper.ReadValue("optas_forder_url"), "E", isEnd);
                    GenLogger("E");
                    if (isLayout)
                        UpdateLayout();
                    bool hasSixNines = Regex.IsMatch(StationName.UnitID, "999999");
                    var config = ConfigHelper.ReadConfig();
                    if (!hasSixNines)
                    {
                        Worktarget = int.Parse(config.ContainsKey("SL") ? config["SL"] : "") + 1;
                        ConfigHelper.UpdateConfigValue("SL", Worktarget.ToString());
                    }
                    ProcessFinished();
                    this.BeginInvoke(new Action(() =>
                    {
                       
                        SwitchTab(tab_nyuuryoku);
                        tbUnitID.Focus();
                        StationName = null;
                        _pdfDocument = null;
                        im_pdf.Image = null;
                    }));
                    HideAllSTLabels();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Lỗi khi chuyển trang: {ex.Message}",
                                "Lỗi",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
        }

        private void UpdateLayout()
        {
            isLayout = false;
            var layout = new OptasLayout
            {
                KariId = StationName.UnitID,
                Station = int.Parse(StationName.Station),
                WorkerId = StationName.WorkerID,
                CurrentPage = _currentPage+1,
                TotalPages = _pdfDocument.PageCount,
                TotalTime = ToSeconds(la_time.Text),
                IsActive = false
            };
            InsertOrUpdate(layout);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (_pdfDocument == null) return;
            if (_currentPage > 0)
            {
                _currentPage--;
                ShowPage(_currentPage);
                UpdatePageStatus(); 
            }
        }
        public void InitClock()
        {
            timerClock = new Timer();
            timerClock.Interval = 1000; 
            timerClock.Tick += TimerClock_Tick;
            timerClock.Start();
        }
        private void TimerClock_Tick(object sender, EventArgs e)
        {
            la_new_time.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            timerClock?.Stop();
            _pdfDocument?.Dispose();
        }


        public void id_check()
        {
            var dbCheck = DbConnectivityChecker.Check(2, true);
            isdbCheck= dbCheck.IsOk;
            var config = ConfigHelper.ReadConfig();
            int day_Target = int.Parse(config.ContainsKey("SL") ? config["SL"] : "");
            string unit_id = StationName.UnitID;
           
            string Worker_ID = StationName.WorkerID;
            int CurrentPage = int.TryParse(config.ContainsKey("CurrentPage") ? config["CurrentPage"] : null, out int val) ? val : 0;

            bool optas_exits = Optas.kariidCheck(unit_id, StationName.Station);
            

            if (optas_exits && CurrentPage ==0)
            {
                la_duplicateid.Visible = true;
                la_duplicateid_2.Visible = true;

                if (!isBypass)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        tbUnitID.Text = "";
                        lblStatus.Text = "UnitID Đã Tồn Tại Trên Hệ Thống";
                        lblStatus.ForeColor = Color.Red;

                        // Chuyển tab về nhập liệu
                        SwitchTab(tab_nyuuryoku);    

                        // Tạo timer để focus lại sau 200ms (đợi UI render)
                        var timer = new System.Windows.Forms.Timer();
                        timer.Interval = 200;
                        timer.Tick += (s2, e2) =>
                        {
                            timer.Stop();
                            tbUnitID.Focus();
                            tbUnitID.Select();
                        };
                        timer.Start();
                    }));
                    return;
                }

                
            }
            string prefix = unit_id.Length >= 4 ? unit_id.Substring(0, 4) : unit_id;
            string Station = StationName.Station;
            string Proficiency = StationName.Proficiency;
            var CTTK= lstCTTK.Where(x => x.Kari == prefix && x.Station.ToString() == Station 
                     && x.Proficiency.ToUpper() == Proficiency.ToUpper()).FirstOrDefault();
            
            lstTimeplant = timeplantInfo.GetTimeplantByKari(int.Parse(prefix),dbCheck.IsOk);

            // 1️⃣ Lấy tất cả Station trong danh sách Timeplant
            var lstST = lstTimeplant.Select(x => x.St).Distinct().ToList();

            NowTarget = lstTimeplant
            .Where(x => x.St == int.Parse(Station))
            .Select(x => x.ProductionQtyEdit != 0 ? x.ProductionQtyEdit : x.ProductionQty)
            .FirstOrDefault();
            if (CTTK != null)
            {
                string unitname = lstTimeplant.Where(x => x.St == int.Parse(Station)).Select(x => x.UnitName).FirstOrDefault().ToString();
                lblVersion.Text = $"PDF Version : {CTTK.Version}";
                lblUnitName.Text = $"Unit Name : {unitname}";
                iniROKUGA["DEVICE", "namevideo"] = $"{unitname}_{unit_id}_{Station}".ToUpper();
                ShowSTLabels(stationList: lstST);
                StartWorkTimer();
                LoadPdf(CTTK.FileLink);
                if (!isBypass )
                {
                    ShowPage(CurrentPage > 0 ? CurrentPage-1 : 0);
                }
                else
                {
                    var optas_layout = OptasLayout.Search(unit_id, int.Parse(Station), true).FirstOrDefault();
                    if (optas_layout != null)
                    {
                        CurrentPage = optas_layout != null ? optas_layout.CurrentPage : 0;
                        ShowPage(CurrentPage);
                        isLayout = true;
                    }
                }
                   
                isEnd = CTTK.isLast.IndexOf("E") >= 0;
                la_workerID.Text = Worker_ID;
                la_proficiency.Text = Proficiency;
                la_station.Text = Station;
                la_utnitID.Text = unit_id;
                int now_target_display = day_Target + 1; 
                la_now_target.Text = now_target_display.ToString();
                la_difference.Text = (NowTarget - day_Target).ToString();
                la_day_target.Text = $"{day_Target}/{NowTarget}";
                UpdatePageStatus();
                if (Station == "1")
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                    string fileName = $"S_{unit_id}_{Station}_{timestamp}.csv";
                    string uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "csv_upload", "Upload_NG");
                    Directory.CreateDirectory(uploadDir);
                    string fullPath = Path.Combine(uploadDir, fileName);
                    File.Create(fullPath).Close();

                }
                if(!optas_exits)
                {
                    Optas.Upload(Station, unit_id, AppInitializer.GetMacAddress(), Worker_ID, ConfigHelper.ReadValue("optas_forder_url"), "S", false);
                    GenLogger("S");
                }
                this.BeginInvoke(new Action(() =>
                {
                    xtraTabControl1.SelectedTabPage = tab_pdf_hyouji;
                    SwitchTab(tab_pdf_hyouji);
                }));

                GetTemplateMachine(dbCheck.IsOk);
            }
            else
            {
                tbUnitID.Text = "";
                lblStatus.Text = "CTTK Không Có Trên Hệ Thống";
                lblStatus.ForeColor = Color.Red;

                // 🕒 Tạo Timer delay 0.5 giây cho an toàn (đợi UI render xong)
                var timer = new System.Windows.Forms.Timer();
                timer.Interval = 200; // nửa giây
                timer.Tick += (s, e) =>
                {
                    timer.Stop();

                    // 🖱️ "Nhả chuột" - bỏ focus khỏi control hiện tại
                    this.ActiveControl = null;

                    // ⌨️ Focus lại vào ô nhập Unit ID
                    tbUnitID.Focus();
                    tbUnitID.Select();
                };
                timer.Start();

            }

        }


        public void GetTemplateMachine(bool dbCheck)
        {
            try
            {
                if (lstTemplateMachine == null)
                    lstTemplateMachine = new List<TemplateMachine>();

                if (lstTemplateMachine.Count == 0)
                {
                    string unitId = StationName.UnitID ?? "";
                    string station = StationName.Station ?? "";
                    string unitPrefix = unitId.Length >= 4 ? unitId.Substring(0, 4) : unitId;
                    lstTemplateMachine = TemplateMachine.GetByMachineAndStation(unitPrefix, station, dbCheck) ?? new List<TemplateMachine>();
                    if (lstTemplateMachine.Count == 0) return;
                }

                var template = lstTemplateMachine.FirstOrDefault(x => x.Page == _currentPage);
                if (template != null) ApplyTemplateToUI(template);
            }
            catch (Exception ex)
            {
                LogError("GetTemplateMachine", ex);
                MessageBox.Show("Lỗi khi lấy TemplateMachine: " + ex.Message,
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ApplyTemplateToUI(TemplateMachine template)
        {
            Xtra_TemplateMarchine xtra_Template = new Xtra_TemplateMarchine(template.ValueDefault);  
            xtra_Template.ShowDialog();

        }

        private void LogError(string method, Exception ex)
        {
            try
            {
                string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log");
                Directory.CreateDirectory(logDir);
                string logPath = Path.Combine(logDir, "runtime_error.txt");
                string message = string.Format("[{0}] [{1}] {2}\n{3}\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), method, ex.Message, ex.StackTrace);
                File.AppendAllText(logPath, message);
            }
            catch { }
        }


        private void UpdatePageStatus()
        {
            try
            {
                // 🧭 Hiển thị trang hiện tại / tổng số trang
                int currentPage = _currentPage; // hoặc M_page nếu bạn dùng biến khác
                int totalPages = _pdfDocument != null ? _pdfDocument.PageCount : 0;

                // Cập nhật label số trang
                this.la_page.Text = $"{currentPage + 1}/{totalPages}";
                
                ConfigHelper.UpdateConfigValue("UnitID", StationName.UnitID);
                int currentNow = _currentPage+1;
                ConfigHelper.UpdateConfigValue("CurrentPage", (currentNow).ToString());

                // 📊 Tính phần trăm (nếu tổng > 0)
                if (totalPages > 0)
                {
                    double pasento2 = (double)(currentPage + 1) / totalPages * 100;
                    la_pasento.Text = $"{pasento2:F0}%"; // hoặc ((int)pasento2).ToString() + "%"
                }
                else
                {
                    la_pasento.Text = "0%";
                }
            }
            catch (Exception ex)
            {
                la_page.Text = "0/0";
                la_pasento.Text = "0%";
                Console.WriteLine($"❌ Lỗi khi cập nhật trạng thái trang: {ex.Message}");
            }
        }

        private void btnLayout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                   "Bạn có muốn Layout không?",           // Nội dung
                   "Xác nhận Layout",                     // Tiêu đề
                   MessageBoxButtons.YesNo,              // Nút Yes / No
                   MessageBoxIcon.Error               // Biểu tượng dấu hỏi
               );

            if (result == DialogResult.Yes)
            {
                var layout = new OptasLayout
                {
                    KariId = StationName.UnitID,
                    Station = int.Parse(StationName.Station),
                    WorkerId = StationName.WorkerID,
                    CurrentPage = _currentPage,
                    TotalPages = _pdfDocument.PageCount,
                    TotalTime = ToSeconds(la_time.Text),
                    IsActive = true
                };
                InsertOrUpdate(layout);
                ProcessFinished();
            }
        }

        // ========================
        // 🔄 InsertOrUpdate (Truyền đối tượng)
        // ========================
        public static void InsertOrUpdate(OptasLayout data)
        {
            try
            {
                if (data == null)
                    throw new ArgumentNullException(nameof(data), "Đối tượng truyền vào bị null!");

                // 🔍 Dùng lại Search() để kiểm tra tồn tại (theo KariId + Station)
                var existing = OptasLayout.Search(data.KariId, data.Station, null);

                if (existing != null && existing.Count > 0)
                {
                    // Nếu tồn tại → Update
                    data.Id = existing[0].Id;
                    data.Update();
                }
                else
                {
                    // Nếu chưa có → Insert
                    data.Insert();
                }
            }
            catch (Exception ex)
            {
            }
        }



        public static int ToSeconds(string time)
        {
            if (TimeSpan.TryParse(time, out TimeSpan ts))
                return (int)ts.TotalSeconds;
            return 0;
        }

        private void PDFV5_forWin_for_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // 🔍 Tìm tất cả tiến trình tên ROKUGA1.0
                Process[] procs = Process.GetProcessesByName("ROKUGA1.0");
                foreach (Process proc in procs)
                {
                    try
                    {
                        proc.Kill();              // 🔪 Đóng tiến trình
                        proc.WaitForExit(1000);   // ⏳ Chờ 1 giây cho chắc
                    }
                    catch { /* bỏ qua tiến trình đã thoát */ }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đóng ROKUGA1.0: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ProcessFinished()
        {
            lblVersion.Text = $"Sản Lượng : {Worktarget}/{NowTarget}";
            lblUnitName.Text = $"{la_utnitID.Text} Hoàn Thành Công Đoạn";
            StopWorkTimer();
            tbUnitID.Text = "";
            iniROKUGA["DEVICE", "namevideo"] = "null";
            isEnd = false;
            ConfigHelper.UpdateConfigValue("UnitID", "");
            ConfigHelper.UpdateConfigValue("CurrentPage", "0");
            lstTemplateMachine = null;
            this.BeginInvoke(new Action(() =>
            {
                xtraTabControl1.SelectedTabPage = tab_nyuuryoku;
                tbUnitID.Focus();
            }));

        }


        public string ip_get()
        {
            foreach (IPAddress hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
                    return hostAddress.ToString();
            }
            return "No IP Address";
        }
        private void KillProcess()
        {
            try
            {
                // 🔍 Tìm tất cả tiến trình tên ROKUGA1.0
                Process[] procs = Process.GetProcessesByName("ROKUGA1.0");
                foreach (Process proc in procs)
                {
                    try
                    {
                        proc.Kill();              // 🔪 Đóng tiến trình
                        proc.WaitForExit(1000);   // ⏳ Chờ 1 giây cho chắc
                    }
                    catch { /* bỏ qua tiến trình đã thoát */ }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi đóng ROKUGA1.0: {ex.Message}",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void tbWorkerID_Properties_Leave(object sender, EventArgs e)
        {
            tbUnitID.Text = tbUnitID.Text.ToUpper();
        }
    }


    
}

using OpenCvSharp;
using System;

namespace PDFV5_forWin_for.Initializer
{
    public static class CameraHelper
    {
        /// <summary>
        /// Giải phóng camera hiện tại nếu đang mở.
        /// </summary>
        /// <param name="cap">Đối tượng VideoCapture cần giải phóng</param>
        public static void CloseCamera(int camId)
        {
            try
            {
                var cap = new VideoCapture(camId);
                if (cap.IsOpened())
                {
                    cap.Release();          // 🧹 Giải phóng khỏi driver
                    Cv2.WaitKey(100);       // ⏳ Cho hệ thống thời gian đóng thiết bị
                    Console.WriteLine($"✅ Camera ID {camId} đã được đóng.");
                }
                else
                {
                    Console.WriteLine($"⚠️ Camera ID {camId} chưa mở hoặc đã bị driver khóa.");
                }
                cap.Dispose();              // 🗑️ Thu hồi tài nguyên
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi đóng camera ID {camId}: {ex.Message}");
            }
        }


    }
}

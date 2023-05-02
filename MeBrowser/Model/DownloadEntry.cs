using CefSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace MeBrowser.Model
{
    public class DownloadEntry : BindableBase
    { 
        /// <summary>
        /// Id của tab đang download, khi download kết thúc sẽ được gán -1.
        /// </summary>
        public int TabId { get; set; }
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get => GetProperty<DateTime?>(); set => SetProperty(value); }
        public string OriginalUrl { get; set; }
        public string Url { get; set; }
        public string FullPath { get => GetProperty<string>(); set => SetProperty(value); }
        public string ShortPath { get => GetProperty<string>(); set => SetProperty(value); }
        public double ReceivedData { get => GetProperty<double>(); set => SetProperty(value); }
        public double TotalData { get => GetProperty<double>(); set => SetProperty(value); }
        public string DataUnit { get => GetProperty<string>(); set => SetProperty(value); }
        public int PercentComplete { get => GetProperty<int>(); set => SetProperty(value); }
        public double CurrentSpeed { get => GetProperty<double>(); set => SetProperty(value); }
        public string SpeedUnit { get => GetProperty<string>(); set => SetProperty(value); }
        public bool IsPaused { get => GetProperty<bool>(); set => SetProperty(value); }
        public bool IsCancelled { get => GetProperty<bool>(); set => SetProperty(value); }
        public bool IsComplete { get => GetProperty<bool>(); set => SetProperty(value); }
        public bool IsInProgress { get => GetProperty<bool>(); set => SetProperty(value); }
        /// <summary>
        /// 1 giá trị có thể thay đổi mỗi lần update để hỗ trợ tạo hiệu ứng trên UI.
        /// </summary>
        public int InProgressState { get => GetProperty<int>(); set => SetProperty(value); }
        [XmlIgnore]
        public IDownloadItemCallback? Callback { get; set; }
        public static bool IsDownloadInProgress { get; set; }
        public void Update(DownloadArgs downloadArgs)
        {
            IsDownloadInProgress = downloadArgs.Item.IsInProgress;

            Callback = downloadArgs.Callback;

            IsCancelled = downloadArgs.Item.IsCancelled;
            IsComplete = downloadArgs.Item.IsComplete;
            if (IsComplete || IsCancelled)
            {
                Callback = null;
                TabId = -1;
                CommandManager.InvalidateRequerySuggested();
            }
            FullPath = downloadArgs.Item.FullPath;
            EndTime = downloadArgs.Item.EndTime;
            PercentComplete = downloadArgs.Item.PercentComplete;

            if (IsPaused) IsInProgress = false;
            else IsInProgress = downloadArgs.Item.IsInProgress;

            if (IsInProgress)
            {
                if (InProgressState == 0) InProgressState = 1;
                else InProgressState = 0;
            }

            ShortPath = downloadArgs.Item.FullPath.Split(@"\").LastOrDefault(string.Empty);

            string totalUnit = "B";
            double totalFactor = 1;
            string speedUnit = "B";
            double speedFactor = 1;
            long total = downloadArgs.Item.TotalBytes;
            long received = downloadArgs.Item.ReceivedBytes;
            long speed = downloadArgs.Item.CurrentSpeed;

            if (total < 1024)
            {
                totalFactor = 1;
                totalUnit = "B";
            }
            else if (total < 1_048_576)
            {
                totalFactor = 1024;
                totalUnit = "KB";
            }
            else if (total < 1_073_741_824)
            {
                totalFactor = 1_048_576;
                totalUnit = "MB";
            }
            else if (total < Math.Pow(2, 40))
            {
                totalFactor = 1_073_741_824;
                totalUnit = "GB";
            }
            else if (total > Math.Pow(2, 40))
            {
                totalFactor = Math.Pow(2, 40);
                totalUnit = "TB";
            }

            if (speed < 1024)
            {
                speedFactor = 1;
                speedUnit = "B";
            }
            else if (speed < 1_048_576)
            {
                speedFactor = 1024;
                speedUnit = "KB";
            }
            else if (speed < 1_073_741_824)
            {
                speedFactor = 1_048_576;
                speedUnit = "MB";
            }
            else if (speed < Math.Pow(2, 40))
            {
                speedFactor = 1_073_741_824;
                speedUnit = "GB";
            }
            else if (speed > Math.Pow(2, 40))
            {
                speedFactor = Math.Pow(2, 40);
                speedUnit = "TB";
            }

            ReceivedData = Math.Round(received / totalFactor, 1);
            TotalData = Math.Round(total / totalFactor, 1);
            DataUnit = totalUnit;

            CurrentSpeed = Math.Round(speed / speedFactor, 1);
            SpeedUnit = speedUnit;
        }
        public void Cancel()
        {
            if (Callback != null && Callback.IsDisposed == false)
            {
                IsPaused = false;
                Callback.Cancel();
                Callback = null;
                //TabId = -1;
            }
        }
        /// <summary>
        /// Khi close tab hoặc close app, chromium sẽ bị dispose, downloadhandler không thể gửi các event cuối cùng của nó nên ta sẽ tự cập nhật thủ công cho nó.
        /// </summary>
        public void CancelBeforeClose()
        {
            if (Callback != null && Callback.IsDisposed == false)
            {
                IsPaused = false;
                IsInProgress = false;
                IsCancelled = true;
                ShortPath = string.Empty;
                ReceivedData = 0;
                CurrentSpeed = 0;
                Callback.Cancel(); //Cancel để nó xoá file đang tải dở
                Callback.Dispose();
                Callback = null;
                Id = -1;
                TabId = -1;
            }
        }
        public void Pause()
        {
            if (IsPaused == false && Callback != null && Callback.IsDisposed == false)
            {
                IsPaused = true;
                Callback.Pause();
            }
        }
        public void Resume()
        {
            if (IsPaused == true && Callback != null && Callback.IsDisposed == false)
            {
                IsPaused = false;
                Callback.Resume();
            }
        }
    }
}

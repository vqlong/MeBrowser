using CefSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MeBrowser.Model
{
    /// <summary>
    /// Quản lý dữ liệu history và download.
    /// </summary>
    public class BrowsingData
    {
        public static ObservableCollection<HistoryEntry> History { get; set; } //= new ObservableCollection<HistoryEntry>();
        public static ObservableCollection<DownloadEntry> Downloads { get; set; } //= new ObservableCollection<DownloadEntry>();        
        public static void AddHistoryEntry(string address)
        {
            //Do các trang của google bị load liên tiếp 2 lần
            if (History.Count > 0 && History[^1].Url == address) return;

            if (!string.IsNullOrWhiteSpace(address)) History.Add(new HistoryEntry { Time = DateTime.Now, Url = address });
        }
        public static DownloadEntry AddDownloadEntry(DownloadItem downloadItem, int tabId)
        {
            var entry = new DownloadEntry
            {
                TabId = tabId,
                Id = downloadItem.Id,
                StartTime = downloadItem.StartTime,
                OriginalUrl = downloadItem.OriginalUrl,
                Url = downloadItem.Url, 
            }; 
            Downloads.Add(entry);

            return entry;
        }
        public static DownloadEntry? GetDownloadEntry(int tabId, int id, DateTime? startTime)
        {
            return Downloads.FirstOrDefault(e => e.TabId == tabId && e.Id == id && e.StartTime == startTime);
        }
        public static void CancelAllDownloadsBeforeClose()
        {
            foreach (var download in Downloads) { download.CancelBeforeClose(); }
        }
    }
}

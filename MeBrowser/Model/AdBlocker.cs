using CefSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MeBrowser.Model
{
    public static class AdBlocker
    {
        public static List<string> Ads { get; set; } = new List<string>();
        public static Dictionary<string, string> Scripts { get; set; } = new Dictionary<string, string>();
        public static string LastCheckAd { get; set; } = string.Empty;
        public static string LastCheckSafeUrl { get; set; } = string.Empty; 
        public static string YoutubeScript { get; } = 
            @"(function()
            {
                const defined = v => v !== null && v !== undefined;
                const ad = [...document.querySelectorAll('.ad-showing')][0];
                if (defined(ad))
                {
                    const video = document.querySelector('video');
                    if (defined(video)) { video.currentTime = video.duration; }
                }
            })();";
        public static string YoutubeScript1 { get; } =
            @"(function()
            {
                const answer = prompt('type something', 'something');

                if(answer != null)
                {
                    alert(answer);
                }
                else
                {
                    alert('cancel');
                }

                if(confirm('Do you want google right here?') == true)
                {
                    window.open('www.google.com', '_blank');
                }
                else
                {
                    alert('You dont want google.');
                }
            })();";
        public static void Load()
        {
            if (File.Exists(Settings.ADS_FILEPATH))
            {
                string[] ads = File.ReadAllText(Settings.ADS_FILEPATH).Split(Environment.NewLine);
                Ads.Clear();
                Ads.AddRange(ads); 
            }

            //Scripts["voz.vn"] = @"(function()
            //{
            //    document.querySelectorAll('.adsbypubpower').forEach(ads => ads.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('div[class=""message message--post""]').forEach(rec => rec.style.setProperty('display', 'none', 'important'));
            //})();";

            //Scripts["youtube.com"] = @"(function()
            //{
            //    document.querySelectorAll('#player-ads').forEach(ads => ads.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('#panels').forEach(ads => ads.style.setProperty('display', 'none', 'important'));

            //    const defined = v => v !== null && v !== undefined;
            //    const timeout = setInterval(() => {
            //    const ad = [...document.querySelectorAll('.ad-showing')][0];
            //        if (defined(ad)) 
            //        {
            //            const video = document.querySelector('video');
            //            if (defined(video)) { video.currentTime = video.duration; }
            //        }
            //    }, 500);
            //    return function() { clearTimeout(timeout); }
            //})();";

            //Scripts["thanhnien.vn"] = @"(function()
            //{
            //    document.querySelector('.super-masthead').style.display = 'none'; 
            //    document.querySelector('.section__topbanner').style.display = 'none'; 
            //    document.querySelectorAll('.box-home-2').forEach(ad => ad.style.setProperty('display', 'none', 'important'));

            //    document.querySelectorAll('.section__qadd').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('.section__nsp-sub').forEach(ad => ad.style.setProperty('display', 'none', 'important')); 
            //    document.querySelectorAll('.d-block.mt-32.mb-32').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('.section__stream-sub-qadd').forEach(ad => ad.style.setProperty('display', 'none', 'important'));

            //    document.querySelectorAll('.list__focus-sub').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('.list__stream-sub').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('.detail__cmain-sub').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('#zone-lbosc0aj').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('#zone-kxgtakp8').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //    document.querySelectorAll('.container.mt-20').forEach(ad => ad.style.setProperty('display', 'none', 'important'));
            //})();";

            if (File.Exists(Settings.SCRIPTS_FILEPATH))
            {
                using var stream = new FileStream(Settings.SCRIPTS_FILEPATH, FileMode.Open, FileAccess.Read);
                var serializer = new XmlSerializer(typeof(List<KeyValueWrapper>));
                var obj = serializer.Deserialize(stream);
                if(obj is List<KeyValueWrapper> wrappers)
                {
                    Scripts.Clear();
                    wrappers.ForEach(kvw => Scripts[(string)kvw.Key] = (string)kvw.Value);
                }

            }
 
        }

        public static void BlockByPage(string url, IWebBrowser chromiumWebBrowser)
        {
            foreach (var key in Scripts.Keys)
            {
                if (url.Contains(key))
                {
                    chromiumWebBrowser.ExecuteScriptAsync(Scripts[key]);
                    return;
                }
            }
        }
        public static bool IsAd(this string url)
        {
            if (!string.IsNullOrWhiteSpace(LastCheckAd) && url.Contains(LastCheckAd)) return true;
            if (!string.IsNullOrWhiteSpace(LastCheckSafeUrl) && url.Contains(LastCheckSafeUrl)) return false;

            if (Ads.Count > 0) 
            {
                foreach (var ad in Ads)
                {
                    if (url.Contains(ad))
                    {
                        LastCheckAd = ad; 
                        return true;
                    }
                }
            }

            LastCheckSafeUrl = url; 
            return false;
        }
    }
}

using CefSharp;
using CefSharp.Wpf;
using MeBrowser.Helpers;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;

namespace MeBrowser.View
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            cbSourceLanguage.ItemsSource = CultureInfo.GetCultures(CultureTypes.AllCultures);
            cbTargetLanguage.ItemsSource = CultureInfo.GetCultures(CultureTypes.AllCultures);


        }

        private void Translate_Click(object sender, RoutedEventArgs e)
        {
            Translate(txbSourceLanguage.Text);
            //chromium.ExecuteScriptAsync($"document.getElementById('tta_srcsl').value='{cbSourceLanguage.SelectedValue}';"); 
            //chromium.ExecuteScriptAsync($"document.getElementById('tta_input_ta').value='{txbSourceLanguage.Text}';");
            //chromium.ExecuteScriptAsync($"document.getElementById('tta_input_ta').click();");
            ////chromium.ExecuteScriptAsync($"document.querySelector('div[id=\"tta_playiconsrc\"]').click();");
            //chromium.ExecuteScriptAsync($"document.getElementById('tta_playiconsrc').click()");
        }
        public async void Translate(string word)
        {
            var fromLanguage = cbSourceLanguage.SelectedValue;
            var toLanguage = cbTargetLanguage.SelectedValue;
            var url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={fromLanguage}&tl={toLanguage}&dt=t&q={/*HttpUtility.UrlEncode(word)*//*Uri.EscapeUriString(word)*/ Uri.EscapeDataString(word)}";
            //var webClient = new WebClient
            //{
            //    Encoding = System.Text.Encoding.UTF8
            //};
            //var result = webClient.DownloadString(url);
            HttpClient httpClient = new HttpClient();
            var result = await httpClient.GetStringAsync(url);
            try
            {
                result = result.Substring(4, result.IndexOf("\"", 4, StringComparison.Ordinal) - 4);
                txbTargetlanguage.Text = result;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
 
        }
        private void GetVoicesAsync_Click(object sender, RoutedEventArgs e)
        {
            GetVoicesAsync(); 
        }
        public async void GetVoicesAsync()
        {
            string script = @"(function()
            {
   	            const voices = window.speechSynthesis.getVoices(); 
                const arrayVoices = [];
                for (const voice of voices) 
                { 
                    arrayVoices.push(voice.name + '@' + voice.lang);
                }
                return arrayVoices;
            })();";

            var response = await chromium.EvaluateScriptAsync(script);

            if (response.Success)
            {
                var voices = (IList<object>)response.Result;
                if(voices.Count == 0)
                {
                    GetVoicesAsync();
                }
                else
                {
                    List<SpeechSynthesisVoice> speechSynthesisVoices = new List<SpeechSynthesisVoice>();
                    foreach (var voice in voices)
                    {
                        string[] array = voice.ToString().Split("@");
                        speechSynthesisVoices.Add(new SpeechSynthesisVoice { Name = array[0], Language = array[1] });
                    }
                    cbVoices.ItemsSource = speechSynthesisVoices;
                }
            }
        }

        private void SpeakAsync_Click(object sender, RoutedEventArgs e)
        {
            Speak();
        }

        private void Speak()
        {
            string script = @"(function(text, voiceName)
            {
                const utterThis = new SpeechSynthesisUtterance(text);
                const voices = window.speechSynthesis.getVoices();
  	            for (let i = 0; i < voices.length; i++) 
                {
                    if (voices[i].name === voiceName) 
    	            {
      		            utterThis.voice = voices[i];
    	            }
  	            }
                window.speechSynthesis.speak(utterThis);
            })('" + txbSourceLanguage.Text + "','" + (string)cbVoices.SelectedValue + "'); ";

            chromium.ExecuteScriptAsync(script);
        }
    }
}

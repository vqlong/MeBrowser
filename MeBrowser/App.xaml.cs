using CefSharp;
using CefSharp.Wpf;
using MeBrowser.Model;
using MeBrowser.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MeBrowser
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CefSettings settings = new CefSettings() { RemoteDebuggingPort = Settings.REMOTEDEBUGGER_PORT };
            //debug.log  ...Use the command line flag --remote-allow-origins=http://localhost:8080 to allow connections from this origin or --remote-allow-origins=* to allow all origins. 
            settings.CefCommandLineArgs.Add("remote-allow-origins", "*");
            //
            //settings.CefCommandLineArgs.Add("autoplay-policy", "no-user-gesture-required");
            Cef.Initialize(settings);

            base.OnStartup(e);
        }
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            DialogBox.Show(e.Exception.Message + "\n" + (e.Exception.InnerException != null ? e.Exception.InnerException.Message : ""), "Unhandled exception");
            e.Handled = true;
        }
    }
}

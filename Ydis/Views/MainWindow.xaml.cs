using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Ydis.Model;
using Ydis.Model.Recording;
using Ydis.Model.UserSettings;
using Ydis.Model.Utilities;
using Ydis.Properties;
using Ydis.ViewModels;

namespace Ydis.Views
{
    /// <summary>
    /// Code-behind for main window
    /// </summary>
    public partial class MainWindow : Window
    {
        const string MINIMIZED_ARG = "--minimized";
        const string MUTEX_NAME = "GDYdisMutex";

        private Recorder _recorder;
        private static Mutex _mutex;
        public MainWindow()
        {
            Application.Current.DispatcherUnhandledException += ExceptionApp;
            AppDomain.CurrentDomain.UnhandledException += ExceptionDomain;
            _mutex = new Mutex(true, MUTEX_NAME, out var createdNew);
            if (createdNew)
            {
                Closing += FreeMutex;
                Init();
            }
            else
            {
                Application.Current.Shutdown();
            }
        }

        private void FreeMutex(object sender, CancelEventArgs e)
        {
            _mutex?.Close();
        }

        private void StopApp(object sender, EventArgs e)
        {
            _recorder?.StopRecording();
        }

        private void Init()
        {
            _recorder = new Recorder();
            _recorder.StartRecording();

            InitAppData();
            WritePathFile();
            DataContext = new MainWindowViewModel(_recorder, GetMinimizedArg());
            InitializeComponent();

            Closing += StopApp;
        }

        private void InitAppData()
        {
            if (!Directory.Exists(YDISSettings.AppData))
            {
                Directory.CreateDirectory(YDISSettings.AppData);
            }
        }

        private void WritePathFile()
        {
            try
            {
                File.WriteAllText(YDISSettings.ExeLocationPath, Assembly.GetEntryAssembly().Location);
            }
            catch { }
        }

        private bool GetMinimizedArg()
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg == MINIMIZED_ARG)
                {
                    return true;
                }
            }
            return false;
        }

        private void ExceptionApp(object sender, DispatcherUnhandledExceptionEventArgs e)
        { 
            e.Handled = true;
            CrashAndLog(e.Exception);
        }

        private void ExceptionDomain(object sender, UnhandledExceptionEventArgs e)
        {
            CrashAndLog((Exception)e.ExceptionObject);
        }

        private void CrashAndLog(Exception e)
        {
            try
            {
                Closing -= StopApp;
                ExceptionLogger.Log(e);
                _recorder?.CrashRecorder();
                string msg = string.Format("{0}\n{1}", Properties.Resources.ErrorMessage,
                                                       string.Format(Properties.Resources.ErrorMessageLogLocationFormat, YDISSettings.LogsPath));
                //Thread needed because sometimes the message box is not displayed                
                MessageBox.Show(Application.Current.MainWindow, msg, Properties.Resources.ErrorMessageTitle);
            }
            catch { } // Probably prevents looping if an error occurs while logging the error
            Application.Current.Shutdown();
        }
    }
}

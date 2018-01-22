using FZ185FtpDecl.Controllers;
using System;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;

namespace FZ185FtpAsync.WinService
{
    public partial class Service1 : ServiceBase
    {
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer1.Interval = 30000;
            try
            {
                timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["interval"]);
                timer1.Start();
            }
            catch { }

            Logger.EventLog("Start Service");
        }

        protected override void OnStop()
        {
            timer1.Stop();
            Logger.EventLog("Stop Service");
        }

        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            timer1.Enabled = false;

            try
            {
                string _ftpHost = ConfigurationManager.AppSettings["host"];
                string _ftpUsername = ConfigurationManager.AppSettings["username"];
                string _ftpPasswd = ConfigurationManager.AppSettings["password"];
                string _ftpPathForImport = ConfigurationManager.AppSettings["ftpPathForImport"];
                string _ftpPathForExport = ConfigurationManager.AppSettings["ftpPathForExport"];
                string _localPathForImport = ConfigurationManager.AppSettings["localPathForImport"];
                string _localPathForExport = ConfigurationManager.AppSettings["localPathForExport"];

                var ftpController = new FtpMessageController(_ftpHost, _ftpUsername, _ftpPasswd);

                Download(ftpController, _localPathForImport, _ftpPathForImport);

                Download(ftpController, _localPathForExport, _ftpPathForExport);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog("Timer1 " + ex.Message);
            }
            timer1.Enabled = true;
        }

        private void Download(FtpMessageController ftpController, string localFolder, string ftpFolder)
        {
            var files = ftpController.ListFiles(ftpFolder);

            for (int i = 0; i < files.Count(); i++)
            {
                string filename = files[i];
                string destinationFileName = System.IO.Path.Combine(localFolder, filename);

                bool downstate = ftpController.DownloadFile(filename, ftpFolder, destinationFileName);

                if (downstate)
                {
                    var deleCode = ftpController.Delete(filename, ftpFolder);

                    Logger.EventLog(string.Format("{0} ==> {1}", filename, deleCode));
                }
            }
        }
    }
}
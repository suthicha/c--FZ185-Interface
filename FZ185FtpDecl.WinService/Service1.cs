using FZ185FtpDecl.Controllers;
using System;
using System.Configuration;
using System.ServiceProcess;

namespace FZ185FtpDecl.WinService
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

                Upload(ftpController, _localPathForImport, _ftpPathForImport);
                Upload(ftpController, _localPathForExport, _ftpPathForExport);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog("Timer1 " + ex.Message);
            }
            timer1.Enabled = true;
        }

        private void Upload(FtpMessageController ftpController, string sourceFolder, string destFolder)
        {
            var msgController = new MessageInfoController();

            // Read import local folder.
            var lstImportZips = msgController.GetFiles(sourceFolder);
            for (int i = 0; i < lstImportZips.Count; i++)
            {
                var fileInfo = lstImportZips[i];
                var upstatus = ftpController.UploadFtpFile(destFolder, fileInfo.FullName);

                if (upstatus)
                {
                    msgController.Delete(fileInfo.FullName);
                    Logger.EventLog("UPLOAD " + fileInfo.FullName + " ==> OK");
                }
                else
                {
                    Logger.EventLog("UPLOAD " + fileInfo.FullName + " ==> FAIL");
                }
            }
        }
    }
}
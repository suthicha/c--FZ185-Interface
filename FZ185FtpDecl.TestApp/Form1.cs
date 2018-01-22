using FZ185FtpDecl.Controllers;
using FZ185FtpDecl.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FZ185FtpDecl.TestApp
{
    public partial class Form1 : Form
    {
        private FtpMessageController _ftpController;
        private string _ftpHost = "";
        private string _ftpUsername = "";
        private string _ftpPasswd = "";
        private string _ftpPathForImport = "";
        private string _ftpPathForExport = "";
        private string _localPathForImport = "";
        private string _localPathForExport = "";

        public Form1()
        {
            InitializeComponent();

            _ftpHost = ConfigurationManager.AppSettings["host"];
            _ftpUsername = ConfigurationManager.AppSettings["username"];
            _ftpPasswd = ConfigurationManager.AppSettings["password"];
            _ftpPathForImport = ConfigurationManager.AppSettings["ftpPathForImport"];
            _ftpPathForExport = ConfigurationManager.AppSettings["ftpPathForExport"];
            _localPathForImport = ConfigurationManager.AppSettings["localPathForImport"];
            _localPathForExport = ConfigurationManager.AppSettings["localPathForExport"];
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _ftpController = new FtpMessageController(_ftpHost, _ftpUsername, _ftpPasswd);

            //_ftpController.UploadFtpFile(_ftpPathForExport, @"c:\temp\fz_data\DEXX920050274_8500030631_A0131600112979.zip");

            //MessageBox.Show("OK");

            var msgController = new MessageInfoController();
            var lst = msgController.GetFiles(_localPathForExport);

            for (int i = 0; i < lst.Count; i++)
            {
                var fileInfo = lst[i];

                var resp = _ftpController.UploadFtpFile(_ftpPathForExport,
                    fileInfo.FullName);

                if (resp)
                {
                    System.IO.File.Delete(fileInfo.FullName);
                }
            }

            MessageBox.Show("Done");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _ftpController = new FtpMessageController(_ftpHost, _ftpUsername, _ftpPasswd);

            var data = _ftpController.ListFiles(_ftpPathForExport);

            for (int i = 0; i < data.Count(); i++)
            {
                string filename = data[i];
                string destinationFileName = System.IO.Path.Combine(_localPathForExport, filename);

                bool downstate = _ftpController.DownloadFile(filename, _ftpPathForExport, destinationFileName);

                if (downstate)
                {
                    var deleCode = _ftpController.Delete(filename, _ftpPathForExport);
                    Logger.EventLog(deleCode);
                }
            }

            MessageBox.Show("Done");
        }
    }
}
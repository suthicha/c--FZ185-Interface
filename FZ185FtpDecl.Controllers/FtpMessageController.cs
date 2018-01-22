using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace FZ185FtpDecl.Controllers
{
    public class FtpMessageController
    {
        private readonly string _host;
        private readonly string _username;
        private readonly string _password;

        public FtpMessageController(string host, string username, string password)
        {
            _host = host;
            _username = username;
            _password = password;
        }

        public bool UploadFtpFile(string ftpDestFolder, string fileName)
        {
            FtpWebRequest request;
            bool uploadStatus = false;

            try
            {
                Logger.EventLog("UPLOAD " + fileName);

                string absoluteFileName = Path.GetFileName(fileName);

                request = WebRequest.Create(new Uri(string.Format(@"ftp://{0}/{1}/{2}", _host, ftpDestFolder, absoluteFileName))) as FtpWebRequest;
                request.Method = WebRequestMethods.Ftp.UploadFile;
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                request.Credentials = new NetworkCredential(_username, _password);
                // request.ConnectionGroupName = "group";

                using (FileStream fs = File.OpenRead(fileName))
                {
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    fs.Close();
                    Stream requestStream = request.GetRequestStream();
                    requestStream.Write(buffer, 0, buffer.Length);
                    requestStream.Flush();
                    requestStream.Close();
                }

                Logger.EventLog("UPLOAD " + absoluteFileName + " ==> DONE");
                uploadStatus = true;
            }
            catch (Exception ex)
            {
                Logger.ErrorLog("UPLOAD " + ex.Message);
            }
            return uploadStatus;
        }

        public string[] ListFiles(string ftpFolder)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Format("ftp://{0}/{1}", _host, ftpFolder));
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                request.Credentials = new NetworkCredential(_username, _password);
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(responseStream);
                string names = reader.ReadToEnd();

                reader.Close();
                response.Close();

                return names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            }
            catch (Exception ex)
            {
                Logger.ErrorLog("ListFile " + ex.Message);
            }

            return null;
        }

        public string Delete(
            string filename,
            string ftpFolder)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Format("ftp://{0}/{1}/{2}", _host, ftpFolder, filename));

            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(_username, _password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusDescription;
            }
        }

        public bool DownloadFile(
            string filename,
            string ftpFolder,
            string localDestinationFilePath)
        {
            try
            {
                if (File.Exists(localDestinationFilePath))
                {
                    File.Delete(localDestinationFilePath);
                }

                int bytesRead = 0;
                byte[] buffer = new byte[2048];

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(string.Format("ftp://{0}/{1}/{2}", _host, ftpFolder, filename));

                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential(_username, _password);

                Stream reader = request.GetResponse().GetResponseStream();
                FileStream fileStream = new FileStream(localDestinationFilePath, FileMode.Create);

                while (true)
                {
                    bytesRead = reader.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0)
                        break;

                    fileStream.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }
            catch (Exception ex)
            {
                Logger.ErrorLog("Download File " + ex.Message);
            }

            return File.Exists(localDestinationFilePath);
        }
    }
}
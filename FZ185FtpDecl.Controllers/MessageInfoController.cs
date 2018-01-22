using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FZ185FtpDecl.Controllers
{
    public class MessageInfoController
    {
        public List<FileInfo> GetFiles(string path, string ext = "*.zip")
        {
            var di = new DirectoryInfo(path);
            return di.GetFiles(ext).ToList();
        }

        public void Delete(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch { }
        }
    }
}
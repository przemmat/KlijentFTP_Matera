using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace KlijentFTP
{
    internal class FtpClient
    {
        #region Pola
        private string host;
        private string userName;
        private string password;
        private string ftpDirectory;
        private bool downloadCompleated;
        private bool uploadCompleated;




        #endregion
        #region Własności
        public string Host { get => host; set => host = value; }
        public string UserName { get => userName; set => userName = value; }
        public string Password { get => password; set => password = value; }
        public string FtpDirectory { get
            { if (ftpDirectory.StartsWith("ftp://")) return ftpDirectory; else return "ftp://" + ftpDirectory; }
            set => ftpDirectory = value; }

        public bool DownloadCompleated { get => downloadCompleated; set => downloadCompleated = value; }
        public bool UploadCompleated { get => uploadCompleated; set => uploadCompleated = value; }
        #endregion
        public FtpClient()
        {
            downloadCompleated = true;
            uploadCompleated = true;
        }

        public FtpClient(string host, string userName, string password)
        {
            this.host = host;
            this.userName = userName;
            this.password = password;
            this.ftpDirectory = "ftp://" + this.host;
        }
        public ArrayList GetDirectories()
        {
            ArrayList directories = new ArrayList();
            FtpWebRequest request;
            try
            {
                request = (FtpWebRequest)WebRequest.Create(this.FtpDirectory);
                request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                request.Credentials = new NetworkCredential(this.userName, this.password);
                request.KeepAlive = false;
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string directory;
                        while ((directory = reader.ReadLine()) != null)
                            directories.Add(directory);
                    }
                }
                return directories;
            }
            catch
            {
                throw new Exception("Błąd: Nie można nawiązać połącznia z " + host);
            }
        }
        public ArrayList ChangeDirectory(string DirectoryName)
        {
            ftpDirectory += "/" + DirectoryName;
            return GetDirectories();
        }
        public ArrayList ChangeDirecoryUp()
        {
            if (ftpDirectory != "ftp://" + host)
            {
                ftpDirectory = ftpDirectory.Remove(ftpDirectory.LastIndexOf("/"), ftpDirectory.Length - ftpDirectory.LastIndexOf("/"));
                return GetDirectories();
            } else return GetDirectories();
        }
        public void DownloadFileAsync(string ftpFileName, string localFileName)
        {
            WebClient client = new WebClient();
            try
            {
                Uri uri = new Uri(ftpDirectory + "/" + ftpFileName);
                FileInfo file = new FileInfo(localFileName);
                if (file.Exists)
                    throw new Exception("Błąd: Plik " + localFileName + "instnieje");
                else
                {
                    client.DownloadFileCompleted += Client_DownloadFileCompleted;
                    client.DownloadProgressChanged += Client_DownloadProgressChanged;
                    client.Credentials = new NetworkCredential(this.userName, this.password);
                    client.DownloadFileAsync(uri, localFileName);
                    downloadCompleated = false;
                }
            }
            catch
            {
                client.Dispose();
                throw new Exception("Błą: Pobranie pliku niemozliwe");
            }
        }
        public void UploadFileAsync(string FileName)
        {
            try
            {
                System.Net.Cache.RequestCachePolicy chace = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.Reload);
                WebClient client = new WebClient();
                FileInfo file = new FileInfo(FileName);
                Uri uri = new Uri((FtpDirectory + '/' + file.Name).ToString());
                client.Credentials = new NetworkCredential(this.userName, this.password);
                uploadCompleated = false;
                if (file.Exists)
                {
                    client.UploadFileCompleted += Client_UploadFileCompleted;
                    client.UploadProgressChanged += Client_UploadProgressChanged;
                }
            }
            catch
            {
                throw new Exception("Bład: nie można wysłać pliku");
            }
        }
        public string DeleteFile(string nazwa)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpDirectory + "//" + nazwa);
                request.Method = WebRequestMethods.Ftp.DeleteFile;
                request.Credentials = new NetworkCredential(this.userName, this.password);
                request.KeepAlive = false;
                request.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                return response.StatusDescription;
            }
            catch (Exception ex)
            {
                throw new Exception("Błą: nie można usunąć pliku"+nazwa+" ("+ex.Message+")");
            }
        }
        private void Client_UploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
        {
            this.OnUploadProgressChanged(sender,e);
        }

        private void Client_UploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            this.OnUploadCompleted(sender,e);   
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.OnDownloadProgressChanged(sender, e);
        }

        private void Client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.OnDownloadCompleated(sender, e);
        }
        #region Zdarzenia

        public delegate void DownProgressChangedEventHandler(object sender, DownloadProgressChangedEventArgs e);
        public event DownProgressChangedEventHandler DownProgressChanged;
        protected virtual void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (DownProgressChanged != null) DownProgressChanged(sender, e);
        }
        public delegate void DownCompleatedEventHandler(object sender, System.ComponentModel.AsyncCompletedEventArgs e);
        public event DownCompleatedEventHandler DownCompleated;
        protected virtual void OnDownloadCompleated(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            if (DownCompleated != null) DownCompleated(sender, e);
        }
        public delegate void UpCompletedEventHandler(object sender, UploadFileCompletedEventArgs e);
        public event UpCompletedEventHandler UpCompleted;
        protected virtual void OnUploadCompleted(object sender, UploadFileCompletedEventArgs e)
        {
            if (UpCompleted != null) UpCompleted(sender, e);
        }
        public delegate void UpProgressChangedEventHandler(object sender, UploadProgressChangedEventArgs e);
        public event UpProgressChangedEventHandler UpProgressChanged;
        protected virtual void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
            {
            if (UpProgressChanged != null) UpProgressChanged(sender, e);
            }
        #endregion
    }

}

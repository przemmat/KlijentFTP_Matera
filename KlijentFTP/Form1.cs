﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KlijentFTP
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBoxLocalPath.Text = folderBrowserDialog1.SelectedPath;
        }
        private FtpClient client = new FtpClient();
        private void GetFtpContent(ArrayList directoriesList)
        {
            listBoxFtpDir.Items.Clear();
            listBoxFtpDir.Items.Add("[...]");
            directoriesList.Sort();
            foreach (string name in directoriesList) 
            {
            string position = name.Substring(name.LastIndexOf(' ')+1,name.Length - name.LastIndexOf(' ')-1);
                if(position!=".." && position!=".")
                    switch (name[0])
                    {
                        case 'd':
                            listBoxFtpDir.Items.Add("[" + position + "]");
                            break;
                        case 'l':
                            listBoxFtpDir.Items.Add("->" + position);
                            break;
                        default:
                            listBoxFtpDir.Items.Add(position); break;
                    }
            }
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if(comboBoxServer.Text!=string.Empty & comboBoxServer.Text.Trim() != String.Empty) 
            {
                try
                {
                    string serverName = comboBoxServer.Text;
                    if (serverName.StartsWith("ftp://"))
                        serverName = serverName.Replace("ftp://","");
                    client = new FtpClient(serverName, textBoxLogin.Text, maskedTextBoxPaxx.Text);
                    client.DownProgressChanged += Client_DownProgressChanged;
                    client.DownCompleated += Client_DownCompleated;
                    client.UpCompleted += Client_UpCompleted;
                    client.UpProgressChanged += Client_UpProgressChanged;
                    GetFtpContent(client.GetDirectories());
                    textBoxFtpPath.Text = client.FtpDirectory;
                    toolStripStatusLabelServer.Text = "Serwer: ftp://" + client.Host;
                    buttonConnect.Enabled = false;
                    buttonDisconnect.Enabled = true;
                    buttonDownload.Enabled = true;
                    buttonUpload.Enabled = true;

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
            }
            else
            { MessageBox.Show("Wprowadź nazwę serwera FTP", "Błąd");
            comboBoxServer.Text = string.Empty; }
        }

        private void Client_UpProgressChanged(object sender, System.Net.UploadProgressChangedEventArgs e)
        {
            toolStripStatusLabelServer.Text = "Wysłano: " + (e.BytesSent / (double)1024).ToString() + "kB";
        }

        private void Client_UpCompleted(object sender, System.Net.UploadFileCompletedEventArgs e)
        {
            if(e.Cancelled || e.Error != null)
            {
                MessageBox.Show("Błąd: " + e.Error.Message);
                client.UploadCompleated = true;
                buttonDownload.Enabled = true;
                buttonUpload.Enabled = true;
                return;
            }
            client.UploadCompleated = true;
            buttonUpload.Enabled= true;
            buttonDownload.Enabled= true;
            MessageBox.Show("Wysłano plik");
            try
            {
                GetFtpContent(client.GetDirectories());
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Błąd");
            }
        }

        private void Client_DownCompleated(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled || e.Error != null)
                MessageBox.Show("Błąd" + e.Error.Message);
            else MessageBox.Show("Plik pobrany");
            client.DownloadCompleated = true;
            buttonDownload.Enabled = true;
            buttonUpload.Enabled = true;
        }

        private void Client_DownProgressChanged(object sender, System.Net.DownloadProgressChangedEventArgs e)
        {
            toolStripStatusLabelServer.Text = "Pobrano: " + (e.BytesReceived / (double)1024).ToString() + "kB";
        }

        private void buttonDownload_Click(object sender, EventArgs e)
        {
            int index = listBoxFtpDir.SelectedIndex;
            if (listBoxFtpDir.Items[index].ToString()[0]!= '[')
            {
                if(MessageBox.Show("Czy pobrać plik?","Pobieranie pliku",MessageBoxButtons.OKCancel,MessageBoxIcon.Question)==DialogResult.OK)
                {
                    try
                    {
                        string localFile = textBoxLocalPath.Text + "\\" + listBoxFtpDir.Items.ToString();
                        FileInfo fi = new FileInfo(localFile);
                        if (fi.Exists == false)
                        {
                            client.DownloadFileAsync(listBoxFtpDir.Items[index].ToString(), localFile);
                            buttonDownload.Enabled = false;
                            buttonUpload.Enabled = false;
                        }
                        else MessageBox.Show("Plik istnieje");
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Błąd"); 
                    }
                }
            }
        }

        private void listBoxFtpDir_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            int index = listBoxFtpDir.SelectedIndex;
            try
            {
                if(index > -1)
                {
                    if (index == 0)
                    {
                        GetFtpContent(client.ChangeDirecoryUp());
                    }
                    else
                        if (listBoxFtpDir.Items[index].ToString()[0] == '[')
                    {
                        string directory = listBoxFtpDir.Items[index].ToString().Substring(1, listBoxFtpDir.Items[index].ToString().Length - 2);
                        GetFtpContent(client.ChangeDirectory(directory));
                    }
                    else
                        if (listBoxFtpDir.Items[index].ToString()[0] == '-' & listBoxFtpDir.Items[index].ToString()[2] == '.')
                    {
                        string link = listBoxFtpDir.Items[index].ToString().Substring(5, listBoxFtpDir.Items[index].ToString().Length - 5);
                        client.FtpDirectory = "ftp://" + client.Host;
                        GetFtpContent(client.ChangeDirectory(link));
                    }
                    else this.buttonUpDir_Click(sender, e);
                    listBoxFtpDir.SelectedIndex = 0;
                }
            }
            catch(Exception ex) { MessageBox.Show(ex.Message, "Błąd"); }
        }

        private void buttonUpDir_Click(object sender, EventArgs e)
        {

        }

        private void listBoxFtpDir_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter) { this.listBoxFtpDir_MouseDoubleClick(sender, null); }
        }

        private void buttonUpload_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog()==DialogResult.OK)
            {
                try
                {
                    client.UploadFileAsync(openFileDialog1.FileName);
                    buttonDownload.Enabled = false;
                    buttonUpload.Enabled = false;

                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Bład");
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode==Keys.F8) {
                int indeks = listBoxFtpDir.SelectedIndex;
                if(indeks > -1) {
                    if (listBoxFtpDir.Items[indeks].ToString()[0] !='[')
                    {
                        try
                        {
                            MessageBox.Show(client.DeleteFile(listBoxFtpDir.Items[indeks].ToString()));
                            GetFtpContent(client.GetDirectories());
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("Nie można usunąć pliku" + " (" + ex.Message + ")");
                        }
                    }
                            }
            }
        }

        private void buttonDisconnect_Click(object sender, EventArgs e)
        {
            client.DownProgressChanged -= Client_DownProgressChanged;
            client.DownCompleated -= Client_DownCompleated;
            client.UpCompleted -= Client_UpCompleted;
            client.UpProgressChanged -= Client_UpProgressChanged;
            buttonConnect.Enabled = true;
            listBoxFtpDir.Items.Clear();
            textBoxFtpPath.Text = "";
        }
    }
}

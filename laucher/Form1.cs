using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Xml.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace laucher
{
    public partial class Form1 : Form
    {
        string client_url = string.Empty;
        string new_version = string.Empty;
        bool complete = false;
        private bool dragging = false;
        private Point dragCursorPoint;
        private Point dragFormPoint;

        private Uri web_viewer { get; set; }

        public Form1()
        {
            InitializeComponent();
            button1.FlatAppearance.BorderSize = 0;
            pictureBox2.Visible = false;
            button1.TabStop = false;
            button1.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            backgroundWorker1.RunWorkerCompleted += BackgroundWorker1_RunWorkerCompleted;
            label2.Text = "v." + Application.ProductVersion;
            this.MouseDown += Form1_MouseDown1;
            this.MouseMove += Form1_MouseMove1;
            this.MouseUp += Form1_MouseUp1;
            webBrowser1.DocumentCompleted += WebBrowser1_DocumentCompleted;
            loadNews();
        }

        private void WebBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.AbsolutePath != (sender as WebBrowser).Url.AbsolutePath)
            return;
            webBrowser1.Visible = true;
            button6.Visible = true;
            button7.Visible = true;
        }

        private void loadNews()
        {
            button6.Visible = false;
            button7.Visible = false;
            button6.BackgroundImage = Properties.Resources.Circle_icon;
            button7.BackgroundImage = Properties.Resources.Circle_icon_gray;
            //-------------------------------------------------------------->
            web_viewer = new Uri("http://imperialageonline.servegame.com//client_n.php");
            webBrowser1.Visible = false;
            webBrowser1.Url = web_viewer;           
        }

        private void loadChangelog()
        {
            button6.Visible = false;
            button7.Visible = false;
            button7.BackgroundImage = Properties.Resources.Circle_icon;
            button6.BackgroundImage = Properties.Resources.Circle_icon_gray;
            //-------------------------------------------------------------->
            web_viewer = new Uri("http://imperialageonline.servegame.com//client_c.php");
            webBrowser1.Visible = false;
            webBrowser1.Url = web_viewer;
        }

        private void Form1_MouseUp1(object sender, MouseEventArgs e)
        {
            dragging = false;
        }

        private void Form1_MouseMove1(object sender, MouseEventArgs e)
        {
            if (dragging)
            {
                Point dif = Point.Subtract(Cursor.Position, new Size(dragCursorPoint));
                this.Location = Point.Add(dragFormPoint, new Size(dif));
            }
        }

        private void Form1_MouseDown1(object sender, MouseEventArgs e)
        {
            dragging = true;
            dragCursorPoint = Cursor.Position;
            dragFormPoint = this.Location;
        }

        private void BackgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            pictureBox2.Visible = false;
            if (complete)
            {
                label1.Text = "Complete";
                setClientVersion();
                complete = false;
                lauchClient();
            }
            else
            {
                label1.Text = "Fail";
            }
        }

        private int stringToInt(string num)
        {
            int r = 0;
            try
            {
                r = int.Parse(num);
            }
            catch (Exception)
            {

            }
            return r;
        }

        private void lauchClient()
        {
            try
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\Imperial Age Online.exe");
                Application.Exit();
            }
            catch (Exception)
            {
                MessageBox.Show(this, "[Error] Imperial Age Online.exe is not found in the installation folder or is damaged.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void checkingUpdate()
        {
            string local_cv = string.Empty;
            label1.Text = "Connecting...";
            //------------------------------------------------>
            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += (sender, e) =>
            {
                try
                {
                    WebClient wc = new WebClient();
                    string html = wc.DownloadString("http://imperialageonline.servegame.com/client_url.php");
                    string[] s = html.Split(',');
                    if (s.Length == 2)
                    {
                        new_version = s[0];
                        client_url = s[1];
                        local_cv = getClientVersion();                       
                    }
                    else
                    {
                        MessageBox.Show(this, "[Error] url not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, "[Error] Cannot connect to the update server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    lauchClient();
                    errorLog(err.ToString());
                }
            };
            bg.RunWorkerCompleted += (sender, e) =>
            {
                label1.Text = string.Empty;
                if (new_version != string.Empty)
                {
                    if (local_cv != string.Empty)
                    {
                        if (stringToInt(new_version) > stringToInt(local_cv))
                        {
                            downloadClient();
                        }
                        else
                        {
                            lauchClient();
                        }
                    }
                    else
                    {
                        MessageBox.Show(this, "[Error] the client.xml file is not found in the installation folder or is damaged.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show(this, "[Error] version not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            if (!bg.IsBusy)
            {
                bg.RunWorkerAsync();
            }
        }

        private void setClientVersion()
        {
            try
            {
                XDocument client_xml = XDocument.Load(Application.StartupPath + "\\client.xml");

                var version = from x in client_xml.Descendants("Client") select x;

                foreach (XElement x in version)
                {
                    x.SetElementValue("URL", new_version);                  
                }
                client_xml.Save(Application.StartupPath + "\\client.xml");
            }
            catch (Exception)
            {
                MessageBox.Show(this, "[Error] the client.xml file is not found in the installation folder or is damaged.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string getClientVersion()
        {
            string v = string.Empty;
            try
            {
                XDocument client_xml = XDocument.Load(Application.StartupPath + "\\client.xml");

                var version = (from x in client_xml.Descendants("Client") select x.Element("URL")).SingleOrDefault();

                if(version != null)
                {
                    v = version.Value;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(this, "[Error] the client.xml file is not found in the installation folder or is damaged.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return v;
        }

        private void downloadClient()
        {           
            button1.Enabled = false;
            pictureBox2.Visible = true;
            startDownload();           
        }

        private void startDownload()
        {
            try
            {
                Thread thread = new Thread(() =>
                {
                    label1.Text = "Updating... ";
                    System.Threading.Thread.Sleep(2000);
                    WebClient client = new WebClient();
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(client_DownloadProgressChanged);
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler(client_DownloadFileCompleted);
                    client.DownloadFileAsync(new Uri(client_url), Application.StartupPath + @"\client.zip");                   
                });
                thread.Start();
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "[Error] try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;
                pictureBox2.Visible = false;
                errorLog(err.ToString());
            }
        }

        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                this.BeginInvoke((MethodInvoker)delegate
                {
                    label1.Text = "Downloaded: " +  Math.Round((e.BytesReceived / 1000000f), 2) + " MB / " + Math.Round((e.TotalBytesToReceive / 1000000f), 2) + " MB";
                });
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "[Error] try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;
                pictureBox2.Visible = false;
                errorLog(err.ToString());
            }
        }

        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            this.BeginInvoke((MethodInvoker)delegate {
                decompressFiles();
            });
        }

        void decompressFiles()
        {
            if (!backgroundWorker1.IsBusy)
            {
                label1.Text = "Installing...";
                backgroundWorker1.RunWorkerAsync();
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string zipfile = Application.StartupPath + "\\client.zip";
            bool error = false;
            try
            {
                ZipFile.OpenRead(zipfile);
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "[Error] try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                button1.Enabled = true;
                pictureBox2.Visible = false;
                error = true;
                errorLog(err.ToString());
            }

            if (!error)
            {
                try
                {
                    if (File.Exists(Application.StartupPath + "\\Imperial Age Online.exe"))
                    {
                        File.Delete(Application.StartupPath + "\\Imperial Age Online.exe");
                    }
                    ZipFile.ExtractToDirectory(zipfile, Application.StartupPath);
                    if (File.Exists(Application.StartupPath + "\\client.zip"))
                    {
                        File.Delete(Application.StartupPath + "\\client.zip");
                    }
                    complete = true;
                }
                catch (Exception err)
                {
                    MessageBox.Show(this, "[Error] <?>.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    button1.Enabled = true;
                    pictureBox2.Visible = false;
                    errorLog(err.ToString());
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkingUpdate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://imperialageonline.servegame.com");
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "[Error] try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errorLog(err.ToString());
            }
        }

        private void errorLog(string msg)
        {
            string path = Application.StartupPath + "\\laucher.txt";
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
                using (var tw = new StreamWriter(path, true))
                {
                    tw.Write(msg);
                }
            }
            else if (File.Exists(path))
            {
                using (var tw = new StreamWriter(path, true))
                {
                    tw.Write(msg);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Process[] p = System.Diagnostics.Process.GetProcessesByName(System.Diagnostics.Process.GetCurrentProcess().ProcessName);
            if (p.Length > 1)
            {
                Environment.Exit(0);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Environment.Exit(1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://www.facebook.com/Imperial-Age-Online-413004102183212/?_rdc=1&_rdr");
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "[Error] try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errorLog(err.ToString());
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("https://discord.gg/bgxdQZA");
            }
            catch (Exception err)
            {
                MessageBox.Show(this, "[Error] try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                errorLog(err.ToString());
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            loadNews();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            loadChangelog();
        }
    }
}

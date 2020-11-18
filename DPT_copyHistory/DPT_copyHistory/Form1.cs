using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DPT_copyHistory
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        public string DPTpath, MESpath;
        public DateTime now;
        private bool breakThread = false;
        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialogDPT.ShowDialog();
            DPTpath = folderBrowserDialogDPT.SelectedPath;
            textBox2.Text = DPTpath;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialogMES.ShowDialog();
            MESpath = folderBrowserDialogMES.SelectedPath;
            textBox1.Text = MESpath;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"C:\DEV_PJT\DPT_MES_BRIDE\")) Directory.CreateDirectory(@"C:\DEV_PJT\DPT_MES_BRIDE\");
            string config = DPTpath + Environment.NewLine + MESpath;
            File.WriteAllText(@"C:\DEV_PJT\DPT_MES_BRIDE\config.cfg", config);


            notifyIcon.Visible = true;
            now = DateTime.Now;
            string folderResult = now.ToString("yyyyMM");
            textBox2.Text = DPTpath;
            textBox1.Text = MESpath;

            if (notifyIcon.Visible)
            {
                Thread thread = new Thread(copyFile);
                thread.Start();
            }

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(@"C:\DEV_PJT\DPT_MES_BRIDE\" + "config.cfg"))
            {
                string[] config = File.ReadAllLines(@"C:\DEV_PJT\DPT_MES_BRIDE\" + "config.cfg");
                DPTpath = config[0];
                MESpath = config[1];
                textBox2.Text = DPTpath;
                textBox1.Text = MESpath;
            }
        }

        public void copyFile()
        {
            string buffer = "";
            while (true)
            {
                if (!notifyIcon.Visible)
                {
                    break;
                }
                now = DateTime.Now;
                string folderResult = now.ToString("yyyyMM");

                if (!Directory.Exists(MESpath + @"\" + folderResult)) Directory.CreateDirectory(MESpath + @"\" + folderResult);
                if (!File.Exists(MESpath + @"\" + folderResult + @"\" + folderResult + ".rlt"))
                {
                    if (Directory.Exists(DPTpath + @"\" + folderResult))
                    {
                        Console.WriteLine(DPTpath + @"\" + folderResult);
                        foreach (string f in Directory.GetFiles(DPTpath + @"\" + folderResult, "*.rlt"))
                        {
                            string extension = Path.GetExtension(f);
                            if (extension != null && (extension.Equals(".rlt")))
                            {
                                FileInfo fileInfo = new FileInfo(f);
                                if (!IsFileLocked(fileInfo))
                                {
                                    string alldata = File.ReadAllText(f);
                                    string newdata;
                                    if (alldata.Contains(buffer))
                                    {
                                        newdata = alldata.Remove(0, buffer.Length);
                                    }
                                    else
                                    {
                                        newdata = alldata;
                                    }
                                    File.WriteAllText(MESpath + @"\" + folderResult + @"\" + fileInfo.Name, newdata);
                                    buffer = alldata;
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(60000);
            }
            notifyIcon.Visible = false;
        }
        protected virtual bool IsFileLocked(FileInfo file)
        {
            try
            {
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

    }
}

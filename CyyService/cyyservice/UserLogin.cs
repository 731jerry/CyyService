using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.Net;
using System.IO;

namespace CyyService
{
    public partial class UserLogin : Form
    {
        public UserLogin()
        {
            InitializeComponent();
            updateVerionTimer.Enabled = true;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.caiyingying.com");
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            btLogin.Enabled = cb.Checked;
            btLogin.BackColor = btLogin.Enabled == true ? SystemColors.ActiveCaption : Color.Transparent;
        }

        private void btLogin_Click(object sender, EventArgs e)
        {
            if (!tbUserName.Text.Equals("") && !tbPassword.Text.Equals(""))
            {
                if (Form1.GetInstence().LoginSystem(tbUserName.Text.Trim(), tbPassword.Text.Trim()))
                {
                    this.Close();
                }
            }
            else {
                MessageBox.Show("帐号或者密码不能为空!","警告");
            }
        }

        private string getOnlineFile(string fileUrl)
        {

            WebClient wcClient = new WebClient();

            long fileLength = 0;

            string updateFileUrl = fileUrl;

            WebRequest webReq = WebRequest.Create(updateFileUrl);
            WebResponse webRes = null;
            Stream srm = null;
            StreamReader srmReader = null;
            string ss = "";
            try
            {
                webRes = webReq.GetResponse();
                fileLength = webRes.ContentLength;

                srm = webRes.GetResponseStream();
                srmReader = new StreamReader(srm);

                byte[] bufferbyte = new byte[fileLength];
                int allByte = (int)bufferbyte.Length;
                int startByte = 0;
                while (fileLength > 0)
                {
                    Application.DoEvents();
                    int downByte = srm.Read(bufferbyte, startByte, allByte);
                    if (downByte == 0) { break; };
                    startByte += downByte;
                    allByte -= downByte;

                    float part = (float)startByte / 1024;
                    float total = (float)bufferbyte.Length / 1024;
                    int percent = Convert.ToInt32((part / total) * 100);

                }
                ss = Encoding.Default.GetString(bufferbyte).Trim();

                srm.Close();
                srmReader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取信息异常: " + ex.Message);
                //throw;
                return "";
            }
            return ss;
        }


        // 比较版本是否需要更新
        private bool compareVersion(string currentVersion, string newVersion, int versionBit)
        {
            string[] currentVersionArray = currentVersion.Split('.');
            string[] newVersionArray = newVersion.Split('.');

            for (int i = 0; i < currentVersionArray.Length; i++)
            {
                if (currentVersionArray[i].Length < versionBit)
                {
                    currentVersionArray[i] = "0" + currentVersionArray[i];
                }
                if (newVersionArray[i].Length < versionBit)
                {
                    newVersionArray[i] = "0" + newVersionArray[i];
                }
            }
            string currentVersionString = currentVersionArray[0] + currentVersionArray[1] + currentVersionArray[2];
            string newVersionString = newVersionArray[0] + newVersionArray[1] + newVersionArray[2];
            if (int.Parse(currentVersionString) < int.Parse(newVersionString))
            {
                return true;
            }
            return false;
        }

        private void updateVerionTimer_Tick(object sender, EventArgs e)
        {
            updateVerionTimer.Enabled = false;
            string ss = getOnlineFile(@"http://www.caiyingying.com/products/CyyService/Version.txt");

            if (!ss.Equals(""))
            {
                bool ok = false;
                if (!Form1.proudctVersionString.Equals(""))
                {
                    string localVersionString = "";
                    string proudctVersionString = Form1.proudctVersionString;
                    if (proudctVersionString.Split('.').Length == 2)
                    {
                        localVersionString = proudctVersionString + ".0";
                    }
                    else
                    {
                        localVersionString = proudctVersionString;
                    }

                    if (compareVersion(localVersionString, ss, 2))
                    {
                        ok = true;
                        /* using (FileStream fs = new FileStream(System.Environment.CurrentDirectory + @"\Version.txt", FileMode.OpenOrCreate, FileAccess.Write))
                        / {
                             fs.Write(bufferbyte, 0, bufferbyte.Length);
                         }  
                         */
                    }

                    if (ok)
                    {
                        string updateLog = getOnlineFile(@"http://www.caiyingying.com/products/CyyService/updateLog.txt");

                        if (!updateLog.Equals(""))
                        {
                            update ud = new update(@"http://www.caiyingying.com/products/CyyService/" + Form1.productKeyNameString + Form1.productKeyVersionNameString + @"v" + ss + ".exe", ss, updateLog);
                            ud.ShowDialog(this);
                        }
                    }
                }
            }
        }


    }
}

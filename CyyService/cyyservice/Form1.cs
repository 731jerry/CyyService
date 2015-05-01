using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using LitJson;
using OCAPIDemo;

namespace CyyService
{
    public partial class Form1 : Form
    {

        public static Form1 _form = null;

        public UserLogin userLogin;
        public Form1()
        {
            InitializeComponent();
        }

        public static Form1 GetInstence()
        {
            if (_form == null || _form.IsDisposed)
            {
                _form = new Form1();
            }
            return _form;
        }

        private bool isWindowLoaded = false;

        // 已经修改了DetailForm
        public static bool isModified;

        // 是否帐号密码正确
        public static bool USER_IS_CORRECT;
        // 代理商登录名
        public static string USER_ALAIS;
        // 代理商姓名
        public static string USER_NAME = "-1";
        // 代理商到期时间
        public static DateTime USER_LIS_DATE;
        // 代理商等级 省3 市4 县5 兼职6
        public static int USER_DEGREE;
        // 用户权限 000 查看本级会员 代理注册会员 限制会员登录
        public static string USER_QX;
        public static int[] USER_QX_BOOL_ARRAY = new int[3];
        // province, city, area
        public static string USER_PROVINCE;
        public static string USER_CITY;
        public static string USER_DISTRICT;

        Microsoft.Win32.RegistryKey productKey;
        public static string productKeyNameString = ""; // 彩盈盈
        public static string productKeyVersionNameString = ""; // 精华版
        public static string proudctVersionString = ""; // 2.9

        public struct OnlineUsers
        {
            public string user_name { get; set; }
            public string realname { get; set; }
            public string nickname { get; set; }
            public string userDegree { get; set; }
            public string province { get; set; }
            public string city { get; set; }
            public string area { get; set; }
            public string lastlogonTime { get; set; }
            public string versionName { get; set; }
            public string versionID { get; set; }
            public string login_IP { get; set; }
            public string login_Address { get; set; }


        }
        public List<OnlineUsers> onlineUsersList;

        public struct GeneralUsers
        {
            public string id { get; set; }
            public string user_name { get; set; }
            public string realname { get; set; }
            public string nickname { get; set; }
            public string userDegree { get; set; }
            public string cyykey { get; set; }
            public string lisExpireTime { get; set; }
            public string lastlogonTime { get; set; }
            public string reg_time { get; set; }
            public string province { get; set; }
            public string city { get; set; }
            public string area { get; set; }
            public string mobile_phone { get; set; }
            public string qq { get; set; }
            public int paid { get; set; }

        }
        public List<GeneralUsers> generalUsersList;

        public struct CPinfo
        {
            public string cpName { get; set; }
            public string cpTpye { get; set; }
            public string cpDay { get; set; }
            public string cpData { get; set; }

        }
        public List<CPinfo> cpInfoList;

        public struct ProgramWarning
        {
            public string pwID { get; set; }
            public string pwWarningText { get; set; }
            public string pwShown { get; set; }

        }
        public List<ProgramWarning> programWarningList;

        //    private const string sqlConnectionCommand = @"server=qdm-011.hichina.com; user id=qdm0110106; password=CYYDB2014; database=qdm0110106_db;persist security info=False; max pool size=500;";
        private const string sqlConnectionCommand = @"server=120.27.30.10; user id=admin; password=admin; database=cyydb;Charset=utf8";

        private MySqlConnection sqlConnection = new MySqlConnection(sqlConnectionCommand);

        #region 自动关闭
        /*
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public const int WM_CLOSE = 0x10;

        private void StartKiller()
        {
            Timer timer = new Timer();
            timer.Interval = 3000;//10秒启动
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            KillMessageBox();
            //停止计时器
            ((Timer)sender).Stop();
        }

        private void KillMessageBox()
        {
            //查找MessageBox的弹出窗口,注意对应标题
            IntPtr ptr = FindWindow(null, "MessageBox");
            if (ptr != IntPtr.Zero)
            {
                //查找到窗口则关闭
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        private void KillAutoRestartErrorMessageBox(string title)
        {
            IntPtr ptr = FindWindow(null, title);
            if (ptr != IntPtr.Zero)
            {
                //查找到窗口则关闭
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }
        */
        #endregion

        // 打开连接
        private void DBOpen()
        {
            try
            {
                if (sqlConnection.State != ConnectionState.Open)
                {
                    sqlConnection.Open();
                }
            }
            catch (Exception ex)
            {
                //KillAutoRestartErrorMessageBox("无法打开连接！");
                RecordLog("无法打开连接!\r\nTargetSite: " + ex.TargetSite + "\r\n" + ex.ToString());
                MessageBox.Show(ex.Message, "无法打开连接！");
                return;
            }
        }

        // 关闭连接
        public void DBClose()
        {
            sqlConnection.Close();
        }


        public bool LoginSystem(string user_name, string password)
        {
            DBOpen();

            MD5 md5Hash = MD5.Create();
            string hash = GetMd5Hash(md5Hash, password);

            string SQLforGeneral = @"SELECT COUNT(user_name), realname, nickname, province, city, area, degreeid, DLquanxian, UserLisDay FROM ecs_users WHERE user_name = '" + user_name + "'" + " AND password = '" + hash.ToLower() + "'";

            try
            {
                //sqlConnection.Open();

                MySqlCommand cmd = new MySqlCommand(SQLforGeneral, sqlConnection);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    USER_IS_CORRECT = int.Parse(dataReader["COUNT(user_name)"].ToString()) == 1 ? true : false;
                    USER_NAME = user_name;
                    USER_ALAIS = dataReader["nickname"].ToString().Equals("") ? "" : dataReader["nickname"].ToString();
                    USER_DEGREE = int.Parse(dataReader["degreeid"].ToString().Equals("") ? "-1" : dataReader["degreeid"].ToString()); // 3 4 5 6
                    USER_QX = dataReader["DLquanxian"].ToString().Equals("") ? "" : dataReader["DLquanxian"].ToString();
                    USER_LIS_DATE = DateTime.Parse(dataReader["UserLisDay"].ToString().Equals("") ? "1914-08-11 12:58:39" : dataReader["UserLisDay"].ToString());
                    USER_PROVINCE = dataReader["province"].ToString();
                    USER_CITY = dataReader["city"].ToString();
                    USER_DISTRICT = dataReader["area"].ToString();
                }

                dataReader.Close();
                DBClose();

                if (!USER_IS_CORRECT)
                {
                    MessageBox.Show("帐号密码错误, 请重新输入!", "提示");
                    return false;
                }
                // 0超级 1普通 2高级 3省级 4市级 5县级 6兼职
                if ((USER_DEGREE == 1) || (USER_DEGREE == 2))
                {
                    MessageBox.Show("您还不是代理商, 请与超级管理员确认!", "提示");
                    return false;
                }
                if (USER_LIS_DATE < DateTime.Now)
                {
                    MessageBox.Show("帐号已到期!", "提示");
                    return false;
                }

                // 权限
                USER_QX_BOOL_ARRAY[0] = int.Parse(USER_QX.Substring(0, 1));
                USER_QX_BOOL_ARRAY[1] = int.Parse(USER_QX.Substring(1, 1));
                USER_QX_BOOL_ARRAY[2] = int.Parse(USER_QX.Substring(2, 1));

                return true;
            }
            catch (Exception e)
            {
                if (MessageBox.Show("登录出现异常,请重启软件!" + e.Message, "警告", MessageBoxButtons.OK) == DialogResult.OK)
                {
                    Application.Exit();
                }
                return false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            productKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\彩盈盈后台管理系统\");
            try
            {
                productKeyNameString = productKey.GetValue("DisplayName").ToString();
                productKeyVersionNameString = productKey.GetValue("VersionName").ToString(); ;
                proudctVersionString = productKey.GetValue("DisplayVersion").ToString();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }

            userLogin = new UserLogin();
            this.Visible = false;
            userLogin.ShowDialog(this);

            if (USER_NAME.Equals("-1"))
            {
                //DbClose();
                this.Close();
                Application.Exit();
                return;
            }

            lbAppVersion.Text = "v" + proudctVersionString;

            UpdateUserOnlineInfoTimer.Enabled = false;

            tabControl1.Enabled = false;
            onlineUsersList = new List<OnlineUsers>();
            generalUsersList = new List<GeneralUsers>();
            programWarningList = new List<ProgramWarning>();

            cpInfoList = new List<CPinfo>();

            // 会员管理
            contextMenuStrip1.Items.Add("拷贝当前会员");
            contextMenuStrip1.Items.Add("删除当前会员");
            contextMenuStrip1.Items[0].Click += new EventHandler(contextMenuStrip1_click_copy);
            contextMenuStrip1.Items[1].Click += new EventHandler(contextMenuStrip1_click_delete);

            // 实时在线会员
            contextMenuStrip2.Items.Add("拷贝当前实时在线会员");
            contextMenuStrip2.Items.Add("删除当前实时在线会员");
            contextMenuStrip2.Items[0].Click += new EventHandler(contextMenuStrip2_click_copy);
            contextMenuStrip2.Items[1].Click += new EventHandler(contextMenuStrip2_click_delete);

            // SD = 1, GD_11_5, JX_11_5, CQ_11_5, JS_11_5, ZJ_11_5, SH_11_5
            cpTpyeComboBox.Items.Add("山东11选5");
            cpTpyeComboBox.Items.Add("广东11选5");
            cpTpyeComboBox.Items.Add("江西11选5");
            cpTpyeComboBox.Items.Add("重庆11选5");
            cpTpyeComboBox.Items.Add("江苏11选5");
            cpTpyeComboBox.Items.Add("浙江11选5");
            cpTpyeComboBox.Items.Add("上海11选5");

            // SD = 2, GD_11_5, JX_11_5, CQ_11_5, JS_11_5, ZJ_11_5, SH_11_5
            cpTpyeComboBox.Items.Add("山东11选3");
            cpTpyeComboBox.Items.Add("广东11选3");
            cpTpyeComboBox.Items.Add("江西11选3");
            cpTpyeComboBox.Items.Add("重庆11选3");
            cpTpyeComboBox.Items.Add("江苏11选3");
            cpTpyeComboBox.Items.Add("浙江11选3");
            cpTpyeComboBox.Items.Add("上海11选3");

            cpTpyeComboBox.SelectedItem = cpTpyeComboBox.Items[0];

            StartService(); // 开启服务

            // test
            LabelUserName.Text = USER_ALAIS;
            isWindowLoaded = true;
        }

        private string cpNameString = "11选5";
        private int cpNameInt = 1;
        private int realCpNameInt = 1;
        private string cpTypeString = "山东";
        private int cpTypeInt = 1;

        // 定义彩票
        private void defineSelectedCPType(string selected)
        {
            // 11选5
            if (selected.Equals("山东11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "山东";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 1;
            }
            else if (selected.Equals("广东11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "广东";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 2;
            }
            else if (selected.Equals("江西11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "江西";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 3;
            }
            else if (selected.Equals("重庆11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "重庆";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 4;
            }
            else if (selected.Equals("江苏11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "江苏";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 5;
            }
            else if (selected.Equals("浙江11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "浙江";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 6;
            }
            else if (selected.Equals("上海11选5"))
            {
                cpNameString = "11选5";
                cpTypeString = "上海";
                cpNameInt = 1;
                realCpNameInt = 1;
                cpTypeInt = 7;
            }

            // 11选3
            else if (selected.Equals("山东11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "山东";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 1;
            }
            else if (selected.Equals("广东11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "广东";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 2;
            }
            else if (selected.Equals("江西11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "江西";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 3;
            }
            else if (selected.Equals("重庆11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "重庆";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 4;
            }
            else if (selected.Equals("江苏11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "江苏";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 3;
            }
            else if (selected.Equals("浙江11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "浙江";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 6;
            }
            else if (selected.Equals("上海11选3"))
            {
                cpNameString = "11选3";
                cpTypeString = "上海";
                cpNameInt = 2;
                realCpNameInt = 2;
                cpTypeInt = 7;
            }
        }

        // 开启服务
        private void StartService()
        {
            //DBOpen();
            UpdateUserOnlineInfoTimer.Enabled = true;
            tabControl1.Enabled = true;
            //clearOnlineUsersButton.Enabled = true;

            //updateOnlineUserInfo(120);

            // 提示
            OnlineInfoLabel.Enabled = true;
            manProgressLabel.Enabled = true;
            UserCountsLabel.Enabled = true;

            dataGridView3.Enabled = true;

            if (USER_DEGREE == 0)
            {
                cbUserTypeSelector.SelectedIndex = 0;
                updateUserPasswordTimer.Enabled = true;
                button3.Visible = false;
            }
            else
            {
                if (USER_QX_BOOL_ARRAY[0] == 0)
                {
                    tabPage2.Parent = null;
                }
                if (USER_QX_BOOL_ARRAY[1] == 0)
                {
                    button8.Visible = false;
                }
                if (USER_QX_BOOL_ARRAY[2] == 0)
                {
                    tabPage5.Parent = null;
                }

                tabPage3.Parent = null;
                tabPage4.Parent = null;

                cbUserTypeSelector.Visible = false;

                //button3.PerformClick();
                RefreshGeneralUser(GetQueryByDegree());
                //manProgressLabel.Text = "修改会员数据成功!";
            }

            //button3_Click(sender,e);
            manProgressLabel.Text = "服务已开启";
        }

        // 关闭服务
        private void StopService()
        {
            manProgressLabel.Text = "正在关闭服务...";

            //panel1.Visible = true;
            updateUserPasswordTimer.Enabled = false;
            UpdateUserOnlineInfoTimer.Enabled = false;
            //clearOnlineUsersButton.Enabled = false;

            dataGridView1.Rows.Clear();
            dataGridView2.Rows.Clear();

            tabControl1.Enabled = false;

            // 提示
            OnlineInfoLabel.Enabled = false;
            manProgressLabel.Enabled = false;
            UserCountsLabel.Enabled = false;

            dataGridView3.Enabled = false;

            //DbClose();
            manProgressLabel.Text = "服务已关闭";
        }

        private void updateUserPasswordTimer_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            //now.ToString("HH:mm:ss");
            // 1000 * 60 = 1min 每10分钟检测 每天12点运行一次
            if (now.Hour == 12)
            {
                updateUserPasswordTimer.Enabled = false;
                if (USER_DEGREE == 0)
                {
                    updateUserPasswordFun();
                }
            }
        }

        private void updateUserPasswordFun()
        {
            try
            {
                string randompassword = generateRandomPassword();

                string SQLforGeneral = @"UPDATE cyy_lngpack SET langstr = '" + randompassword + @"' WHERE  lpid= '857';";

                MD5 md5Hash = MD5.Create();
                string hashRandompassword = GetMd5Hash(md5Hash, randompassword).ToLower();

                string SQLforNormalUsers = @"UPDATE ecs_users SET password = '" + hashRandompassword + @"' WHERE  degreeid= '1';";

                string SQLcommand = SQLforGeneral + "\r\n" + SQLforNormalUsers;

                DBOpen();
                MySqlCommand cmd = new MySqlCommand(SQLcommand, sqlConnection);
                cmd.ExecuteNonQuery();

                MessageBox.Show("基础版用户动态密码更新成功!", "提示");
                RecordLog("***更新普通用户密码 updateUserPasswordTimer_Tick\n");

                DBClose();
            }
            catch (Exception e)
            {
                //StopService();
                //lanuchRestartService();
                RecordLog("更新普通会员密码出错, 已关闭服务, 稍后将自动重启\r\nTargetSite: " + e.TargetSite + "\r\n" + e.ToString());
                MessageBox.Show(e.Message, "更新普通会员密码出错, 已关闭服务, 稍后将自动重启");
                //MessageBox.Show("", "更新普通会员密码出错, 已关闭服务, 稍后将自动重启");
            }
        }

        // 产生随机密码 
        // 格式： C 6位数字 YY
        private string generateRandomPassword()
        {
            Random rNumber = new Random();
            int randomN = rNumber.Next(100000, 999999);
            return "C" + randomN + "YY";
        }

        public string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        // 每
        private void UpdateUserOnlineInfoTimer_Tick(object sender, EventArgs e)
        {
            // dataGridView1
            updateOnlineUserInfo(120);
            //button3.PerformClick();
        }

        private void updateOnlineUserInfo(int delaySeconds)
        {
            try
            {
                /*
                    DBOpen();
                    string SQL = @"SELECT TIMESTAMPDIFF( 
                            SECOND , UserLogined_DaySave, NOW() ) AS diff, mid
                            FROM cyy_OnlinesUsers";
                    MySqlCommand cmdDiffAll = new MySqlCommand(SQL, sqlConnection);
                    MySqlDataReader dataReaderDiffAll = cmdDiffAll.ExecuteReader();

                    List<int> midList = new List<int>();
                    while (dataReaderDiffAll.Read())
                    {
                        int aa = int.Parse(dataReaderDiffAll["diff"].ToString());
                        if (int.Parse(dataReaderDiffAll["diff"].ToString()) > delaySeconds)
                        {
                            midList.Add(int.Parse(dataReaderDiffAll["mid"].ToString()));
                        }
                    }

                    dataReaderDiffAll.Close();

                    foreach (int i in midList)
                    {
                        string SQL2 = @"DELETE FROM cyy_OnlinesUsers WHERE mid = " + @"'" + i.ToString() + @"'";

                        MySqlCommand cmd3 = new MySqlCommand(SQL2, sqlConnection);
                        cmd3.ExecuteNonQuery();

                    }

                    DBClose();
                    */
                // 更新数据
                getOnlineUserInfo(GetQueryByDegree());
            }

            catch (Exception e)
            {
                string title = "更新会员数据时出错, 已关闭服务, 稍后将自动重启";
                //KillAutoRestartErrorMessageBox(title);

                //Application.Exit();
                //StopService();
                //lanuchRestartService();
                RecordLog(title + "\r\nTargetSite: " + e.TargetSite + "\r\n" + e.ToString());
                MessageBox.Show(e.Message, title);
                //MessageBox.Show("", "更新会员数据时出错, 已关闭服务");
            }
        }

        private void getOnlineUserInfo(string condition)
        {
            dataGridView1.Rows.Clear();
            onlineUsersList.Clear();
            try
            {
                if (UpdateUserOnlineInfoTimer.Enabled)
                {
                    DBOpen();

                    string SQLforGeneral = @"SELECT user_name, realname, nickname, degreeid, province, city, area, cyykey, UserLogined_DaySave, versionName, versionID, UserLogined_ip, UserLogined_Address From cyy_OnlinesUsers " + condition;

                    MySqlCommand cmd = new MySqlCommand(SQLforGeneral, sqlConnection);
                    MySqlDataReader dataReader = cmd.ExecuteReader();

                    int counts = 0;
                    while (dataReader.Read())
                    {
                        onlineUsersList.Add(new OnlineUsers
                        {
                            user_name = dataReader["user_name"].ToString(),
                            realname = USER_DEGREE == 0 ? dataReader["realname"].ToString() : "********",
                            nickname = dataReader["nickname"].ToString(),
                            userDegree = dataReader["degreeid"].ToString(),
                            province = dataReader["province"].ToString(),
                            city = dataReader["city"].ToString(),
                            area = dataReader["area"].ToString(),
                            lastlogonTime = dataReader["UserLogined_DaySave"].ToString(),
                            versionName = dataReader["versionName"].ToString(),
                            versionID = dataReader["versionID"].ToString(),
                            login_IP = dataReader["UserLogined_ip"].ToString(),
                            login_Address = dataReader["UserLogined_Address"].ToString(),
                        });
                        counts++;
                    }

                    dataReader.Close();
                    DBClose();

                    if (counts > 0)
                    {
                        OnlineInfoLabel.Text = "当前在线会员：" + counts + "位";

                        int id = 1;
                        foreach (OnlineUsers ou in onlineUsersList)
                        {
                            dataGridView1.Rows.Add(
                                ou.user_name,
                                ou.nickname,
                                ou.realname,
                                GetDegreeNameByInt(int.Parse(ou.userDegree.ToString())),
                                ou.login_Address,
                                ou.versionName + " " + ou.versionID,
                                ou.province,
                                ou.city,
                                ou.login_IP
                                );

                            if (dataGridView1.Rows[id - 1].Cells[3].Value.ToString().Equals("高级会员"))
                            {
                                dataGridView1.Rows[id - 1].Cells[3].Style.ForeColor = System.Drawing.Color.Red;
                            }
                            id++;
                        }

                        dataGridView1.Rows[0].Selected = false;
                    }
                    else
                    {
                        OnlineInfoLabel.Text = "当前在线会员：0位";
                    }
                }
            }
            catch (Exception e)
            {
                string title = "获取会员数据时出错, 已关闭服务";
                //StopService();
                //lanuchRestartService();
                RecordLog(title + "\r\nTargetSite: " + e.TargetSite + "\r\n" + e.ToString());
                MessageBox.Show(e.Message, title);
            }
        }

        // 刷新
        public void RefreshGeneralUser(string condition)
        {
            tbSearch.Enabled = true;
            btSearch.Enabled = true;

            dataGridView2.Rows.Clear();
            //dataGridView2.BackgroundColor = Color.DarkGray;
            generalUsersList.Clear();

            manProgressLabel.Text = "正在刷新会员...";

            UserCountsLabel.Text = "正在刷新...";

            DBOpen();

            string SQLforGeneral = @"SELECT user_id, user_name, realname, nickname, degreeid, cyykey, reg_time, province, city, area, UserLisDay, mobile_phone, qq, UserLogined_day, paid From ecs_users " + condition + " ORDER BY user_name ASC";

            MySqlCommand cmd = new MySqlCommand(SQLforGeneral, sqlConnection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            int counts = 0;
            while (dataReader.Read())
            {
                generalUsersList.Add(new GeneralUsers
                {
                    id = dataReader["user_id"].ToString(),
                    user_name = dataReader["user_name"].ToString(),
                    realname = USER_DEGREE == 0 ? dataReader["realname"].ToString() : "********",
                    nickname = dataReader["nickname"].ToString(),
                    userDegree = dataReader["degreeid"].ToString(),
                    cyykey = dataReader["cyykey"].ToString(),
                    reg_time = ConvertTSToDateTime(int.Parse(dataReader["reg_time"].ToString())).ToString(),
                    mobile_phone = dataReader["mobile_phone"].ToString(),
                    qq = dataReader["qq"].ToString(),
                    lisExpireTime = dataReader["UserLisDay"].ToString(),
                    province = dataReader["province"].ToString(),
                    city = dataReader["city"].ToString(),
                    area = dataReader["area"].ToString(),
                    lastlogonTime = dataReader["UserLogined_day"].ToString(),
                    paid = int.Parse(dataReader["paid"].ToString()),
                });
                counts++;
            }

            dataReader.Close();
            DBClose();

            UserCountsLabel.Text = "总共会员：" + counts + "位";
            int id = 1;
            foreach (GeneralUsers gu in generalUsersList)
            {
                dataGridView2.Rows.Add(
                    int.Parse(gu.id),
                    gu.user_name,
                    (gu.nickname == null) ? "" : gu.nickname,
                    (gu.realname == null) ? "" : gu.realname,
                    GetDegreeNameByInt(int.Parse(gu.userDegree.ToString())),
                    gu.cyykey,
                    (gu.lisExpireTime == null) ? "" : gu.lisExpireTime,
                    gu.lastlogonTime,
                    gu.reg_time,
                    gu.province,
                    gu.city,
                    gu.area
                    );

                if (dataGridView2.Rows[id - 1].Cells[4].Value.ToString().Equals("高级会员"))
                {
                    dataGridView2.Rows[id - 1].Cells[4].Style.ForeColor = System.Drawing.Color.Red;
                }
                if (gu.paid == 0)
                {
                    dataGridView2.Rows[id - 1].Cells[6].Style.ForeColor = System.Drawing.Color.Red;
                }
                id++;
            }
            manProgressLabel.Text = "刷新会员成功!";

        }

        private string GetQueryByDegree()
        {
            string sql = "";
            if (USER_DEGREE == 0)
            {
                //sql = "WHERE degreeid = 0";
            }
            //province, city, area
            else if (USER_DEGREE > 2 && USER_DEGREE < 6)
            {
                switch (USER_DEGREE)
                {
                    case 3:
                        sql = "WHERE (degreeid = 1 OR degreeid = 2) AND province = '" + USER_PROVINCE + "'";
                        break;
                    case 4:
                        sql = "WHERE (degreeid = 1 OR degreeid = 2) AND city = '" + USER_CITY + "'";
                        break;
                    case 5:
                        sql = "WHERE (degreeid = 1 OR degreeid = 2) AND area = '" + USER_DISTRICT + "'";
                        break;
                }
            }
            else if (USER_DEGREE == 6)
            {
                sql = "WHERE cyykey='CYY" + BasicFeature.FormatID(USER_NAME, 7, "0") + "'";
            }
            return sql;
        }

        private string GetDegreeNameByInt(int num)
        {
            string degree = "";
            switch (num)
            {
                case 0:
                    degree = "超级管理";
                    break;
                case 1:
                    degree = "普通会员";
                    break;
                case 2:
                    degree = "高级会员";
                    break;
                case 3:
                    degree = "省级代理";
                    break;
                case 4:
                    degree = "市级代理";
                    break;
                case 5:
                    degree = "县级代理";
                    break;
                case 6:
                    degree = "兼职业务";
                    break;
            }
            return degree;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            manProgressLabel.Text = "正在刷新会员数据...";

            try
            {
                // 刷新数据
                RefreshGeneralUser(GetQueryByDegree());
            }
            catch (Exception ex)
            {
                RecordLog("刷新会员数据时出错\r\nTargetSite: " + ex.TargetSite + "\r\n" + ex.ToString());
                MessageBox.Show(ex.Message, "刷新会员数据时出错");
            }
            manProgressLabel.Text = "刷新成功";
            SoundPlay(System.Environment.CurrentDirectory + @"\config\complete.wav");
        }

        // 提交数据
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                DBOpen();
                manProgressLabel.Text = "正在提交数据...";

                List<GeneralUsers> gul = new List<GeneralUsers>();

                for (int i = 0; i < dataGridView2.RowCount; i++)
                {
                    gul.Add(new GeneralUsers()
                    {
                        user_name = dataGridView2.Rows[i].Cells[1].Value.ToString(),
                        nickname = dataGridView2.Rows[i].Cells[2].Value.ToString(),
                        userDegree = dataGridView2.Rows[i].Cells[3].Value.ToString(),
                        cyykey = dataGridView2.Rows[i].Cells[4].Value.ToString(),
                        lisExpireTime = dataGridView2.Rows[i].Cells[5].Value.ToString(),
                        lastlogonTime = dataGridView2.Rows[i].Cells[6].Value.ToString(),
                        reg_time = dataGridView2.Rows[i].Cells[7].Value.ToString(),
                        province = dataGridView2.Rows[i].Cells[8].Value.ToString(),
                        city = dataGridView2.Rows[i].Cells[9].Value.ToString(),
                        area = dataGridView2.Rows[i].Cells[10].Value.ToString(),
                        mobile_phone = dataGridView2.Rows[i].Cells[11].Value.ToString(),
                        qq = dataGridView2.Rows[i].Cells[12].Value.ToString(),
                    }
                    );
                }
                foreach (GeneralUsers gu in gul)
                {
                    string SQL = @"UPDATE ecs_users SET 
                        UserLisDay ='" + gu.lisExpireTime + @"'"
                                + @", degreeid = '" + ((gu.userDegree.Equals(manUserDegree.HeaderText.ToString())) ? @"1" : @"2") + @"'"
                                + @", province = '" + gu.province + @"'"
                                + @", city = '" + gu.city + @"'"
                                + @", area = '" + gu.area + @"'"
                                + @", mobile_phone = '" + gu.mobile_phone + @"'"
                                + @", qq = '" + gu.qq + @"'"
                                + @", nickname = '" + gu.nickname + @"'"
                                + @" WHERE user_name = " + @"'" + gu.user_name + @"'";

                    MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);
                    cmd.ExecuteNonQuery();
                }

                DBClose();

                manProgressLabel.Text = "数据提交成功";
                SoundPlay(System.Environment.CurrentDirectory + @"\config\complete.wav");

                //MessageBox.Show("恭喜您，数据提交成功", "消息");
                //dataGridView2.Refresh();
                //button3.PerformClick();
            }
            catch (Exception exx)
            {
                RecordLog("提交会员数据时出错\r\nTargetSite: " + exx.TargetSite + "\r\n" + exx.ToString());
                MessageBox.Show(exx.Message, "提交会员数据时出错");
            }
        }

        public void SoundPlay(string filename)
        {
            System.Media.SoundPlayer media = new System.Media.SoundPlayer(filename);
            media.Play();
        }

        DetailForm detailForm;
        public static Dictionary<string, string> generalUserDetailDic;

        private void getGeneralUsersDetail(string usernameString)
        {
            generalUserDetailDic = new Dictionary<string, string>();

            string SQLforGeneral = @"SELECT 
                                    user_id,
                                    user_name, 
                                    realname, 
                                    nickname, 
                                    sex,
                                    degreeid, 
                                    province, 
                                    city, 
                                    area, 
                                    UserLisDay, 
                                    mobile_phone, 
                                    qq, 
                                    UserLogined_day,
                                    reg_time,
                                    birthday,
                                    bankcard,
                                    bank,
                                    idcard,
                                    email,
                                    LogonMinutes,
                                    cyykey,
                                    beizhu,
                                    paid,
                                    LastPaidDate,
                                    address,
                                    race,
                                    DLquanxian,
                                    IsAuthedFlag,
                                    paidToDate,
                                    paidWayNId,
                                    UserLogined_ip, 
                                    UserLogined_Address,
                                    password 
                                    From ecs_users WHERE user_name ='" + usernameString + @"'";

            DBOpen();

            MySqlCommand cmd = new MySqlCommand(SQLforGeneral, sqlConnection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            while (dataReader.Read())
            {
                generalUserDetailDic.Add("user_id", dataReader["user_id"].ToString());
                generalUserDetailDic.Add("user_name", dataReader["user_name"].ToString());
                generalUserDetailDic.Add("realname", dataReader["realname"].ToString());
                generalUserDetailDic.Add("nickname", dataReader["nickname"].ToString());
                generalUserDetailDic.Add("sex", (int.Parse(dataReader["sex"].ToString()) == 1 ? "男" : "女")); //1为男 0为女
                generalUserDetailDic.Add("paid", int.Parse(dataReader["paid"].ToString()).ToString());
                generalUserDetailDic.Add("userDegree", GetDegreeNameByInt(int.Parse(dataReader["degreeid"].ToString())));
                generalUserDetailDic.Add("mobile_phone", dataReader["mobile_phone"].ToString());
                generalUserDetailDic.Add("qq", dataReader["qq"].ToString());
                generalUserDetailDic.Add("lisExpireTime", dataReader["UserLisDay"].ToString());
                generalUserDetailDic.Add("province", dataReader["province"].ToString());
                generalUserDetailDic.Add("city", dataReader["city"].ToString());
                generalUserDetailDic.Add("area", dataReader["area"].ToString());
                generalUserDetailDic.Add("lastlogonTime", dataReader["UserLogined_day"].ToString());
                generalUserDetailDic.Add("reg_time", dataReader["reg_time"].ToString());
                generalUserDetailDic.Add("birthday", dataReader["birthday"].ToString());
                generalUserDetailDic.Add("bankcard", dataReader["bankcard"].ToString());
                generalUserDetailDic.Add("idcard", dataReader["idcard"].ToString());
                generalUserDetailDic.Add("email", dataReader["email"].ToString());
                generalUserDetailDic.Add("LogonMinutes", dataReader["LogonMinutes"].ToString());
                generalUserDetailDic.Add("password", dataReader["password"].ToString());
                generalUserDetailDic.Add("cyykey", dataReader["cyykey"].ToString());
                generalUserDetailDic.Add("beizhu", dataReader["beizhu"].ToString());
                generalUserDetailDic.Add("LastPaidDate", dataReader["LastPaidDate"].ToString());
                generalUserDetailDic.Add("address", dataReader["address"].ToString());
                generalUserDetailDic.Add("race", dataReader["race"].ToString());
                generalUserDetailDic.Add("IsAuthedFlag", int.Parse(dataReader["IsAuthedFlag"].ToString()).ToString());
                generalUserDetailDic.Add("paidToDate", dataReader["paidToDate"].ToString());
                generalUserDetailDic.Add("paidWayNId", dataReader["paidWayNId"].ToString());
                generalUserDetailDic.Add("UserLogined_ip", dataReader["UserLogined_ip"].ToString());
                generalUserDetailDic.Add("UserLogined_Address", dataReader["UserLogined_Address"].ToString());
                generalUserDetailDic.Add("DLquanxian", dataReader["DLquanxian"].ToString());
                generalUserDetailDic.Add("bank", dataReader["bank"].ToString());
            }

            dataReader.Close();
            DBClose();

        }

        int dgv2CurrentRowIndex = -1;
        private void dataGridView2_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    dataGridView2.ClearSelection();
                    dataGridView2.Rows[e.RowIndex].Selected = true;
                    dataGridView2.CurrentCell = dataGridView2.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    dgv2CurrentRowIndex = e.RowIndex;
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        // copy
        protected void contextMenuStrip1_click_copy(object sender, EventArgs e)
        {
            string tmp = "=====会员管理详情=====\r\n";

            for (int i = 0; i < dataGridView2.Rows[dgv2CurrentRowIndex].Cells.Count; i++)
            {
                tmp += dataGridView2.Columns[i].HeaderText.ToString() + ": " + dataGridView2.Rows[dgv2CurrentRowIndex].Cells[i].Value.ToString() + "\r\n";
            }

            Clipboard.SetDataObject(tmp);
            MessageBox.Show("已经成功复制到剪贴板!", "提示");
        }

        // delete
        protected void contextMenuStrip1_click_delete(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否删除当前会员: " + dataGridView2.Rows[dgv2CurrentRowIndex].Cells[1].Value.ToString(), "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DeleteUserByUserName("user_name", dataGridView2.Rows[dgv2CurrentRowIndex].Cells[1].Value.ToString(), "ecs_users");
                DeleteUserByUserName("username", dataGridView2.Rows[dgv2CurrentRowIndex].Cells[1].Value.ToString(), "pw_members");
                manProgressLabel.Text = "删除当前会员成功!";
                //button3.PerformClick();
                RefreshGeneralUser(GetQueryByDegree());
            }
        }

        // 彩票11选5类型 浙江11选5
        public enum eCPType11_5
        {
            SD = 1, GD_11_5, JX_11_5, CQ_11_5, JS_11_5, ZJ_11_5, SH_11_5
        }

        // update 彩票数据
        private void updateCPdata(int number)
        {
            try
            {
                manProgressLabel.Text = "正在刷新彩票数据...";
                dataGridView3.Rows.Clear();
                cpInfoList.Clear();

                DBOpen();

                string SQL = @"SELECT CName, CType, CDay, CData From cyyCPData 
                            WHERE CName = " + 1
                            + @" AND CType = " + cpTypeInt
                            + @" ORDER BY CDAY DESC LIMIT " + number;

                MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    // 11选5
                    if (realCpNameInt == 1)
                    {
                        cpInfoList.Add(new CPinfo
                        {
                            cpName = dataReader["CName"].ToString(),
                            cpTpye = dataReader["CType"].ToString(),
                            cpDay = dataReader["CDay"].ToString(),
                            cpData = dataReader["CData"].ToString(),
                        });
                    }
                    // 11选3
                    else if (realCpNameInt == 2)
                    {
                        cpInfoList.Add(new CPinfo
                        {
                            cpName = dataReader["CName"].ToString(),
                            cpTpye = dataReader["CType"].ToString(),
                            cpDay = dataReader["CDay"].ToString(),
                            cpData = dataReader["CData"].ToString().Substring(0, 6),
                        });
                    }
                }

                dataReader.Close();
                DBClose();

                if (cpInfoList.Count == 0)
                {
                    MessageBox.Show("暂无该彩票信息");
                }
                else
                {
                    foreach (CPinfo gu in cpInfoList)
                    {
                        dataGridView3.Rows.Add(
                            cpNameString,
                            cpTypeString,
                            gu.cpDay,
                            gu.cpData
                            );
                    }
                    manProgressLabel.Text = "刷新成功";
                    SoundPlay(System.Environment.CurrentDirectory + @"\config\complete.wav");
                }
            }
            catch (Exception ex)
            {
                RecordLog("刷新错误，错误信息\r\nTargetSite: " + ex.TargetSite + "\r\n" + ex.ToString());
                MessageBox.Show("刷新错误，错误信息：" + ex.Message);
            }
        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            defineSelectedCPType(cpTpyeComboBox.SelectedItem.ToString());
            updateCPdata(int.Parse(cpNumberTextBox.Text.ToString()));
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                manProgressLabel.Text = "正在刷新数据...";
                dataGridView4.Rows.Clear();
                programWarningList.Clear();

                DBOpen();

                //string SQL = @"SELECT * FROM cyyShows ORDER BY id DESC LIMIT 1";
                string SQL = @"SELECT id, Text, shown FROM cyyShows";
                MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);

                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    programWarningList.Add(new ProgramWarning
                    {
                        pwID = dataReader["id"].ToString(),
                        pwWarningText = dataReader["Text"].ToString(),
                        pwShown = dataReader["shown"].ToString(),
                    });
                }

                dataReader.Close();
                DBClose();

                if (programWarningList.Count == 0)
                {
                    MessageBox.Show("暂无信息");
                }
                else
                {
                    button7.Enabled = true;

                    foreach (ProgramWarning pw in programWarningList)
                    {
                        dataGridView4.Rows.Add(
                            pw.pwWarningText,
                            (int.Parse(pw.pwShown) == 0) ? "设为当前显示" : "取 消"
                            );
                    }
                    manProgressLabel.Text = "刷新成功";
                    SoundPlay(System.Environment.CurrentDirectory + @"\config\complete.wav");
                }
            }
            catch (Exception ex)
            {
                RecordLog("刷新错误，错误信息： TargetSite: " + ex.TargetSite + "\r\n" + ex.ToString());
                MessageBox.Show("刷新错误，错误信息：" + ex.Message);
            }
        }

        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dataGridView1.Rows[e.RowIndex].Selected = true;
        }

        private void clearOnlineUsersButton_Click(object sender, EventArgs e)
        {
            DialogResult resault = MessageBox.Show("是否确认清空在线会员信息？", "警告", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (resault == DialogResult.OK)
            {
                clearOnlineUsersInfo();
                //this.Close();
            }
        }

        private void clearOnlineUsersInfo()
        {
            try
            {
                DBOpen();
                string SQL = @"DELETE FROM cyy_OnlinesUsers";

                MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);
                cmd.ExecuteNonQuery();

                DBClose();

                manProgressLabel.Text = "在线会员信息清空成功!";
                SoundPlay(System.Environment.CurrentDirectory + @"\config\clearSucceed.wav");
            }
            catch (Exception eee)
            {
                RecordLog("清除时报错： TargetSite: " + eee.TargetSite + "\r\n" + eee.ToString());
                MessageBox.Show(eee.Message, "清除时报错!");
            }

        }

        private void clearDuplicateUserButton_Click(object sender, EventArgs e)
        {
            updateOnlineUserInfo(65);
            // clearSucceed.wav
            manProgressLabel.Text = "清空成功!";
            SoundPlay(System.Environment.CurrentDirectory + @"\config\clearSucceed.wav");
        }

        int dgv1CurrentRowIndex = -1;
        private void dataGridView1_CellMouseDown_1(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[e.RowIndex].Selected = true;
                    dataGridView1.CurrentCell = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    dgv1CurrentRowIndex = e.RowIndex;
                    contextMenuStrip2.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        // copy
        protected void contextMenuStrip2_click_copy(object sender, EventArgs e)
        {
            string tmp = "=====实时在线会员=====\r\n";

            for (int i = 0; i < dataGridView1.Rows[dgv1CurrentRowIndex].Cells.Count; i++)
            {
                tmp += dataGridView1.Columns[i].HeaderText.ToString() + ": " + dataGridView1.Rows[dgv1CurrentRowIndex].Cells[i].Value.ToString() + "\r\n";
            }

            Clipboard.SetDataObject(tmp);
            MessageBox.Show("已经成功复制到剪贴板!", "提示");
        }

        // delete
        protected void contextMenuStrip2_click_delete(object sender, EventArgs e)
        {
            if (MessageBox.Show("是否删除当前在线会员: " + dataGridView1.Rows[dgv1CurrentRowIndex].Cells[1].Value.ToString(), "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                DeleteUserByUserName("user_name", dataGridView1.Rows[dgv1CurrentRowIndex].Cells[0].Value.ToString(), "cyy_OnlinesUsers");
                manProgressLabel.Text = "删除当前会员成功!";
            }
        }

        private void DeleteUserByUserName(string userNameTitle, string userName, string tableString)
        {
            try
            {
                //cyy_OnlinesUsers
                //ecs_users
                DBOpen();
                string SQL = @"DELETE FROM " + tableString + @" WHERE " + userNameTitle + " = '" + userName + @"'";

                MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);
                cmd.ExecuteNonQuery();

                DBClose();

                // 更新数据
                getOnlineUserInfo(GetQueryByDegree());

                SoundPlay(System.Environment.CurrentDirectory + @"\config\clearSucceed.wav");
            }
            catch (Exception e)
            {
                RecordLog("删除在线会员时错误\r\nTargetSite: " + e.TargetSite + "\r\n" + e.ToString());
                MessageBox.Show(e.Message, "删除在线会员时错误");
            }
        }

        // 修改会员
        public void UpdateUserDetailedInfo(bool isNeedTosetPsw)
        {
            if (isNeedTosetPsw)
            {
                MD5 md5Hash = MD5.Create();
                generalUserDetailDic["password"] = GetMd5Hash(md5Hash, generalUserDetailDic["password"]).ToLower();
            }

            string SQLecs_users = @"UPDATE ecs_users SET 
                                    nickname = '" + generalUserDetailDic["nickname"] +
                                   @"', user_name = '" + generalUserDetailDic["user_name"] +
                                   @"', realname = '" + generalUserDetailDic["realname"] +
                                   @"', sex = '" + (generalUserDetailDic["sex"].Equals("男") ? 1 : 0) +
                                   @"', paid = " + int.Parse(generalUserDetailDic["paid"]) +
                                   @",  reg_time = '" + generalUserDetailDic["reg_time"] +
                                   @"', birthday = '" + generalUserDetailDic["birthday"] +
                                   @"', idcard = '" + generalUserDetailDic["idcard"] +
                                   @"', bankcard = '" + generalUserDetailDic["bankcard"] +
                                   @"', degreeid = '" + generalUserDetailDic["userDegree"] +
                                   @"', email = '" + generalUserDetailDic["email"] +
                                   @"', qq = '" + generalUserDetailDic["qq"] +
                                   @"', mobile_phone = '" + generalUserDetailDic["mobile_phone"] +
                                   @"', UserLisDay = '" + generalUserDetailDic["lisExpireTime"] +
                                   @"', province = '" + generalUserDetailDic["province"] +
                                   @"', city = '" + generalUserDetailDic["city"] +
                                   @"', area = '" + generalUserDetailDic["area"] +
                                   @"', bank = '" + generalUserDetailDic["bank"] +
                                   @"', cyykey = '" + generalUserDetailDic["cyykey"] +
                                   @"', beizhu = '" + generalUserDetailDic["beizhu"] +
                                   @"', LastPaidDate = '" + generalUserDetailDic["LastPaidDate"] +
                                   @"', address = '" + generalUserDetailDic["address"] +
                                   @"', race = '" + generalUserDetailDic["race"] +
                                   @"', IsAuthedFlag = " + int.Parse(generalUserDetailDic["IsAuthedFlag"]) +
                                   @",  paidToDate = '" + generalUserDetailDic["paidToDate"] +
                                   @"', paidWayNId = '" + generalUserDetailDic["paidWayNId"] +
                                   @"', DLquanxian = '" + generalUserDetailDic["DLquanxian"] +
                                   @"', password = '" + generalUserDetailDic["password"] +
                                   @"' WHERE user_id = " + @"'" + generalUserDetailDic["user_id"].ToString() + @"';";

            string SQLcyyOnlines = @"UPDATE cyy_OnlinesUsers SET 
                                    nickname = '" + generalUserDetailDic["nickname"] +
                                    @"', user_name = '" + generalUserDetailDic["user_name"].ToString() +
                                   @"' WHERE user_name = " + @"'" + generalUserDetailDic["oldUserName"].ToString() + @"';";

            string SQLpw_members = @"UPDATE pw_members SET 
                                    username = '" + generalUserDetailDic["user_name"] +
                                    @"', password = '" + generalUserDetailDic["password"] +
                                   @"' WHERE username = " + @"'" + generalUserDetailDic["oldUserName"].ToString() + @"';";

            //MySqlCommand SQLecs_usersCmd = new MySqlCommand(SQLecs_users, sqlConnection);
            //MySqlCommand SQLcyyOnlinesCmd = new MySqlCommand(SQLcyyOnlines, sqlConnection);

            MySqlCommand SQLCmd = new MySqlCommand(SQLecs_users + "\r\n" + SQLcyyOnlines + "\r\n" + SQLpw_members, sqlConnection);

            try
            {
                DBOpen();

                //SQLecs_usersCmd.ExecuteNonQuery();
                //SQLcyyOnlinesCmd.ExecuteNonQuery();
                SQLCmd.ExecuteNonQuery();

                DBClose();

                SoundPlay(System.Environment.CurrentDirectory + @"\config\clearSucceed.wav");
                if (isNeedTosetPsw)
                {
                    MessageBox.Show("修改会员信息和密码成功", "提示信息");
                }
                else
                {
                    MessageBox.Show("修改会员信息成功", "提示信息");
                }
            }
            catch (Exception e)
            {
                //throw;
                RecordLog("修改会员信息出错\r\nTargetSite: " + e.TargetSite + "\r\n" + e.ToString());
                MessageBox.Show(e.Message, "修改会员信息出错");
            }
        }

        // 添加新会员
        public void CreateNewUserDetailed(Dictionary<string, string> newUserDic)
        {
            MD5 md5Hash = MD5.Create();
            newUserDic["password"] = GetMd5Hash(md5Hash, newUserDic["password"]).ToLower();

            string SQLecs_users = @"INSERT INTO ecs_users(
                                        nickname,
                                        realname,
                                        birthday,
                                        reg_time,
                                        user_name,
                                        sex,
                                        paid,
                                        idcard,
                                        degreeid,
                                        bankcard,
                                        email,
                                        qq,
                                        mobile_phone,
                                        UserLisDay,
                                        province,
                                        city,
                                        cyykey,
                                        beizhu,
                                        area,
                                        password)
                                        VALUES('"
                                        + newUserDic["nickname"] +
                                   @"', '" + newUserDic["realname"] +
                                   @"', '" + newUserDic["birthday"] +
                                   @"', '" + newUserDic["reg_time"] +
                                   @"', '" + newUserDic["user_name"] +
                                   @"', '" + (newUserDic["sex"].Equals("男") ? 1 : 0) +
                                   @"', '" + (newUserDic["paid"].Equals("已结算") ? 1 : 0) +
                                   @"', '" + newUserDic["idcard"] +
                                   @"', '" + newUserDic["userDegree"] +
                                   @"', '" + newUserDic["bankcard"] +
                                   @"', '" + newUserDic["email"] +
                                   @"', '" + newUserDic["qq"] +
                                   @"', '" + newUserDic["mobile_phone"] +
                                   @"', '" + newUserDic["lisExpireTime"] +
                                   @"', '" + newUserDic["province"] +
                                   @"', '" + newUserDic["city"] +
                                   @"', '" + newUserDic["cyykey"] +
                                   @"', '" + newUserDic["beizhu"] +
                                   @"', '" + newUserDic["area"] +
                                   @"', '" + newUserDic["password"] +
                                   @"');";

            string SQLpw_members = @"INSERT INTO pw_members (username, password) VALUES('" + newUserDic["user_name"] + "','" + newUserDic["password"] + "');";

            MySqlCommand SQLecs_usersCmd = new MySqlCommand(SQLecs_users + "\r\n" + SQLpw_members, sqlConnection);
            try
            {
                DBOpen();
                //SQLecs_usersCmd.CommandTimeout = 1200;

                SQLecs_usersCmd.ExecuteNonQuery();
                DBClose();
                SoundPlay(System.Environment.CurrentDirectory + @"\config\clearSucceed.wav");
                MessageBox.Show("添加会员成功", "提示");
            }
            catch (Exception e)
            {
                //throw;
                RecordLog("添加会员时出错\r\nTargetSite: " + e.TargetSite + "\r\n" + e.ToString());
                MessageBox.Show(e.Message, "添加会员时出错");
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            UpdateUserOnlineInfoTimer.Enabled = false;
            if (e.RowIndex != -1)
            {
                getGeneralUsersDetail(this.dataGridView1.CurrentRow.Cells["userName"].Value.ToString());

                detailForm = new DetailForm(false);
                try
                {
                    detailForm.ShowDialog();
                    UpdateUserOnlineInfoTimer.Enabled = true;
                }
                catch (Exception exxxx)
                {
                    RecordLog("详细信息载入错误\r\nTargetSite: " + exxxx.TargetSite + "\r\n" + exxxx.ToString());
                    MessageBox.Show("详细信息载入错误: " + exxxx.Message);
                }
            }
        }

        private void dataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            UpdateUserOnlineInfoTimer.Enabled = false;
            isModified = false;
            if (e.RowIndex != -1)
            {
                getGeneralUsersDetail(this.dataGridView2.CurrentRow.Cells["manUserName"].Value.ToString());
                detailForm = new DetailForm(false);
                try
                {
                    detailForm.ShowDialog();
                    //button3.PerformClick();
                    UpdateUserOnlineInfoTimer.Enabled = true;
                    if (isModified)
                    {
                        //RefreshGeneralUser(GetQueryByDegree());
                        cbUserTypeSelector.SelectedIndex = 0;
                        manProgressLabel.Text = "修改会员数据成功!";
                        dataGridView2.ClearSelection();
                        dataGridView2.Rows[e.RowIndex].Selected = true;
                        dataGridView2.FirstDisplayedScrollingRowIndex = dataGridView2.Rows[e.RowIndex].Index;
                        if (USER_DEGREE == 0)
                        {
                            cbUserTypeSelector.SelectedIndex = 0;
                        }
                        else
                        {
                            button3.PerformClick();
                        }
                    }
                }
                catch (Exception exxxx)
                {
                    cbUserTypeSelector.SelectedIndex = 0;
                    RecordLog("详细信息载入错误\r\nTargetSite: " + exxxx.TargetSite + "\r\n" + exxxx.ToString());
                    MessageBox.Show("详细信息载入错误: " + exxxx.Message);
                }
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            UpdateUserOnlineInfoTimer.Enabled = false;
            isModified = false;
            latestIDFromDataBase();

            detailForm = new DetailForm(true);
            try
            {
                detailForm.ShowDialog();
                //button3.PerformClick();
                UpdateUserOnlineInfoTimer.Enabled = true;
                if (isModified)
                {
                    //RefreshGeneralUser(GetQueryByDegree());
                    cbUserTypeSelector.SelectedIndex = 0;

                    manProgressLabel.Text = "添加新会员成功!";
                    if (USER_DEGREE == 0)
                    {
                        cbUserTypeSelector.SelectedIndex = 0;
                    }
                    else
                    {
                        button3.PerformClick();
                    }
                }
            }
            catch (Exception exxxx)
            {
                cbUserTypeSelector.SelectedIndex = 0;

                RecordLog("添加新会员错误\r\nTargetSite: " + exxxx.TargetSite + "\r\n" + exxxx.ToString());
                MessageBox.Show("添加新会员错误: " + exxxx.Message);
            }
        }

        public static string LATEST_ID = "";
        private void latestIDFromDataBase()
        {
            DBOpen();
            string SQL = "select max(user_id)  from ecs_users";//建表语句  

            MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);
            MySqlDataReader dataReader = cmd.ExecuteReader();

            dataReader.Read();

            if (dataReader["max(user_id)"].ToString().Equals(""))
            {
                LATEST_ID = "0";
            }
            else
            {
                LATEST_ID = dataReader["max(user_id)"].ToString();
            }

            dataReader.Close();
            DBClose();

        }

        public System.DateTime ConvertTSToDateTime(double timestamp)
        {
            //create a new datetime value based on the unix epoch
            System.DateTime converted = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            //add the timestamp to the value
            System.DateTime newdatetime = converted.AddSeconds(timestamp);
            //return the value in string format
            return newdatetime.ToLocalTime();
        }

        public double ConvertDTToTimestamp(System.DateTime value)
        {
            //create timespan by subtracting the value provided from
            //the unix epoch
            System.TimeSpan span = (value - new System.DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());
            //return the total seconds (which is a unix timestamp)
            return (double)span.TotalSeconds;
        }

        private int restartTime;
        private void lanuchRestartService()
        {
            restartTime = 6; // 5秒
            RestartServiceTimer.Enabled = true;
        }

        private void RestartServiceTimer_Tick(object sender, EventArgs e)
        {
            /*
            if (isWindowLoaded)
            {
                RestartServiceTimer.Enabled = false;
                DBOpen();
            }
            else
            {
             */
            this.Text = "服务已关闭 将于[" + (restartTime - 1) + "秒]后重启";
            restartTime--;
            if (restartTime == 0)
            {
                this.Text = "彩盈盈管理系统";
                RestartServiceTimer.Enabled = false;

                StartService();
                DBOpen();
            }
            //}
        }

        // Search 

        private Color hightlightColor = Color.Yellow;
        private void btSearch_Click(object sender, EventArgs e)
        {
            string searchString = tbSearch.Text;
            int tempCount = 0;

            manProgressLabel.Text = "";

            RefreshGeneralUser(GetQueryByDegree());

            if (!searchString.Equals(""))
            {
                for (int i = 0; i < dataGridView2.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView2.Columns.Count; j++)
                    {
                        string temp = dataGridView2.Rows[i].Cells[j].Value.ToString();
                        if (temp.Contains(searchString))
                        {
                            for (int m = 0; m < dataGridView2.Rows[i].Cells.Count; m++)
                            {
                                dataGridView2.Rows[i].Cells[m].Style.BackColor = hightlightColor;
                            }

                            if (tempCount < 1)
                            {
                                dataGridView2.FirstDisplayedScrollingRowIndex = dataGridView2.Rows[i].Index;
                            }
                            tempCount++;
                        }
                        continue;
                    }
                }

            }
            manProgressLabel.Text = "查询成功! 共" + calculateSearchCount() + "条记录匹配!";
        }

        private int calculateSearchCount()
        {
            int count = 0;
            for (int i = 0; i < dataGridView2.Rows.Count; i++)
            {
                if (dataGridView2.Rows[i].Cells[0].Style.BackColor == hightlightColor)
                {
                    count++;
                }

            }
            return count;
        }

        private void tbSearch_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
            {
                btSearch.PerformClick();
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                if (dataGridView2.SelectedRows.Count > 1)
                {
                    manProgressLabel.Text = "已选中 " + dataGridView2.SelectedRows.Count + "条数据";
                }
                else if (dataGridView2.SelectedRows.Count == 1)
                {
                    manProgressLabel.Text = "已选中[" + dataGridView2.Rows[e.RowIndex].Cells[4].Value.ToString() + " " + dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString() + "]";
                }
            }
        }


        public void RecordLog(string log)
        {
            string fileName = System.Environment.CurrentDirectory + @"\config\log.txt";
            string fileString = "";
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fszz = System.IO.File.OpenRead(fileName))
                {
                    byte[] bytes = new byte[fszz.Length];
                    fszz.Read(bytes, 0, bytes.Length);

                    fileString = Encoding.UTF8.GetString(bytes);
                }
            }
            using (System.IO.FileStream fs = System.IO.File.Create(fileName))
            {
                StringBuilder sb = new StringBuilder();
                byte[] info = new UTF8Encoding().GetBytes(DateTime.Now + "   " + log + "\r\n" + fileString);
                fs.Write(info, 0, info.Length);
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

        #region 上传
        public void UpLoadFile(string fileNamePath, string fileName, string uriString)
        {
            // System.Environment.CurrentDirectory + @"\data\data.db"
            // ftp://qyw28051:cyy2014@qyw28051.my3w.com/data.db
            System.Net.WebClient webClient = new System.Net.WebClient();
            try
            {
                webClient.UploadFile(uriString, fileNamePath);
                MessageBox.Show(fileName, "上传文件成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "上传文件出现异常");
            }
        }
        #endregion

        private void button4_Click_1(object sender, EventArgs e)
        {
            label2.Text = "精华版当前最新版本: " + getOnlineFile(@"http://www.caiyingying.com/products/cyy/full/Version.txt");
            textBox2.Text = getOnlineFile(@"http://www.caiyingying.com/products/cyy/full/updateLog.txt");

            label3.Text = "基础版当前最新版本: " + getOnlineFile(@"http://www.caiyingying.com/products/cyy/free/Version.txt");
        }

        public bool isAdvanced = false;
        private void button9_Click(object sender, EventArgs e)
        {
            //初始化一个OpenFileDialog类
            OpenFileDialog fileDialog = new OpenFileDialog();
            //判断会员是否正确的选择了文件
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                //获取会员选择文件的后缀名
                string extension = Path.GetExtension(fileDialog.FileName);
                //声明允许的后缀名
                string[] str = new string[] { ".exe" };
                if (!str.Contains(extension))
                {
                    MessageBox.Show("仅能上传.exe格式的文件！");
                }
                else
                {
                    manProgressLabel.Text = "正在上传文件...";
                    UpLoadFile(fileDialog.FileName, fileDialog.SafeFileName, "ftp://qyw28051:cyy2014@qyw28051.my3w.com/products/cyy/full/" + fileDialog.SafeFileName);
                    manProgressLabel.Text = "上传文件...";
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show("KeyDown"+e.KeyCode);
            // 组合键
            if (e.KeyCode == Keys.K && e.Modifiers == Keys.Control)         //Ctrl+F1
            {
                if (!isAdvanced)
                {
                    isAdvanced = true;
                    button9.Visible = true;
                }
                else
                {
                    isAdvanced = false;
                    button9.Visible = false;
                }
            }
        }

        private void button31_Click(object sender, EventArgs e)
        {
            string body = "以下会员请予以限制登录使用彩盈盈软件，若本代理人提交的限制理由有误，愿意承担一切责任。"
                            + "<table border=\"1\">"
                            + "<tr>"
                            + "<td>" + tbXZA0.Text + "</td>"
                            + "<td>" + tbXZA1.Text + "</td>"
                            + "<td>" + tbXZA2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZB0.Text + "</td>"
                            + "<td>" + tbXZB1.Text + "</td>"
                            + "<td>" + tbXZB2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZC0.Text + "</td>"
                            + "<td>" + tbXZC1.Text + "</td>"
                            + "<td>" + tbXZC2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZD0.Text + "</td>"
                            + "<td>" + tbXZD1.Text + "</td>"
                            + "<td>" + tbXZD2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZE0.Text + "</td>"
                            + "<td>" + tbXZE1.Text + "</td>"
                            + "<td>" + tbXZE2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZF0.Text + "</td>"
                            + "<td>" + tbXZF1.Text + "</td>"
                            + "<td>" + tbXZF2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZG0.Text + "</td>"
                            + "<td>" + tbXZG1.Text + "</td>"
                            + "<td>" + tbXZG2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZH0.Text + "</td>"
                            + "<td>" + tbXZH1.Text + "</td>"
                            + "<td>" + tbXZH2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZI0.Text + "</td>"
                            + "<td>" + tbXZI1.Text + "</td>"
                            + "<td>" + tbXZI2.Text + "</td>"
                            + "</tr>"
                            + "<tr>"
                            + "<td>" + tbXZJ0.Text + "</td>"
                            + "<td>" + tbXZJ1.Text + "</td>"
                            + "<td>" + tbXZJ2.Text + "</td>"
                            + "</tr>"
                            + "</table>";

            SoundPlay(System.Environment.CurrentDirectory + @"\config\complete.wav");
            MessageBox.Show(BasicFeature.SendMail("彩盈盈代理商 " + Form1.USER_ALAIS + "[" + Form1.USER_NAME + "] - 限制用户登录申请单", "", body, true), "提示");
            ClearDistrictUsers();
        }

        private void ClearDistrictUsers()
        {
            tbXZA0.Text = "";
            tbXZA1.Text = "";
            tbXZA2.SelectedIndex = -1;
            tbXZB0.Text = "";
            tbXZB1.Text = "";
            tbXZB2.SelectedIndex = -1;
            tbXZC0.Text = "";
            tbXZC1.Text = "";
            tbXZC2.SelectedIndex = -1;
            tbXZD0.Text = "";
            tbXZD1.Text = "";
            tbXZD2.SelectedIndex = -1;
            tbXZE0.Text = "";
            tbXZE1.Text = "";
            tbXZE2.SelectedIndex = -1;
            tbXZF0.Text = "";
            tbXZF1.Text = "";
            tbXZF2.SelectedIndex = -1;
            tbXZG0.Text = "";
            tbXZG1.Text = "";
            tbXZG2.SelectedIndex = -1;
            tbXZH0.Text = "";
            tbXZH1.Text = "";
            tbXZH2.SelectedIndex = -1;
            tbXZI0.Text = "";
            tbXZI1.Text = "";
            tbXZI2.SelectedIndex = -1;
            tbXZJ0.Text = "";
            tbXZJ1.Text = "";
            tbXZJ2.SelectedIndex = -1;
        }

        private void cbUserTypeSelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (cbUserTypeSelector.SelectedIndex)
            {
                case 0:
                    RefreshGeneralUser("");
                    break;
                case 1:
                    RefreshGeneralUser("WHERE degreeid > 0 AND degreeid < 3");
                    break;
                case 2:
                    RefreshGeneralUser("WHERE degreeid < 1 OR degreeid > 2");
                    break;
            }
        }

        private void GetCurrentCPDataButton_Click(object sender, EventArgs e)
        {
            UpdateCPOnlineData("http://f.opencai.net/sd11x5-1.json", 1, "山东");
            UpdateCPOnlineData("http://f.opencai.net/gd11x5-1.json", 2, "广东");
            UpdateCPOnlineData("http://f.opencai.net/jx11x5-1.json", 3, "江西");
            UpdateCPOnlineData("http://f.opencai.net/cq11x5-1.json", 4, "重庆");
        }

        private void GetCorrectCPDataButton_Click(object sender, EventArgs e)
        {
            UpdateCPOnlineData("http://f.opencai.net/sd11x5-50.json", 1, "山东");
            UpdateCPOnlineData("http://f.opencai.net/gd11x5-50.json", 2, "广东");
            UpdateCPOnlineData("http://f.opencai.net/jx11x5-50.json", 3, "江西");
            UpdateCPOnlineData("http://f.opencai.net/cq11x5-50.json", 4, "重庆");
        }
        // 彩票数据
        /*
         山东1
         http://f.opencai.net/sd11x5-1.json
         广东2
         http://f.opencai.net/gd11x5-1.json
         江西3
         http://f.opencai.net/jx11x5-1.json
         重庆4
         http://f.opencai.net/cq11x5-1.json
         */
        private void UpdateCPOnlineData(string link, int cpType, string cpName)
        {
            try
            {
                string html = api.HttpGet(link, Encoding.GetEncoding("gb2312"));
                JsonData json = JsonMapper.ToObject(html);
                foreach (JsonData row in json["data"])
                {
                    UpdateCPOnlineDataSQL(row, cpType, cpName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("采集出现错误：" + ex.Message);
            }
        }

        private void UpdateCPOnlineDataSQL(JsonData joData, int cpType, string cpName)
        {
            DBOpen();
            string cpday = joData["expect"].ToString().Substring(2, 8);
            string cpdata = joData["opencode"].ToString().Replace(",", "");
            string opentime = joData["opentime"].ToString();
            string SQL = @"INSERT INTO cyycpdata (CDay, CType, CData, AddDate, InputMan, CName, OpenTime) SELECT " + "'"
                + cpday + "',"
                + cpType + ",'"
                + cpdata + "', NOW(),'Server'," + "'"
                + cpName + "', '"
                + opentime + "' FROM DUAL WHERE NOT EXISTS (SELECT * FROM cyycpdata WHERE CDay = '"
                + cpday + "' AND CType = "
                + cpType + ");";
            MySqlCommand cmd = new MySqlCommand(SQL, sqlConnection);
            cmd.ExecuteNonQuery();
            DBClose();
        }

    }
}

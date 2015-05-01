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
using System.Security.Cryptography;

using CyyService.Model;
using CyyService.Service;

namespace CyyService
{
    public partial class DetailForm : Form
    {
        Dictionary<string, string> tempDic = new Dictionary<string, string>();

        public DetailForm()
        {
            InitializeComponent();
        }

        private string currentUserPassword = "";
        private string currentUserID = "";
        private string oldUserName = "";

        #region 三联地区联动

        //private string LOCATION_FILE_NAME = System.Environment.CurrentDirectory + @"\config\location.xml";
        private const string LOCATION_FILE_NAME = @"config\location.xml";

        /// <summary>
        /// 主地址数据.
        /// </summary>
        private Address mainAddressData = null;

        /// <summary>
        /// 当前省.
        /// </summary>
        private Province currentProvince = null;

        /// <summary>
        /// 当前市.
        /// </summary>
        private City currentCity = null;

        /// <summary>
        /// 偷懒用临时列表.
        /// </summary>
        private ComboBox[] tmpComboBoxArray = new ComboBox[3];
        #endregion

        private void DetailForm_Load(object sender, EventArgs e)
        {

        }

        public DetailForm(bool isAdd)
        {
            InitializeComponent();

            // 三联地区联动
            tmpComboBoxArray[0] = tbProvince;
            tmpComboBoxArray[1] = tbCity;
            tmpComboBoxArray[2] = tbDistrict;

            foreach (ComboBox cbo in tmpComboBoxArray)
            {
                cbo.ValueMember = "name";
                cbo.DisplayMember = "name";
            }

            // 三联地区联动
            AddressReader service = new AddressReader();
            // 读取文件.
            mainAddressData = service.ReadFromXmlFile(LOCATION_FILE_NAME);
            // 省   数据绑定.
            this.tbProvince.DataSource = mainAddressData.ProvinceList;

            if (!isAdd) //修改
            {
                this.ActiveControl = tbName;
                FuctionButton.Text = "提交修改";
                btMoreFunctions.Enabled = true;

                string labelString;
                Form1.generalUserDetailDic.TryGetValue("user_id", out labelString);
                currentUserID = labelString;

                Form1.generalUserDetailDic.TryGetValue("user_name", out labelString);
                tbUserID.Text = labelString;
                oldUserName = labelString;

                Form1.generalUserDetailDic.TryGetValue("realname", out labelString);
                tbName.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("nickname", out labelString);
                tbUsername.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("reg_time", out labelString);
                dtpRegisterDay.Text = Form1.GetInstence().ConvertTSToDateTime(int.Parse(labelString == "" ? "0" : labelString)).ToString();

                Form1.generalUserDetailDic.TryGetValue("birthday", out labelString);
                //dtpBirthday.Text = Form1.GetInstence().ConvertTSToDateTime(int.Parse(labelString)).ToString();
                dtpBirthday.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("userDegree", out labelString);
                cbUserdegree.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("idcard", out labelString);
                tbID.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("bankcard", out labelString);
                tbAccount.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("email", out labelString);
                tbEmail.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("qq", out labelString);
                tbQQ.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("mobile_phone", out labelString);
                tbMobile.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("lisExpireTime", out labelString);
                tbUserExpire.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("sex", out labelString);
                cbSex.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("lastlogonTime", out labelString);
                lbLastLogonTimeLabel.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("province", out labelString);
                tbProvince.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("city", out labelString);
                tbCity.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("area", out labelString);
                tbDistrict.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("bank", out labelString);
                tbBankName.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("cyykey", out labelString);
                tbLYCode.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("beizhu", out labelString);
                tbBeizhu.Text = labelString.Equals("") ? "无" : labelString;

                Form1.generalUserDetailDic.TryGetValue("paid", out labelString);
                cbPaid.SelectedIndex = int.Parse(labelString);

                Form1.generalUserDetailDic.TryGetValue("LogonMinutes", out labelString);
                lbLogonMinutes.Text = (labelString.Equals("0") ? "无" : calMintoTime(int.Parse(labelString)));

                Form1.generalUserDetailDic.TryGetValue("LastPaidDate", out labelString);
                tbLastPayDate.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("address", out labelString);
                tbLocation.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("race", out labelString);
                tbMZ.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("IsAuthedFlag", out labelString);
                cbRegistery.SelectedIndex = int.Parse(labelString);

                Form1.generalUserDetailDic.TryGetValue("paidToDate", out labelString);
                dtpJSDate.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("paidWayNId", out labelString);
                tbJSIDNumber.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("password", out labelString);
                tbPassword.Text = labelString;
                currentUserPassword = labelString;

                // tbLastLoginLocation
                Form1.generalUserDetailDic.TryGetValue("UserLogined_Address", out labelString);
                tbLastLoginLocation.Text = labelString;

                Form1.generalUserDetailDic.TryGetValue("UserLogined_ip", out labelString);
                tbLastLoginIP.Text = labelString;

                // DLquanxian
                Form1.generalUserDetailDic.TryGetValue("DLquanxian", out labelString);
                cbViewUser.Checked = (int.Parse(labelString.Substring(0, 1)) == 0) ? false : true;
                cbAddUser.Checked = (int.Parse(labelString.Substring(1, 1)) == 0) ? false : true;
                cbBanUser.Checked = (int.Parse(labelString.Substring(2, 1)) == 0) ? false : true;

                //password
                tbPassword.Text = "";
            }
            else
            {
                this.ActiveControl = tbUsername;
                btMoreFunctions.Enabled = false;

                cbSex.SelectedIndex = 0;
                cbPaid.SelectedIndex = 0;
                cbUserdegree.SelectedIndex = 2;
                cbRegistery.SelectedIndex = 0;

                dtpRegisterDay.Value = DateTime.Now;
                dtpRegisterDay.Enabled = false;

                tbUserExpire.Value = new DateTime(DateTime.Now.Year + 1, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                //tbUserID.ReadOnly = false;

                tbLastPayDate.Value = DateTime.Now;
                dtpJSDate.Value = DateTime.Now;

                //tbLastPayDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
                //dtpJSDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day - 1, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);

                // lbLogonMinutes.Text = "0 分钟";
                FuctionButton.Text = "添加用户";
            }

            // 权限设置
            SetDengji(isAdd);
        }

        private void SetDengji(bool isAdding)
        {
            if ((Form1.USER_DEGREE != 1 || Form1.USER_DEGREE != 2) && Form1.USER_DEGREE != 0)
            {
                grpQX.Visible = false;
                tbUserID.Enabled = false;
                cbUserdegree.Enabled = false;
                cbRegistery.Enabled = false;
                tbLastLoginLocation.Enabled = false;
                tbLastLoginIP.Enabled = false;
                tbCurrentLoginLocation.Enabled = false;
                tbCurrentLoginIP.Enabled = false;
                cbPaid.Enabled = false;
                dtpJSDate.Enabled = false;
                tbJSIDNumber.Enabled = false;
                tbBeizhu.Enabled = false;


                if (isAdding)
                {
                    this.Text = "添加高级会员";

                    if (Form1.USER_DEGREE == 3)
                    {
                        tbProvince.Enabled = false;
                        tbProvince.Text = Form1.USER_PROVINCE;
                        tbCity.Text = "城市";
                    }
                    else if (Form1.USER_DEGREE == 4)
                    {
                        tbProvince.Enabled = false;
                        tbCity.Enabled = false;
                        tbProvince.Text = Form1.USER_PROVINCE;
                        tbCity.Text = Form1.USER_CITY;
                    }
                    else if (Form1.USER_DEGREE == 5)
                    {
                        tbProvince.Enabled = false;
                        tbCity.Enabled = false;
                        tbDistrict.Enabled = false;
                        tbProvince.Text = Form1.USER_PROVINCE;
                        tbCity.Text = Form1.USER_CITY;
                        tbDistrict.Text = Form1.USER_DISTRICT;
                    }
                    else if (Form1.USER_DEGREE == 6)
                    {
                        tbLYCode.Enabled = false;
                        tbLYCode.Text = "CYY" + BasicFeature.FormatID(Form1.USER_NAME, 7, "0");
                    }

                    tbUserID.Text = "666" + BasicFeature.FormatID((int.Parse(Form1.LATEST_ID) + 1).ToString(), 4, "0");
                    lbPaid.Visible = false;
                    cbPaid.Visible = false;
                    lbJSDate.Visible = false;
                    dtpJSDate.Visible = false;
                }
                else
                {
                    this.Size = new System.Drawing.Size(this.Size.Width, this.Size.Height - 30);
                    tbUsername.Enabled = false;
                    cbSex.Enabled = false;
                    lbPassword.Visible = false;
                    tbPassword.Visible = false;
                    tbProvince.Enabled = false;
                    tbCity.Enabled = false;
                    tbDistrict.Enabled = false;
                    tbMobile.Enabled = false;
                    tbEmail.Enabled = false;
                    tbQQ.Enabled = false;
                    dtpRegisterDay.Enabled = false;
                    tbLastPayDate.Enabled = false;
                    tbUserExpire.Enabled = false;
                    tbName.Enabled = false;
                    tbID.Enabled = false;
                    dtpBirthday.Enabled = false;
                    tbLocation.Enabled = false;
                    tbMZ.Enabled = false;

                    lbLastLogonTimeLabel.Enabled = false;
                    lbCurrentLogonTimeLabel.Enabled = false;
                    tbBankName.Enabled = false;
                    tbAccount.Enabled = false;
                    lbLogonMinutes.Enabled = false;
                    tbLYCode.Enabled = false;

                    tbMobile.Text = "********";
                    tbEmail.Text = "********";
                    tbQQ.Text = "********";
                    tbName.Text = "********";
                    tbID.Text = "********";
                    tbLocation.Text = "********";
                    tbBankName.Text = "********";
                    tbAccount.Text = "********";
                }
            }
            else if (Form1.USER_DEGREE == 0)
            {
                this.Text = "添加会员或管理员";
            }
        }

        private void UpdateDetailedFormInfo(Dictionary<string, string> temp)
        {
            temp["user_id"] = currentUserID;
            temp["oldUserName"] = oldUserName;
            temp["user_name"] = tbUserID.Text;
            temp["sex"] = cbSex.Text;
            temp["realname"] = tbName.Text;
            temp["nickname"] = tbUsername.Text;
            temp["reg_time"] = Form1.GetInstence().ConvertDTToTimestamp(dtpRegisterDay.Value).ToString();
            //temp["birthday"] = Form1.GetInstence().ConvertDTToTimestamp(dtpBirthday.Value).ToString();
            temp["birthday"] = dtpBirthday.Value.Date.ToString();
            temp["idcard"] = tbID.Text;
            temp["userDegree"] = cbUserdegree.SelectedIndex.ToString();
            temp["bankcard"] = tbAccount.Text;
            temp["email"] = tbEmail.Text;
            temp["qq"] = tbQQ.Text;
            temp["mobile_phone"] = tbMobile.Text;
            temp["password"] = (tbPassword.Text.Equals("") ? currentUserPassword : tbPassword.Text);
            temp["lisExpireTime"] = tbUserExpire.Value.ToString();
            temp["province"] = tbProvince.Text;
            temp["city"] = tbCity.Text;
            temp["bank"] = tbBankName.Text;
            temp["cyykey"] = tbLYCode.Text;
            temp["beizhu"] = tbBeizhu.Text;
            temp["paid"] = cbPaid.SelectedIndex.ToString();
            temp["LastPaidDate"] = tbLastPayDate.Value.ToString();
            temp["address"] = tbLocation.Text;
            temp["race"] = tbMZ.Text;
            temp["IsAuthedFlag"] = cbRegistery.SelectedIndex.ToString();
            temp["paidToDate"] = dtpJSDate.Value.ToString();
            temp["paidWayNId"] = tbJSIDNumber.Text;
            temp["area"] = tbDistrict.Text;
            //temp["LogonMinutes"] = lbLogonMinutes.Text;

            // DLquanxian
            temp["DLquanxian"] = (cbViewUser.Checked ? 1 : 0).ToString() + (cbAddUser.Checked ? 1 : 0).ToString() + (cbBanUser.Checked ? 1 : 0).ToString();
        }

        // 修改或者添加
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.isModified = true;

            if (FuctionButton.Text.Equals("提交修改"))
            {
                UpdateDetailedFormInfo(Form1.generalUserDetailDic);

                if (tbPassword.Text.Equals(""))
                {
                    Form1.GetInstence().UpdateUserDetailedInfo(false);
                    //Form1.GetInstence().RefreshGeneralUser();
                    this.Close();
                }
                else
                {
                    Form1.GetInstence().UpdateUserDetailedInfo(true);
                    //Form1.GetInstence().RefreshGeneralUser();
                    this.Close();
                }
                tbPassword.Text = "";
            }
            else
            {
                if (checkNecessaryCreateUserItem())
                {
                    UpdateDetailedFormInfo(tempDic);
                    Form1.GetInstence().CreateNewUserDetailed(tempDic);
                    //Form1.GetInstence().RefreshGeneralUser();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("请先填写必填项目", "提示");
                }
            }
        }

        private bool checkNecessaryCreateUserItem()
        {
            bool isOK = false;

            if (!tbUserID.Text.Equals("") && !tbUsername.Text.Equals("") && !tbPassword.Text.Equals("") && !cbUserdegree.Text.Equals("") && !tbName.Text.Equals("") && !cbSex.Text.Equals(""))
            {
                isOK = true;
            }
            else
            {
                tbUserID.BackColor = Color.Yellow;
                tbUsername.BackColor = Color.Yellow;
                tbPassword.BackColor = Color.Yellow;
                cbUserdegree.BackColor = Color.Yellow;
                tbName.BackColor = Color.Yellow;
                cbSex.BackColor = Color.Yellow;

                isOK = false;
            }

            return isOK;
        }

        private bool isMoreFunction = false;
        private void button1_Click_1(object sender, EventArgs e)
        {
            //604, 355 old
            //604, 395 new
            if (isMoreFunction)
            {
                btMoreFunctions.Text = "更多功能";
                isMoreFunction = false;
                this.Size = new Size(this.Size.Width, this.Size.Height - 40);
            }
            else
            {
                btMoreFunctions.Text = "收起";
                isMoreFunction = true;
                this.Size = new Size(this.Size.Width, this.Size.Height + 40);
            }
        }

        // check
        private void button3_Click(object sender, EventArgs e)
        {
            MD5 md5Hash = MD5.Create();
            string inputedPasswordOld = Form1.GetInstence().GetMd5Hash(md5Hash, tbOldPassword.Text).ToLower();
            if (inputedPasswordOld.Equals(currentUserPassword))
            {
                MessageBox.Show("密码与原始密码相同!", "提示");
            }
            else
            {
                MessageBox.Show("密码与原始密码不同, 请查看是否密码已经被修改!", "提示");
            }
        }

        private string calMintoTime(int minutes)
        {
            string fullTime = "";

            int month = minutes / (60 * 24 * 30);
            int day = (minutes - (60 * 24 * 30) * month) / (60 * 24);
            int hour = (minutes - (60 * 24) * day - (60 * 24 * 30) * month) / 60;
            int min = minutes - (60 * 24 * 30) * month - (60 * 24) * day - 60 * hour;

            fullTime = month + "个月 " + day + "天 " + hour + "小时 " + min + "分";
            return fullTime;

        }

        #region 三联地区联动
        private void tbProvince_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbProvince.SelectedIndex != -1)
            {
                // 获取选择的省.
                currentProvince = mainAddressData.GetProvince(tbProvince.SelectedValue as string);
                // 市  数据绑定.
                this.tbCity.DataSource = currentProvince.CityList;
            }
        }

        private void tbCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbCity.SelectedIndex != -1)
            {
                // 获取选择的市.
                currentCity = currentProvince.GetCity(tbCity.SelectedValue as string);
                // 区  数据绑定.
                this.tbDistrict.DataSource = currentCity.DistrictList;
            }
        }
        #endregion

    }
}

using DevComponents.DotNetBar;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CYQ.Data;
using System.IO.Ports;
using System.Runtime.InteropServices;

namespace Plugin_OpenDoor
{
    public partial class frmMain : Office2007Form
    {
        [DllImport("user32.dll", EntryPoint = "BringWindowToTop")]
        public static extern bool BringWindowToTop(IntPtr hwnd);
        private delegate void ShowMsg(string msg);
        private ShowMsg AddMsgDelegate;   
        public frmMain()
        {
            InitializeComponent();
            AddMsgDelegate = new ShowMsg(AddMsg);
            try
            {
                spCardReader = new SerialPort(Common.CardReaderCOM);
                spCardReader.DataReceived += spCardReader_DataReceived;
                spCardReader.Open();
                AddMsg("刷卡器：" + Common.CardReaderCOM + ",打开成功!");
            }
            catch (Exception ex)
            {
                AddMsg(ex.ToString());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (MProc proc = new MProc("OpenDoor"))
            {
                proc.Set("@CardNo", "111");
                proc.SetCustom("result", ParaType.OutPut);//如果有output值
                proc.ExeNonQuery();//执行语句
                string  result = proc.OutPutValue.ToString();//如果只有一个值
                MessageBox.Show(result);
            }
        }
        private int CheckPrivilege(string CardNo)
        {
            int result;
            try
            {
                using (MProc proc = new MProc("OpenDoor"))
                {
                    proc.Set("@CardNo", CardNo);
                    proc.SetCustom("result", ParaType.OutPut);//如果有output值
                    proc.ExeNonQuery();//执行语句
                    result = int.Parse(proc.OutPutValue.ToString());
                }
            }
            catch
            {
                result = 100;
            }
            return result;
        }        
        public void HandleCardNo(string cardNo)
        {
            string result;
            if (CheckPrivilege(cardNo) == -1)
                result =  "人员信息不存在";
            else if (CheckPrivilege(cardNo) == 0)
                result =  "没有开门权限";
            else if (CheckPrivilege(cardNo) == 1)
            {
                result = "有开门权限;" + FourInFourOutHelper.OpenDelay();
            }
            else
                result = "权限查找出错";
            AddMsg("接收卡号:" + cardNo + ";结果：" + result);
        }
        private byte[] ReceiveData = new byte[1000];
        private int ReceiveDataLen = 0; 
        private void spCardReader_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            int n = sp.BytesToRead;//读取当前有多少数据可读。
            byte[] ReadBuf = new byte[n];//新建内容。
            sp.Read(ReadBuf, 0, n);//读取缓冲区
            //
            try
            {
                if (ReceiveDataLen + ReadBuf.Length > 800)
                {
                    ReceiveDataLen = 0;
                    Buffer.BlockCopy(ReadBuf, 0, ReceiveData, ReceiveDataLen, 14);                    
                }
                else
                    Buffer.BlockCopy(ReadBuf, 0, ReceiveData, ReceiveDataLen, ReadBuf.Length);
                ReceiveDataLen = ReceiveDataLen + ReadBuf.Length;
                if (ReceiveDataLen >= 12)
                {
                    for (int i = 0; i < ReceiveDataLen - 12 + 1; i++)
                    {

                        if (ReceiveData[i] == 0x02 && ReceiveData[i +11] == 0x03 && ReceiveData[i + 9] == 0x0D && ReceiveData[i + 10] == 0x0A)
                        {
                            byte[] CardNo = new byte[8];
                            CardNo[0] = ReceiveData[i + 1];
                            CardNo[1] = ReceiveData[i + 2];
                            CardNo[2] = ReceiveData[i + 3];
                            CardNo[3] = ReceiveData[i + 4];
                            CardNo[4] = ReceiveData[i + 5];
                            CardNo[5] = ReceiveData[i + 6];
                            CardNo[6] = ReceiveData[i + 7];
                            CardNo[7] = ReceiveData[i + 8];
                            string cardno = Encoding.ASCII.GetString(ReceiveData, 1, 8);
                            HandleCardNo(cardno);
                            ReceiveDataLen = 0;
                            ReceiveData = new byte[1000];
                        }
                    }
                }
                if (ReceiveDataLen >= 14)
                {
                    for (int i = 0; i < ReceiveDataLen - 12 + 1; i++)
                    {

                        if (ReceiveData[i] == 0x02 && ReceiveData[i + 13] == 0x03 && ReceiveData[i + 11] == 0x0D && ReceiveData[i + 12] == 0x0A)
                        {
                            string cardno = Encoding.ASCII.GetString(ReceiveData, 1, 10);
                            HandleCardNo(cardno);
                            ReceiveDataLen = 0;
                            ReceiveData = new byte[1000];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.ToString());
                ReceiveDataLen = 0;
                ReceiveData = new byte[1000];
            }
        }
        void AddMsg(string msg)
        {
            if (this.lbxMsg.InvokeRequired)
            {
                // 很帅的调自己
                this.lbxMsg.Invoke(AddMsgDelegate, msg);
            }
            else
            {
                if (this.lbxMsg.Items.Count > 100)
                {
                    this.lbxMsg.Items.RemoveAt(0);
                }
                this.lbxMsg.Items.Add("【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "】" + msg);
                this.lbxMsg.TopIndex = this.lbxMsg.Items.Count - (int)(this.lbxMsg.Height / this.lbxMsg.ItemHeight);
            }
        }
        private void btnOpen_Click(object sender, EventArgs e)
        {
            FourInFourOutHelper.OpenDelay();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Maximized;
            this.Activate();
            BringWindowToTop(this.Handle);
        }

        private void 退出系统ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void 最小化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
            this.Hide(); 
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("确定关闭系统吗？", "关闭确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                this.Dispose();
                Application.Exit();
            }
            else
                e.Cancel = true;
            this.Hide();
        }

        private void frmMain_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Hide();  
        }
    }
}

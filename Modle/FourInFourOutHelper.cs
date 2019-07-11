using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Plugin_OpenDoor
{
    public static class FourInFourOutHelper
    {
        public static int SumCheck(byte[] Code, int Start, int CodeLen)
        {
            int SumCheck = 0;
            for (int i = 0; i < CodeLen; i++)
                SumCheck = SumCheck + Code[i + Start];
            return SumCheck;
        }
        public static byte[] OpenDelay(int Add, int PinNo, int DelayTimes)
        {
            byte[] data = new byte[9];
            data[0] = 0x00;
            data[1] = 0x5A;
            data[2] = 0x53;
            data[3] = (byte)Add;
            data[4] = 0x14;
            data[5] = (byte)Math.Pow(2, PinNo - 1);
            data[6] = 0x00;
            data[7] = (byte)DelayTimes;
            data[8] = (byte)SumCheck(data, 0, 8);
            return data;
        }
        public static string OpenDelay()
        {
            string result = "";
            try
            {
                using (SerialPort sp = new SerialPort(Common.LockCOM, 9600))
                {                    
                    byte[] data = FourInFourOutHelper.OpenDelay(0, Common.OpenPoint, Common.DelayTimes);
                    sp.Open();
                    sp.Write(data, 0, 9);
                    sp.Close();
                    result = "电子锁打开成功!";
                }
            }
            catch (Exception ex)
            {
                result = "电子锁打开失败!原因：" + ex.Message + "!";                
            }
            return result;
        }
    }
}

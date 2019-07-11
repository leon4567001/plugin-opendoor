using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Plugin_OpenDoor
{
    public class Common
    {
        public static string LockCOM;
        public static string CardReaderCOM;
        public static int DelayTimes;
        public static int OpenPoint;
        public Common()
        {
            try
            {
                LockCOM = AppConfig.GetApp("LockCOM");
                CardReaderCOM = AppConfig.GetApp("CardReaderCOM");
                DelayTimes = int.Parse(AppConfig.GetApp("DelayTimes"));
                OpenPoint = int.Parse(AppConfig.GetApp("OpenPoint"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

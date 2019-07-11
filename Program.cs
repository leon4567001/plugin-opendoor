using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Plugin_OpenDoor
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Common common = new Common();
            AppInstance();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);//这两行实现   XP   可视风格  
            Application.Run(new frmMain());
        }
        public static void AppInstance()
        {
            Process[] MyProcesses = Process.GetProcesses();
            int i = 0;
            Process process = null;
            foreach (Process MyProcess in MyProcesses)
            {
                if (MyProcess.ProcessName == Process.GetCurrentProcess().ProcessName)
                {
                    process = MyProcess;
                    i++;
                }
            }
            if (i > 1)
            {
                process.Kill();
                process.Close();
            }
        }
    }
}

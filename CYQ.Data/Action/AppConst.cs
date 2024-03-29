using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CYQ.Data
{
    /// <summary>
    /// 内部常量类
    /// </summary>
    internal static class AppConst
    {
        #region License 常量
        //internal const string Lic_Error_Contact = "\r\nContact email:cyq1162@126.com;QQ:272657997\r\n site : http://www.cyqdata.com/cyqdata";
        ////internal const string Lic_Error_AtNight = "Sorry ! You need to get a license key when you run it at night!";
        //internal const string Lic_Error_NotBuyProvider = "Sorry ! Your license key not contains this provider function : ";
        //internal const string Lic_Error_InvalidVersion = "Sorry ! Your license key version invalid!";
        //internal const string Lic_Error_InvalidKey = "Sorry ! Your license key is invalid!";
        //internal const string Lic_PublicKey = "CYQ.Data.License";
        //internal const string Lic_UseKeyFileName = "cyq.data.keys";
        ////internal const string Lic_DevKeyFileName = "/cyq.data.dev.keys";
        //internal const string Lic_MacKeyType = "mac";
        //internal const string Lic_DllKeyType = "dll";
        //internal const string Lic_AriesCore = "Aries.Core";
        //internal const string Lic_AriesLogic = "Aries.Logic";
        #endregion

        #region 全局
        internal const string FilePre = "file:\\";
        internal const string Global_NotImplemented = "The method or operation is not implemented.";
        #endregion

        #region 静态常量
        /// <summary>
        /// 无效的文件路径字符
        /// </summary>
        internal static char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
        private static string _HR;

        internal static string HR
        {
            get
            {
                if (string.IsNullOrEmpty(_HR))
                {
                    _HR = System.Web.HttpContext.Current != null ? "<hr>" : "\r\n<---END--->\r\n";
                }
                return _HR;
            }
            set
            {
                _HR = value;
            }
        }
        private static string _BR;

        internal static string BR
        {
            get
            {
                if (string.IsNullOrEmpty(_BR))
                {
                    _BR = System.Web.HttpContext.Current != null ? "<br>" : "\r\n";
                }
                return _BR;
            }
            set
            {
                _BR = value;
            }
        }
        internal static string SplitChar = "$,$";
        /// <summary>
        /// 框架程序集名称
        /// </summary>
        internal static string _DLLFullName = string.Empty;
        static string _RunfolderPath;
        /// <summary>
        /// 框架的运行路径
        /// </summary>
        internal static string RunFolderPath
        {
            get
            {
                if (string.IsNullOrEmpty(_RunfolderPath))
                {
                    Assembly ass = System.Reflection.Assembly.GetExecutingAssembly();
                    _DLLFullName = ass.FullName;
                    _RunfolderPath = ass.CodeBase;
                    _RunfolderPath = System.IO.Path.GetDirectoryName(_RunfolderPath).Replace(AppConst.FilePre, string.Empty) + "\\";
                    ass = null;
                }
                return _RunfolderPath;
            }
        }
        #endregion
    }
}

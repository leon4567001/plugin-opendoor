using System;
using System.Collections.Generic;
using System.Text;

namespace CYQ.Data.Tool
{
    /// <summary>
    /// ����һ�����ܹ���ֵ
    /// </summary>
    internal class MD5
    {
        static Dictionary<string, string> md5Cache = new Dictionary<string, string>(32);
        //ȡ��MD5���ܿ�win2008���쳣����ʵ�ֲ��� Windows ƽ̨ FIPS ��֤�ļ����㷨��һ��
        //static System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        
    }
}

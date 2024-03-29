﻿using System;
using System.Data.Common;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using CYQ.Data.SQL;
using System.Collections.Generic;
using CYQ.Data.Tool;
using System.Threading;
using System.Data.SqlTypes;


namespace CYQ.Data
{

    /// <summary>
    /// 数据库操作基类
    /// </summary>
    internal abstract class DbBase : IDisposable
    {
        // private static MDictionary<string, bool> _dbOperator = new MDictionary<string, bool>();//数据库是否更新中
        /// <summary>
        /// 记录SQL语句信息
        /// </summary>
        internal System.Text.StringBuilder debugInfo = new System.Text.StringBuilder();

        /// <summary>
        /// 是否允许进入写日志中模块
        /// </summary>
        internal bool isAllowInterWriteLog = true;
        //        internal bool isErrorOnExeCommand;
        internal int recordsAffected = 0;//执行命令时所受影响的行数（-2为发生异常）。
        internal bool isWriteLog = false;//默认不启用，由AppSetting中配置控制，出现是为不用配置文件，内部手动控制启用。
        internal bool isUseUnsafeModeOnSqlite = false;
        internal bool isOpenTrans = false;
        internal DalType dalType = DalType.MsSql;
        private bool isAllowResetConn = true;//如果执行了非查询之后，为了数据的一致性，不允许切换到Slave数据库链接

        internal string tempSql = string.Empty;//附加信息，包括调试信息

        internal string conn = string.Empty;//原生态传进来的链接
        internal string providerName = string.Empty;//传进来的名称
        /// <summary>
        /// 数据库链接实体（创建的时候被赋值）;
        /// </summary>
        internal ConnObject connObject;

        internal IsolationLevel tranLevel = IsolationLevel.ReadCommitted;
        protected DbProviderFactory _fac = null;
        protected DbConnection _con = null;
        protected DbCommand _com;
        internal DbTransaction _tran;
        private Stopwatch _watch;

        /// <summary>
        /// 获得链接的数据库名称
        /// </summary>
        public virtual string DataBase
        {
            get
            {
                if (!string.IsNullOrEmpty(_con.Database))
                {
                    return _con.Database;
                }
                else if (dalType == DalType.Oracle)
                {
                    // (DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT = 1521)))(CONNECT_DATA =(SID = Aries)))
                    int i = _con.DataSource.LastIndexOf('=') + 1;
                    return _con.DataSource.Substring(i).Trim(' ', ')');
                }
                else
                {
                    return System.IO.Path.GetFileNameWithoutExtension(_con.DataSource);
                }
            }
        }
        private string _Version = string.Empty;
        /// <summary>
        /// 数据库的版本号
        /// </summary>
        public string Version
        {
            get
            {
                if (string.IsNullOrEmpty(_Version))
                {
                    switch (dalType)
                    {
                        case DalType.Txt:
                            _Version = "txt2.0";
                            break;
                        case DalType.Xml:
                            _Version = "xml2.0";
                            break;
                        default:
                            if (OpenCon(null))
                            {
                                _Version = _con.ServerVersion;
                                if (!isOpenTrans)//避免把事务给关闭了。
                                {
                                    CloseCon();
                                }
                            }
                            break;
                    }


                }
                return _Version;
            }
        }
        public DbConnection Con
        {
            get
            {
                return _con;
            }
        }
        public DbCommand Com
        {
            get
            {
                return _com;
            }
        }
        private bool _IsAllowRecordSql = true;
        /// <summary>
        /// 是否允许记录SQL语句 (内部操作会关掉此值为False）
        /// </summary>
        internal bool IsAllowRecordSql
        {
            get
            {
                return (AppConfig.Debug.OpenDebugInfo || AppConfig.Debug.SqlFilter > -1) && _IsAllowRecordSql;
            }
            set
            {
                _IsAllowRecordSql = value;
            }
        }
        public DbBase(ConnObject co)
        {
            this.connObject = co;
            this.conn = co.Master.Conn;
            this.providerName = co.Master.ProviderName;
            dalType = co.Master.ConnDalType;
            _fac = GetFactory(providerName);
            _con = _fac.CreateConnection();
            _con.ConnectionString = DalCreate.FormatConn(dalType, conn);
            _com = _con.CreateCommand();
            if (_com != null)//Txt| Xml 时返回Null
            {
                _com.Connection = _con;
                _com.CommandTimeout = AppConfig.DB.CommandTimeout;
            }
            if (IsAllowRecordSql)//开启秒表计算
            {
                _watch = new Stopwatch();
            }
            //if (AppConfig.DB.LockOnDbExe && dalType == DalType.Access)
            //{
            //    string dbName = DataBase;
            //    if (!_dbOperator.ContainsKey(dbName))
            //    {
            //        try
            //        {
            //            _dbOperator.Add(dbName, false);
            //        }
            //        catch
            //        {
            //        }
            //    }
            //}
            //_com.CommandTimeout = 1;
        }
        protected virtual DbProviderFactory GetFactory(string providerName)
        {
            return DbProviderFactories.GetFactory(providerName);
        }
        #region 数据库链接切换相关逻辑
        /// <summary>
        /// 切换数据库（修改数据库链接）
        /// </summary>
        /// <param name="dbName"></param>
        /// <returns></returns>
        internal DbResetResult ChangeDatabase(string dbName)
        {
            if (_con.State == ConnectionState.Closed)//事务中。。不允许切换
            {
                try
                {
                    if (IsExistsDbNameWithCache(dbName))//新的数据库不存在。。不允许切换
                    {
                        conn = GetNewConn(dbName);
                        _con.ConnectionString = DalCreate.FormatConn(dalType, conn);
                        connObject = DalCreate.GetConnObject(dbName + "Conn");
                        connObject.Master.Conn = conn;
                        return DbResetResult.Yes;
                    }
                    else
                    {
                        return DbResetResult.No_DBNoExists;
                    }
                }
                catch (Exception err)
                {
                    Log.WriteLogToTxt(err);
                }

            }
            return DbResetResult.No_Transationing;
        }

        //检测并切换数据库链接。
        internal DbResetResult ChangeDatabaseWithCheck(string dbTableName)//----------
        {
            if (IsOwnerOtherDb(dbTableName))//数据库名称变化了。
            {
                string dbName = dbTableName.Split('.')[0];
                return ChangeDatabase(dbName);

            }
            return DbResetResult.No_SaveDbName;
        }
        internal DbBase ResetDbBase(string dbTableName)
        {
            if (IsOwnerOtherDb(dbTableName))//是其它数据库名称。
            {
                string dbName = dbTableName.Split('.')[0];
                if (_con.State != ConnectionState.Closed)//事务中。。创建新链接切换
                {
                    return DalCreate.CreateDal(GetNewConn(dbName));
                }

            }
            return this;
        }
        /// <summary>
        /// 是否数据库名称变化了
        /// </summary>
        /// <param name="dbTableName"></param>
        /// <returns></returns>
        private bool IsOwnerOtherDb(string dbTableName)
        {
            int index = dbTableName.IndexOf('.');//DBName.TableName
            if (index > 0 && !dbTableName.Contains(" ")) //排除视图语句
            {

                string dbName = dbTableName.Split('.')[0];
                if (string.Compare(DataBase, dbName, StringComparison.OrdinalIgnoreCase) != 0 && conn.IndexOf(DataBase) == conn.LastIndexOf(DataBase))
                {
                    return true;
                }

            }
            return false;
        }
        protected string GetNewConn(string dbName)
        {
            string newConn = AppConfig.GetConn(dbName + "Conn");
            if (!string.IsNullOrEmpty(newConn))
            {
                return newConn;
            }
            return conn.Replace(DataBase, dbName);
        }

        static MDictionary<string, bool> dbList = new MDictionary<string, bool>(3);
        private bool IsExistsDbNameWithCache(string dbName)
        {
            try
            {
                string key = dalType.ToString() + "." + dbName;
                if (dbList.ContainsKey(key))
                {
                    return dbList[key];
                }
                bool result = IsExistsDbName(dbName);
                dbList.Add(key, result);
                return result;
            }
            catch
            {
                return true;
            }
        }
        protected abstract bool IsExistsDbName(string dbName);

        #endregion

        private int returnValue = -1;
        /// <summary>
        /// 存储过程返回值。
        /// </summary>
        public int ReturnValue
        {
            get
            {
                if (returnValue == -1 && _com != null && _com.Parameters != null && _com.Parameters.Count > 0)
                {
                    int.TryParse(Convert.ToString(_com.Parameters[_com.Parameters.Count - 1].Value), out returnValue);
                }
                return returnValue;
            }
            set
            {
                returnValue = value;
            }
        }
        /// <summary>
        /// 存储过程OutPut输出参数值。
        /// 如果只有一个输出，则为值；
        /// 如果有多个输出，则为Dictionary。
        /// </summary>
        public object OutPutValue
        {
            get
            {
                if (_com != null && _com.Parameters != null && _com.Parameters.Count > 0)
                {
                    Dictionary<string, object> opValues = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    object outPutValue = null;
                    foreach (DbParameter para in _com.Parameters)
                    {
                        if (para.Direction == ParameterDirection.Output)
                        {
                            opValues.Add(para.ParameterName, para.Value);
                            outPutValue = para.Value;
                        }
                    }
                    if (opValues.Count < 2)
                    {
                        opValues = null;
                        return outPutValue;
                    }
                    return opValues;
                }
                return null;
            }
        }
        public virtual char Pre
        {
            get
            {
                return '@';
            }
        }

        #region 执行
        /// <summary>
        /// 并发操作检测
        /// </summary>
        /// <returns></returns>
        //private bool CheckIsConcurrent()
        //{
        //    int waitTimes = 500;//5秒
        //    while (_dbOperator.ContainsKey(DataBase) && _dbOperator[DataBase] && waitTimes > -1)
        //    {
        //        waitTimes--;
        //        System.Threading.Thread.Sleep(10);
        //        if (waitTimes == 0)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        private DbDataReader ExeDataReaderSQL(string cmdText, bool isProc)
        {
            DbDataReader sdr = null;
            ConnBean coSlave = null;
            if (!isOpenTrans && _IsAllowRecordSql)
            {
                coSlave = connObject.GetSlave();
            }
            if (OpenCon(coSlave))
            {
                try
                {
                    sdr = _com.ExecuteReader(isOpenTrans ? CommandBehavior.KeyInfo : CommandBehavior.CloseConnection | CommandBehavior.KeyInfo);
                    if (sdr != null)
                    {
                        recordsAffected = sdr.RecordsAffected;
                    }
                }
                catch (DbException err)
                {
                    string msg = "ExeDataReader():" + err.Message;
                    debugInfo.Append(msg + AppConst.BR);
                    recordsAffected = -2;
                    WriteError(msg + (isProc ? "" : AppConst.BR + GetParaInfo(cmdText)));
                }
                //finally
                //{
                //    if (coSlave != null)
                //    {
                //        ChangeConn(connObject.Master);//恢复链接。
                //    }
                //}
            }
            return sdr;
        }
        public DbDataReader ExeDataReader(string cmdText, bool isProc)
        {

            SetCommandText(cmdText, isProc);
            DbDataReader sdr = null;
            //if (_dbOperator.ContainsKey(DataBase))
            //{
            //    if (!CheckIsConcurrent())
            //    {
            //        // dbOperator[DataBase] = true;
            //        sdr = ExeDataReaderSQL(cmdText, isProc);
            //        // dbOperator[DataBase] = false;
            //    }
            //}
            //else
            //{
            sdr = ExeDataReaderSQL(cmdText, isProc);
            //}
            WriteTime();
            return sdr;

        }
        private int ExeNonQuerySQL(string cmdText, bool isProc)
        {
            if (OpenCon())
            {
                try
                {
                    if (isUseUnsafeModeOnSqlite && !isProc && dalType == DalType.SQLite && !isOpenTrans)
                    {
                        _com.CommandText = "PRAGMA synchronous=Off;" + _com.CommandText;
                    }
                    //rowCount = 1;
                    recordsAffected = _com.ExecuteNonQuery();
                }
                catch (DbException err)
                {
                    string msg = "ExeNonQuery():" + err.Message;
                    debugInfo.Append(msg + AppConst.BR);
                    recordsAffected = -2;
                    WriteError(msg + (isProc ? "" : AppConst.BR + GetParaInfo(cmdText)));
                }
                finally
                {
                    if (!isOpenTrans)
                    {
                        CloseCon();
                    }
                }
            }
            return recordsAffected;
        }
        public int ExeNonQuery(string cmdText, bool isProc)
        {
            SetCommandText(cmdText, isProc);
            int rowCount = 0;
            //if (_dbOperator.ContainsKey(DataBase))
            //{
            //    if (!CheckIsConcurrent())
            //    {
            //        _dbOperator[DataBase] = true;
            //        rowCount = ExeNonQuerySQL(cmdText, isProc);
            //        _dbOperator[DataBase] = false;
            //    }
            //}
            //else
            //{
            rowCount = ExeNonQuerySQL(cmdText, isProc);
            //}
            WriteTime();
            return rowCount;
        }
        private object ExeScalarSQL(string cmdText, bool isProc)
        {
            object returnValue = null;
            ConnBean coSlave = null;
            //mssql 有 insert into ...select 操作。
            if (!isOpenTrans && _IsAllowRecordSql && !cmdText.ToLower().TrimStart().StartsWith("insert "))
            {
                coSlave = connObject.GetSlave();
            }
            if (OpenCon(coSlave))
            {
                try
                {
                    returnValue = _com.ExecuteScalar();
                    recordsAffected = returnValue == null ? 0 : 1;
                }
                catch (DbException err)
                {
                    string msg = "ExeScalar():" + err.Message;
                    debugInfo.Append(msg + AppConst.BR);
                    recordsAffected = -2;
                    WriteError(msg + (isProc ? "" : AppConst.BR + GetParaInfo(cmdText)));
                }
                finally
                {
                    if (!isOpenTrans)
                    {
                        CloseCon();
                    }
                }
            }
            return returnValue;
        }
        public object ExeScalar(string cmdText, bool isProc)
        {
            SetCommandText(cmdText, isProc);
            object returnValue = null;
            //if (_dbOperator.ContainsKey(DataBase))
            //{
            //    if (!CheckIsConcurrent())
            //    {
            //        returnValue = ExeScalarSQL(cmdText, isProc);
            //    }
            //}
            //else
            //{
            returnValue = ExeScalarSQL(cmdText, isProc);
            //}
            WriteTime();
            return returnValue;
        }
        /*
        private DataTable ExeDataTableSQL(string cmdText, bool isProc)
        {
            DbDataAdapter sdr = _fac.CreateDataAdapter();
            sdr.SelectCommand = _com;
            DataTable dataTable = null;
            if (OpenCon())
            {
                try
                {
                    dataTable = new DataTable();
                    recordsAffected = sdr.Fill(dataTable);
                }
                catch (DbException err)
                {
                    recordsAffected = -2;
                    WriteError("ExeDataTable():" + err.Message + (isProc ? "" : AppConst.BR + GetParaInfo(cmdText)));
                }
                finally
                {
                    sdr.Dispose();
                    if (!isOpenTrans)
                    {
                        CloseCon();
                    }
                }
            }
            return dataTable;
        }
        public DataTable ExeDataTable(string cmdText, bool isProc)
        {
            SetCommandText(cmdText, isProc);
            DataTable dataTable = null;
            //if (_dbOperator.ContainsKey(DataBase))
            //{
            //    if (!CheckIsConcurrent())
            //    {
            //        dataTable = ExeDataTableSQL(cmdText, isProc);
            //    }
            //}
            //else
            //{
                dataTable = ExeDataTableSQL(cmdText, isProc);
            //}
            WriteTime();
            return dataTable;
        }
        */
        #endregion
        public bool AddParameters(string parameterName, object value)
        {
            return AddParameters(parameterName, value, DbType.String, -1, ParameterDirection.Input);
        }
        public virtual bool AddParameters(string parameterName, object value, DbType dbType, int size, ParameterDirection direction)
        {
            if (dalType == DalType.Oracle)
            {
                parameterName = parameterName.Replace(":", "").Replace("@", "");
                if (dbType == DbType.String && size > 4000)
                {
                    AddCustomePara(parameterName, size == int.MaxValue ? ParaType.CLOB : ParaType.NCLOB, value, null);
                    return true;
                }
            }
            else
            {
                parameterName = parameterName.Substring(0, 1) == Pre.ToString() ? parameterName : Pre + parameterName;
            }
            if (Com.Parameters.Contains(parameterName))//已经存在，不添加
            {
                return false;
            }
            DbParameter para = _fac.CreateParameter();
            para.ParameterName = parameterName;
            para.Value = value == null ? DBNull.Value : value;
            if (dbType == DbType.Time)// && dalType != DalType.MySql
            {
                para.DbType = DbType.String;
            }
            else
            {
                if (dbType == DbType.DateTime && value != null)
                {
                    string time = Convert.ToString(value);
                    if (dalType == DalType.MsSql && time == DateTime.MinValue.ToString())
                    {
                        para.Value = SqlDateTime.MinValue;
                    }
                    else if (dalType == DalType.MySql && (time == SqlDateTime.MinValue.ToString() || time == DateTime.MinValue.ToString()))
                    {
                        para.Value = DateTime.MinValue;
                    }
                }
                para.DbType = dbType;
            }
            if (dbType == DbType.Binary && dalType == DalType.MySql)//mysql不能设定长度，否则会报索引超出了数组界限错误。
            {
                para.Size = -1;
            }
            else if (dbType != DbType.Binary && size > -1)
            {
                if (size != para.Size)
                {
                    para.Size = size;
                }
            }
            para.Direction = direction;

            Com.Parameters.Add(para);
            return true;
        }
        internal virtual void AddCustomePara(string paraName, ParaType paraType, object value, string typeName)
        {
        }
        //internal virtual void AddCustomePara(string paraName, ParaType paraType, object value)
        //{
        //}
        //public abstract DbParameter GetNewParameter();

        public void ClearParameters()
        {
            if (_com != null && _com.Parameters != null)
            {
                _com.Parameters.Clear();
            }
        }
        public abstract void AddReturnPara();

        private void SetCommandText(string commandText, bool isProc)
        {
            if (OracleDal.clientType > 0)
            {
                Type t = _com.GetType();
                System.Reflection.PropertyInfo pi = t.GetProperty("BindByName");
                if (pi != null)
                {
                    pi.SetValue(_com, true, null);
                }
            }
            _com.CommandText = isProc ? commandText : SqlFormat.Compatible(commandText, dalType, false);
            if (!isProc && dalType == DalType.SQLite && _com.CommandText.Contains("charindex"))
            {
                _com.CommandText += " COLLATE NOCASE";//忽略大小写
            }
            _com.CommandType = isProc ? CommandType.StoredProcedure : CommandType.Text;
            if (isProc)
            {
                if (commandText.Contains("SelectBase") && !_com.Parameters.Contains("ReturnValue"))
                {
                    AddReturnPara();
                    //检测是否存在分页存储过程，若不存在，则创建。
                    Tool.DBTool.CreateSelectBaseProc(dalType, conn);//内部分检测是否已创建过。
                }
            }
            else
            {
                //取消多余的参数，新加的小贴心，过滤掉用户不小心写多的参数。
                if (_com != null && _com.Parameters != null && _com.Parameters.Count > 0)
                {
                    bool needToReplace = (dalType == DalType.Oracle || dalType == DalType.MySql) && _com.CommandText.Contains("@");
                    string paraName;
                    for (int i = 0; i < _com.Parameters.Count; i++)
                    {
                        paraName = _com.Parameters[i].ParameterName.TrimStart(Pre);//默认自带前缀的，取消再判断
                        if (needToReplace && _com.CommandText.IndexOf("@" + paraName) > -1)
                        {
                            //兼容多数据库的参数（虽然提供了=:?"为兼容语法，但还是贴心的再处理一下）
                            switch (dalType)
                            {
                                case DalType.Oracle:
                                case DalType.MySql:
                                    _com.CommandText = _com.CommandText.Replace("@" + paraName, Pre + paraName);
                                    break;
                            }
                        }
                        if (_com.CommandText.IndexOf(Pre + paraName, StringComparison.OrdinalIgnoreCase) == -1)
                        {
                            _com.Parameters.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            //else
            //{
            //    string checkText = commandText.ToLower();
            //    //int index=
            //    //if (checkText.IndexOf("table") > -1 && (checkText.IndexOf("delete") > -1 || checkText.IndexOf("drop") > -1 || checkText.IndexOf("truncate") > -1))
            //    //{
            //    //    Log.WriteLog(commandText);
            //    //}
            //}
            if (IsAllowRecordSql)
            {
                tempSql = GetParaInfo(_com.CommandText) + AppConst.BR + "execute time is: ";
            }
        }
        private string GetParaInfo(string commandText)
        {
            string paraInfo = dalType + "." + DataBase + ".SQL: " + AppConst.BR + commandText;
            foreach (DbParameter item in _com.Parameters)
            {
                paraInfo += AppConst.BR + "Para: " + item.ParameterName + "-> " + (item.Value == DBNull.Value ? "DBNull.Value" : item.Value);
            }
            return paraInfo;
        }

        /// <summary>
        /// 记录执行时间
        /// </summary>
        private void WriteTime()
        {
            if (IsAllowRecordSql && _watch != null)
            {
                _watch.Stop();
                double ms = _watch.Elapsed.TotalMilliseconds;
                tempSql += ms + " (ms)" + AppConst.HR;
                if (AppConfig.Debug.OpenDebugInfo)
                {
                    debugInfo.Append(tempSql);
                    if (AppDebug.IsRecording && ms >= AppConfig.Debug.InfoFilter)
                    {
                        AppDebug.Add(tempSql);
                    }
                }
                if (AppConfig.Debug.SqlFilter >= 0 && ms >= AppConfig.Debug.SqlFilter)
                {
                    Log.WriteLogToTxt(tempSql, "SqlFilter_");
                }
                _watch.Reset();
                tempSql = null;
            }
        }

        #region IDisposable 成员

        public void Dispose()
        {
            if (_con != null)
            {
                CloseCon();
                _con = null;
            }
            if (_com != null)
            {
                _com = null;
            }
            if (_watch != null)
            {
                _watch = null;
            }
        }

        internal bool TestConn()
        {
            Thread thread = new Thread(TestOpen);
            thread.IsBackground = true;
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
            int sleepTimes = 0;
            while (!isOpenOK)
            {
                sleepTimes += 30;
                if (sleepTimes > 3000)
                {
                    thread.Abort();
                    break;
                }
                Thread.Sleep(30);
            }
            return isOpenOK;
        }
        private bool isOpenOK;
        private void TestOpen()
        {
            try
            {
                if (_con.State == ConnectionState.Closed)
                {
                    _con.Open();
                    _Version = _con.ServerVersion;//顺带设置版本号。
                    _con.Close();
                }
                isOpenOK = true;
            }
            catch (Exception err)
            {
                isOpenOK = false;
                debugInfo.Append(err.Message);
            }
        }
        internal bool OpenCon()
        {
            bool result = OpenCon(connObject.Master);
            if (_IsAllowRecordSql)
            {
                connObject.SetNotAllowSlave();
                isAllowResetConn = false;
            }
            return result;
        }

        /// <summary>
        /// 切换链接
        /// </summary>
        /// <param name="cb"></param>
        private void ResetConn(ConnBean cb)
        {
            if (_con != null && _con.State != ConnectionState.Open && conn != cb.Conn && _IsAllowRecordSql)
            {
                conn = cb.Conn;//切换。
                _con.ConnectionString = DalCreate.FormatConn(dalType, conn);
            }
        }
        internal bool OpenCon(ConnBean cb)
        {
            try
            {
                if (!isOpenTrans && cb != null && isAllowResetConn && connObject.IsAllowSlave())
                {
                    ResetConn(cb);
                }
                Open();
                if (IsAllowRecordSql)
                {
                    _watch.Start();
                }
                return true;
            }
            catch (DbException err)
            {
                if (cb == null && connObject.BackUp != null)
                {
                    connObject.InterChange();
                    return OpenCon(connObject.BackUp);
                    //if (OpenConBak())
                    //{
                    //    return true;
                    //}
                }
                else
                {
                    WriteError("OpenCon():" + err.Message);
                }

            }
            return false;
        }
        private void Open()
        {
            if (_con.State == ConnectionState.Closed)
            {
                if (dalType == DalType.Sybase)
                {
                    _com.Connection = _con;//重新赋值（Sybase每次Close后命令的Con都丢失）
                }
                _con.Open();
            }
            if (isOpenTrans)
            {
                if (_tran == null)
                {
                    _tran = _con.BeginTransaction(tranLevel);
                    _com.Transaction = _tran;
                }
                else if (_tran.Connection == null)
                {
                    //ADO.NET Bug：在 guid='123' 时不抛异常，但自动关闭链接，不关闭对象，会引发后续的业务不在事务中。
                    Dispose();
                    Error.Throw("Transation：Last execute command has syntax error：" + debugInfo.ToString());
                }
            }
        }
        /*
        private bool OpenConBak()
        {
            try
            {
                if (connObject.ProviderName == connObject.ProviderNameBak)//同种数据库链接。
                {
                    conn = connObject.ConnBak;//切换到备用。
                    _con.ConnectionString = conn;//切换到备用。
                    Open();
                    //交换主从链接
                    connObject.ExchangeConn();
                    if (IsAllowRecordSql)
                    {
                        _watch.Start();
                    }
                    return true;
                }
                else
                {
                    connObject.ExchangeConn();//不同种，切换链接后，本次操作直接抛异常
                }
            }
            catch (DbException err)
            {
                WriteError("OpenConBak():" + err.Message);
            }
            return false;
        }
         */
        internal void CloseCon()
        {
            try
            {
                if (_con.State != ConnectionState.Closed)
                {
                    if (_tran != null)
                    {
                        isOpenTrans = false;
                        if (_tran.Connection != null)
                        {
                            _tran.Commit();
                        }
                        _tran = null;
                    }
                    _con.Close();
                }
            }
            catch (DbException err)
            {
                WriteError("CloseCon():" + err.Message);
            }

        }
        public bool EndTransaction()
        {
            isOpenTrans = false;
            if (_tran != null)
            {
                try
                {
                    if (_tran.Connection == null)
                    {
                        return false;//上一个执行语句发生了异常（特殊情况在ExeReader guid='xxx' 但不抛异常)
                    }
                    _tran.Commit();

                }
                catch (Exception err)
                {
                    RollBack();
                    WriteError("EndTransaction():" + err.Message);
                    return false;
                }
                finally
                {
                    _tran = null;
                    CloseCon();
                }
            }
            return true;
        }
        /// <summary>
        /// 事务（有则）回滚
        /// </summary>
        /// <returns></returns>
        public bool RollBack()
        {
            if (_tran != null)
            {
                try
                {
                    if (_tran.Connection != null)
                    {
                        _tran.Rollback();
                    }
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    _tran = null;//以便重启事务，避免无法二次回滚。
                }
            }
            return true;
        }
        internal delegate void OnException(string msg);
        internal event OnException OnExceptionEvent;
        internal bool IsOnExceptionEventNull
        {
            get
            {
                return OnExceptionEvent == null;
            }
        }
        private void WriteError(string err)
        {
            err = dalType + " Call Function::" + err;
            if (_watch != null && _watch.IsRunning)
            {
                _watch.Stop();
                _watch.Reset();
            }
            RollBack();
            if (isAllowInterWriteLog)
            {
                Log.WriteLog(isWriteLog, err + AppConst.BR + debugInfo);
            }
            if (OnExceptionEvent != null)
            {
                try
                {
                    OnExceptionEvent(err);
                }
                catch
                {

                }
            }
            if (isOpenTrans)
            {
                Dispose();//事务中发生语句语法错误，直接关掉资源，避免因后续代码继续执行。
            }
        }
        #endregion
    }

}
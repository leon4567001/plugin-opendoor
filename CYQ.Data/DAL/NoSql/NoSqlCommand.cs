using System;
using System.Collections.Generic;
using System.Text;
using CYQ.Data.Table;
using CYQ.Data.SQL;
using System.Data.Common;

namespace CYQ.Data
{
    internal class NoSqlCommand : IDisposable // DbCommand
    {
        string tableName = string.Empty;
        string whereSql = string.Empty;
        string sourceSql = string.Empty;//��������SQL���
        public string CommandText
        {
            get
            {
                return sourceSql;
            }
            set
            {
                sourceSql = value;
                FormatSqlText(sourceSql);
            }
        }

        NoSqlAction action = null;
        public NoSqlCommand(string sqlText, DbBase dbBase)
        {
            try
            {
                if (string.IsNullOrEmpty(sqlText))
                {
                    return;
                }
                sourceSql = sqlText;
                FormatSqlText(sqlText);
            }
            catch (Exception err)
            {
                Log.WriteLogToTxt(err);
            }
            if (IsSelect || IsUpdate || IsDelete || IsInsert)
            {
                MDataRow row = new MDataRow();
                if (TableSchema.FillTableSchema(ref row, ref dbBase, tableName, tableName))
                {
                    row.Conn = dbBase.conn;
                    action = new NoSqlAction(ref row, tableName, dbBase.Con.DataSource, dbBase.dalType);
                }
            }
            else
            {
                Log.WriteLogToTxt("NoSql Grammar Error Or No Support : " + sqlText);
            }
        }
        public MDataTable ExeMDataTable()
        {
            int count = 0;
            MDataTable dt = action.Select(1, topN, whereSql, out count);
            if (fieldItems.Count > 0)
            {
                //���� a as B ���С�
                Dictionary<string, string> dic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (string field in fieldItems)
                {
                    string[] items = field.Trim().Split(' ');
                    dic.Add(items[0], items.Length > 1 ? items[items.Length - 1] : "");
                }
                for (int i = dt.Columns.Count - 1; i >= 0; i--)
                {
                    string columnName = dt.Columns[i].ColumnName;
                    if (!dic.ContainsKey(columnName))
                    {
                        dt.Columns.RemoveAt(i);
                    }
                    else if (dic[columnName] != "")//���� a as B ���С�
                    {
                        dt.Columns[i].ColumnName = dic[columnName];
                    }
                }
            }
            if (IsDistinct)
            {
                dt.Distinct();
            }
            return dt;

        }
        public int ExeNonQuery()
        {
            int count = -1;
            if (IsInsert || IsUpdate)
            {
                int index = 0;
                foreach (string item in fieldItems)
                {
                    index = item.IndexOf('=');//�����š�
                    if (index > -1)
                    {
                        string key = item.Substring(0, index);
                        string value = item.Substring(index + 1);
                        if (value.Length > 0 && value[0] == '\'')
                        {
                            value = value.Substring(1, value.Length - 2).Replace("''", "'");
                        }
                        action._Row.Set(key, value);
                    }
                }
                if (IsUpdate)
                {
                    action.Update(whereSql, out count);
                }
                else if (IsInsert && action.Insert(false))
                {
                    count = 1;
                }
            }
            else if (IsDelete)
            {
                action.Delete(whereSql, out count);
            }
            return count;
        }
        public object ExeScalar()
        {
            if (IsSelect)
            {
                if (IsGetCount)
                {
                    return action.GetCount(whereSql);
                }
                else if (fieldItems.Count > 0 && action.Fill(whereSql) && action._Row.Columns.Contains(fieldItems[0]))
                {
                    return action._Row[fieldItems[0]].Value;
                }
                return null;
            }
            else
            {
                return ExeNonQuery();
            }
        }

        void FormatSqlText(string sqlText)
        {
            string[] items = sqlText.Split(' ');
            foreach (string item in items)
            {
                switch (item.ToLower())
                {
                    case "insert":
                        IsInsert = true;
                        break;
                    case "into":
                        if (IsInsert)
                        {
                            IsInsertInto = true;
                        }
                        break;
                    case "select":
                        IsSelect = true;
                        break;
                    case "update":
                        IsUpdate = true;
                        break;
                    case "delete":
                        IsDelete = true;
                        break;
                    case "from":
                        IsFrom = true;
                        break;
                    case "count(*)":
                        IsGetCount = true;
                        break;
                    case "where":
                        whereSql = sqlText.Substring(sqlText.IndexOf(item) + item.Length + 1);
                        //�ý�������ˡ�
                        goto end;
                    case "top":
                        if (IsSelect && !IsFrom)
                        {
                            IsTopN = true;
                        }
                        break;
                    case "distinct":
                        if (IsSelect && !IsFrom)
                        {
                            IsDistinct = true;
                        }
                        break;
                    case "set":
                        if (IsUpdate && !string.IsNullOrEmpty(tableName) && fieldItems.Count == 0)
                        {
                            #region ����Update���ֶ���ֵ

                            int start = sqlText.IndexOf(item) + item.Length;
                            int end = sqlText.ToLower().IndexOf("where");
                            string itemText = sqlText.Substring(start, end == -1 ? sqlText.Length - start : end - start);
                            int quoteCount = 0, commaIndex = 0;

                            for (int i = 0; i < itemText.Length; i++)
                            {
                                if (i == itemText.Length - 1)
                                {
                                    string keyValue = itemText.Substring(commaIndex).Trim();
                                    if (!fieldItems.Contains(keyValue))
                                    {
                                        fieldItems.Add(keyValue);
                                    }
                                }
                                else
                                {
                                    switch (itemText[i])
                                    {
                                        case '\'':
                                            quoteCount++;
                                            break;
                                        case ',':
                                            if (quoteCount % 2 == 0)//˫����������ָ���
                                            {
                                                string keyValue = itemText.Substring(commaIndex, i - commaIndex).Trim();
                                                if (!fieldItems.Contains(keyValue))
                                                {
                                                    fieldItems.Add(keyValue);
                                                }
                                                commaIndex = i + 1;
                                            }
                                            break;

                                    }
                                }
                            }
                            #endregion
                        }
                        break;
                    default:
                        if (IsTopN && topN == -1)
                        {
                            int.TryParse(item, out topN);//��ѯTopN
                            IsTopN = false;//�ر�topN
                        }
                        else if ((IsFrom || IsUpdate || IsInsertInto) && string.IsNullOrEmpty(tableName))
                        {
                            tableName = item.Split('(')[0].Trim();//��ȡ������
                        }
                        else if (IsSelect && !IsFrom)//��ȡ��ѯ���м�������
                        {
                            #region Select �ֶ��Ѽ�
                            switch (item)
                            {
                                case "*":
                                case "count(*)":
                                case "distinct":
                                    break;
                                default:
                                    fieldText.Append(item + " ");
                                    break;
                            }

                            #endregion
                        }
                        else if (IsInsertInto && !string.IsNullOrEmpty(tableName) && fieldItems.Count == 0)
                        {
                            #region ����Insert Into���ֶ���ֵ

                            int start = sqlText.IndexOf(tableName) + tableName.Length;
                            int end = sqlText.IndexOf("values", start, StringComparison.OrdinalIgnoreCase);
                            string keys = sqlText.Substring(start, end - start).Trim();
                            string[] keyItems = keys.Substring(1, keys.Length - 2).Split(',');//ȥ�����������ٰ����ŷָ���

                            string values = sqlText.Substring(end + 6).Trim();
                            values = values.Substring(1, values.Length - 2);//ȥ����������
                            int quoteCount = 0, commaIndex = 0, valueIndex = 0;

                            for (int i = 0; i < values.Length; i++)
                            {
                                if (valueIndex >= keyItems.Length)
                                {
                                    break;
                                }
                                if (i == values.Length - 1)
                                {
                                    string value = values.Substring(commaIndex).Trim();
                                    keyItems[valueIndex] += "=" + value;
                                }
                                else
                                {
                                    switch (values[i])
                                    {
                                        case '\'':
                                            quoteCount++;
                                            break;
                                        case ',':
                                            if (quoteCount % 2 == 0)//˫����������ָ���
                                            {
                                                string value = values.Substring(commaIndex, i - commaIndex).Trim();
                                                keyItems[valueIndex] += "=" + value;
                                                commaIndex = i + 1;
                                                valueIndex++;
                                            }
                                            break;

                                    }
                                }
                            }
                            fieldItems.AddRange(keyItems);

                            #endregion
                        }
                        break;
                }
            }
        end:
            #region Select �ֶν���
            if (fieldText.Length > 0)
            {
                string[] fields = fieldText.ToString().Split(',');
                fieldItems.AddRange(fields);
                fieldText = null;
            }
            #endregion
        }
        StringBuilder fieldText = new StringBuilder();
        int topN = -1;
        bool IsInsert = false;
        bool IsInsertInto = false;
        bool IsSelect = false;
        bool IsUpdate = false;
        bool IsDelete = false;
        bool IsFrom = false;
        bool IsGetCount = false;
        // bool IsAll = false;
        bool IsTopN = false;
        bool IsDistinct = false;
        List<string> fieldItems = new List<string>();


        #region IDisposable ��Ա

        public void Dispose()
        {
            if (action != null)
            {
                action.Dispose();
            }
        }

        #endregion

        //public override void Cancel()
        //{
        //    throw new NotImplementedException();
        //}

        //public override int CommandTimeout
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override System.Data.CommandType CommandType
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //protected override DbParameter CreateDbParameter()
        //{
        //    throw new NotImplementedException();
        //}

        //protected override DbConnection DbConnection
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //protected override DbParameterCollection DbParameterCollection
        //{
        //    get { throw new NotImplementedException(); }
        //}

        //protected override DbTransaction DbTransaction
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public override bool DesignTimeVisible
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //protected override DbDataReader ExecuteDbDataReader(System.Data.CommandBehavior behavior)
        //{
        //    throw new NotImplementedException();
        //}

        //public override int ExecuteNonQuery()
        //{
        //    throw new NotImplementedException();
        //}

        //public override object ExecuteScalar()
        //{
        //    throw new NotImplementedException();
        //}

        //public override void Prepare()
        //{
        //    throw new NotImplementedException();
        //}

        //public override System.Data.UpdateRowSource UpdatedRowSource
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //    set
        //    {
        //        throw new NotImplementedException();
        //    }
        //}
    }
}

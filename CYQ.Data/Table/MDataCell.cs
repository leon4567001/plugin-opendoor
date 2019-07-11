using System;
using System.Data;
using System.Collections.Generic;
using CYQ.Data.SQL;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Collections;
using CYQ.Data.Tool;


namespace CYQ.Data.Table
{
    /// <summary>
    /// �нṹ��ѡ��
    /// </summary>
    public enum AlterOp
    {
        /// <summary>
        /// Ĭ�ϲ��޸�״̬
        /// </summary>
        None,
        /// <summary>
        /// ��ӻ��޸�״̬
        /// </summary>
        AddOrModify,
        /// <summary>
        /// ɾ����״̬
        /// </summary>
        Drop,
        /// <summary>
        /// ��������״̬
        /// </summary>
        Rename
    }
    /// <summary>
    /// ��Ԫ�ṹ��ֵ
    /// </summary>
    [Serializable]
    internal class MCellValue
    {
        internal bool IsNull = true;
        /// <summary>
        /// ״̬�ı�:0;δ��,1;���и�ֵ����[��ֵ��ͬ],2:��ֵ,ֵ��ͬ�ı���
        /// </summary>
        internal int State = 0;
        internal object Value = null;
    }

    /// <summary>
    /// ��Ԫ�ṹ����
    /// </summary>
    [Serializable]
    public class MCellStruct
    {
        private MDataColumn _MDataColumn = null;
        /// <summary>
        /// �ṹ����
        /// </summary>
        public MDataColumn MDataColumn
        {
            get
            {
                return _MDataColumn;
            }
            internal set
            {
                _MDataColumn = value;
            }
        }
        /// <summary>
        /// �Ƿ��ֵ���и�ʽУ��
        /// </summary>
        //public bool IsCheckValue = true;
        /// <summary>
        /// �Ƿ�ؼ���
        /// </summary>
        public bool IsPrimaryKey = false;

        /// <summary>
        /// �Ƿ�Ψһ����
        /// </summary>
        public bool IsUniqueKey = false;

        /// <summary>
        /// �Ƿ����
        /// </summary>
        public bool IsForeignKey = false;
        /// <summary>
        /// �������
        /// </summary>
        public string FKTableName;

        /// <summary>
        /// �ֶ�����
        /// </summary>
        public string Description;
        /// <summary>
        /// Ĭ��ֵ
        /// </summary>
        public object DefaultValue;
        /// <summary>
        /// �Ƿ�����ΪNull
        /// </summary>
        public bool IsCanNull;
        /// <summary>
        /// �Ƿ�������
        /// </summary>
        public bool IsAutoIncrement;
        /// <summary>
        /// �ɵ�������AlterOpΪRenameʱ���ã�
        /// </summary>
        public string OldName;
        /// <summary>
        /// ����
        /// </summary>
        public string ColumnName;
        /// <summary>
        /// ����
        /// </summary>
        public string TableName;
        private SqlDbType _SqlType;
        /// <summary>
        /// SqlDbType����
        /// </summary>
        public SqlDbType SqlType
        {
            get
            {
                return _SqlType;
            }
            set
            {
                _SqlType = value;
                ValueType = DataType.GetType(_SqlType, DalType);
            }
        }
        /// <summary>
        /// ����ֽ�
        /// </summary>
        public int MaxSize;

        /// <summary>
        /// ���ȣ�С��λ��
        /// </summary>
        public short Scale;
        /// <summary>
        /// ԭʼ�����ݿ��ֶ���������
        /// </summary>
        internal string SqlTypeName;
        internal Type ValueType;
        private DalType dalType = DalType.None;
        internal DalType DalType
        {
            get
            {
                if (_MDataColumn != null)
                {
                    return _MDataColumn.dalType;
                }
                return dalType;
            }
        }
        private AlterOp _AlterOp = AlterOp.None;
        /// <summary>
        /// �нṹ�ı�״̬
        /// </summary>
        public AlterOp AlterOp
        {
            get { return _AlterOp; }
            set { _AlterOp = value; }
        }
        //�ڲ�ʹ�õ����������ֶ���Ϊ��ʱʹ��
        internal int ReaderIndex = -1;

        #region ���캯��
        internal MCellStruct(DalType dalType)
        {
            this.dalType = dalType;
        }
        public MCellStruct(string columnName, SqlDbType sqlType)
        {
            Init(columnName, sqlType, false, true, false, -1, null);
        }
        public MCellStruct(string columnName, SqlDbType sqlType, bool isAutoIncrement, bool isCanNull, int maxSize)
        {
            Init(columnName, sqlType, isAutoIncrement, isCanNull, false, maxSize, null);
        }
        internal void Init(string columnName, SqlDbType sqlType, bool isAutoIncrement, bool isCanNull, bool isPrimaryKey, int maxSize, object defaultValue)
        {
            ColumnName = columnName.Trim();
            SqlType = sqlType;
            IsAutoIncrement = isAutoIncrement;
            IsCanNull = isCanNull;
            MaxSize = maxSize;
            IsPrimaryKey = isPrimaryKey;
            DefaultValue = defaultValue;
        }
        internal void Load(MCellStruct ms)
        {
            ColumnName = ms.ColumnName;
            SqlType = ms.SqlType;
            IsAutoIncrement = ms.IsAutoIncrement;
            IsCanNull = ms.IsCanNull;
            MaxSize = ms.MaxSize;
            Scale = ms.Scale;
            IsPrimaryKey = ms.IsPrimaryKey;
            IsUniqueKey = ms.IsUniqueKey;
            IsForeignKey = ms.IsForeignKey;
            FKTableName = ms.FKTableName;
            SqlTypeName = ms.SqlTypeName;
            AlterOp = ms.AlterOp;

            if (ms.DefaultValue != null)
            {
                DefaultValue = ms.DefaultValue;
            }
            if (!string.IsNullOrEmpty(ms.Description))
            {
                Description = ms.Description;
            }
        }
        /// <summary>
        /// ��¡һ������
        /// </summary>
        /// <returns></returns>
        public MCellStruct Clone()
        {
            MCellStruct ms = new MCellStruct(dalType);
            ms.ColumnName = ColumnName;
            ms.SqlType = SqlType;
            ms.IsAutoIncrement = IsAutoIncrement;
            ms.IsCanNull = IsCanNull;
            ms.MaxSize = MaxSize;
            ms.Scale = Scale;
            ms.IsPrimaryKey = IsPrimaryKey;
            ms.IsUniqueKey = IsUniqueKey;
            ms.IsForeignKey = IsForeignKey;
            ms.FKTableName = FKTableName;
            ms.SqlTypeName = SqlTypeName;
            ms.DefaultValue = DefaultValue;
            ms.Description = Description;
            ms.MDataColumn = MDataColumn;
            ms.AlterOp = AlterOp;
            return ms;
        }
        #endregion
    }
    /// <summary>
    /// ��Ԫ��
    /// </summary>
    [Serializable]
    public partial class MDataCell
    {
        internal MCellValue cellValue;
        private MCellStruct _CellStruct;

        #region ���캯��
        internal MDataCell(ref MCellStruct dataStruct)
        {
            Init(dataStruct, null);
        }

        internal MDataCell(ref MCellStruct dataStruct, object value)
        {
            Init(dataStruct, value);
        }

        #endregion

        #region ��ʼ��
        private void Init(MCellStruct dataStruct, object value)
        {
            cellValue = new MCellValue();
            _CellStruct = dataStruct;
            if (value != null)
            {
                Value = value;
            }

        }
        #endregion

        #region ����
        internal string strValue = string.Empty;
        /// <summary>
        /// ֵ
        /// </summary>
        public object Value
        {
            get
            {
                return cellValue.Value;
            }
            set
            {
                //if (!_CellStruct.IsCheckValue)
                //{
                //    cellValue.Value = value;
                //    return;
                //}

                bool valueIsNull = value == null || value == DBNull.Value;
                if (valueIsNull)
                {
                    if (cellValue.IsNull)
                    {
                        cellValue.State = (value == DBNull.Value) ? 2 : 1;
                    }
                    else
                    {
                        cellValue.State = 2;
                        cellValue.Value = null;
                        cellValue.IsNull = true;
                        strValue = string.Empty;
                    }
                }
                else
                {
                    strValue = value.ToString();
                    int groupID = DataType.GetGroup(_CellStruct.SqlType);
                    if (_CellStruct.SqlType != SqlDbType.Variant)
                    {
                        if (strValue == "" && groupID > 0)
                        {
                            cellValue.Value = null;
                            cellValue.IsNull = true;
                            return;
                        }
                        value = ChangeValue(value, _CellStruct.ValueType, groupID);
                        if (value == null)
                        {
                            return;
                        }
                    }

                    if (!cellValue.IsNull && (cellValue.Value.Equals(value) || (groupID != 999 && cellValue.Value.ToString() == strValue)))//����ıȽ�ֵ����==����������õ�ַ��
                    {
                        cellValue.State = 1;
                    }
                    else
                    {
                        cellValue.Value = value;
                        cellValue.State = 2;
                        cellValue.IsNull = false;
                    }

                }
            }
        }
        /// <summary>
        /// �������ͱ��л�����������ֵ�����͡�
        /// </summary>
        public bool FixValue()
        {
            Exception err = null;
            return FixValue(out err);
        }
        /// <summary>
        /// �������ͱ��л�����������ֵ�����͡�
        /// </summary>
        public bool FixValue(out Exception ex)
        {
            ex = null;
            if (!IsNull)
            {
                cellValue.Value = ChangeValue(cellValue.Value, _CellStruct.ValueType, DataType.GetGroup(_CellStruct.SqlType), out ex);
            }
            return ex == null;
        }
        internal object ChangeValue(object value, Type convertionType, int groupID)
        {
            Exception err;
            return ChangeValue(value, convertionType, groupID, out err);
        }
        /// <summary>
        ///  ֵ����������ת����
        /// </summary>
        /// <param name="value">Ҫ��ת����ֵ</param>
        /// <param name="convertionType">Ҫת������������</param>
        /// <param name="groupID">���ݿ����͵����</param>
        /// <returns></returns>
        internal object ChangeValue(object value, Type convertionType, int groupID, out Exception ex)
        {
            ex = null;
            strValue = Convert.ToString(value);
            if (value == null)
            {
                cellValue.IsNull = true;
                return value;
            }
            if (groupID > 0 && strValue == "")
            {
                cellValue.IsNull = true;
                return null;
            }
            try
            {
                #region ����ת��
                if (groupID == 1)
                {
                    switch (strValue)
                    {
                        case "�������":
                            strValue = "Infinity";
                            break;
                        case "�������":
                            strValue = "-Infinity";
                            break;
                    }
                }
                if (value.GetType() != convertionType)
                {
                    #region �۵�
                    switch (groupID)
                    {
                        case 0:
                            if (_CellStruct.SqlType == SqlDbType.Time)//time���͵����⴦��
                            {
                                string[] items = strValue.Split(' ');
                                if (items.Length > 1)
                                {
                                    strValue = items[1];
                                }
                            }
                            value = strValue;
                            break;
                        case 1:
                            switch (strValue.ToLower())
                            {
                                case "true":
                                    value = 1;
                                    break;
                                case "false":
                                    value = 0;
                                    break;
                                case "infinity":
                                    value = double.PositiveInfinity;
                                    break;
                                case "-infinity":
                                    value = double.NegativeInfinity;
                                    break;
                                default:
                                    goto err;
                            }
                            break;
                        case 2:
                            switch (strValue.ToLower().TrimEnd(')', '('))
                            {
                                case "now":
                                case "getdate":
                                case "current_timestamp":
                                    value = DateTime.Now;
                                    break;
                                default:
                                    DateTime dt = DateTime.Parse(strValue);
                                    value = dt == DateTime.MinValue ? (DateTime)SqlDateTime.MinValue : dt;
                                    break;
                            }
                            break;
                        case 3:
                            switch (strValue.ToLower())
                            {
                                case "yes":
                                case "true":
                                case "1":
                                case "on":
                                case "��":
                                    value = true;
                                    break;
                                case "no":
                                case "false":
                                case "0":
                                case "":
                                case "��":
                                default:
                                    value = false;
                                    break;
                            }
                            break;
                        case 4:
                            if (strValue == SqlValue.GUID || strValue.StartsWith("newid"))
                            {
                                value = Guid.NewGuid();
                            }
                            else
                            {
                                value = new Guid(strValue);
                            }
                            break;
                        default:
                        err:
                            if (convertionType.Name.EndsWith("[]"))
                            {
                                value = Convert.FromBase64String(strValue);
                                strValue = "System.Byte[]";
                            }
                            else
                            {
                                value = StaticTool.ChangeType(value, convertionType);
                            }
                            break;
                    }
                    #endregion
                }
                //else if (groupID == 2 && strValue.StartsWith("000"))
                //{
                //    value = SqlDateTime.MinValue;
                //}
                #endregion
            }
            catch (Exception err)
            {
                value = null;
                cellValue.Value = null;
                cellValue.IsNull = true;
                ex = err;
                string msg = string.Format("ChangeType Error��ColumnName��{0}��({1}) �� Value����{2}��\r\n", _CellStruct.ColumnName, _CellStruct.ValueType.FullName, strValue);
                strValue = null;
                if (AppConfig.Log.IsWriteLog)
                {
                    Log.WriteLog(true, msg);
                }

            }
            return value;
        }

        internal T Get<T>()
        {
            if (cellValue.IsNull)
            {
                return default(T);
            }
            Type t = typeof(T);
            return (T)ChangeValue(cellValue.Value, t, DataType.GetGroup(DataType.GetSqlType(t)));
        }

        /// <summary>
        /// ֵ�Ƿ�ΪNullֵ[ֻ������]
        /// </summary>
        public bool IsNull
        {
            get
            {
                return cellValue.IsNull;
            }
        }
        /// <summary>
        /// ֵ�Ƿ�ΪNull��Ϊ��[ֻ������]
        /// </summary>
        public bool IsNullOrEmpty
        {
            get
            {
                return cellValue.IsNull || strValue.Length == 0;
            }
        }
        /// <summary>
        /// ����[ֻ������]
        /// </summary>
        public string ColumnName
        {
            get
            {
                return _CellStruct.ColumnName;
            }
        }
        /// <summary>
        /// ��Ԫ��Ľṹ
        /// </summary>
        public MCellStruct Struct
        {
            get
            {
                return _CellStruct;
            }
        }
        /// <summary>
        /// Value��״̬:0;δ��,1;���и�ֵ����[��ֵ��ͬ],2:��ֵ,ֵ��ͬ�ı���
        /// </summary>
        public int State
        {
            get
            {
                return cellValue.State;
            }
            set
            {
                cellValue.State = value;
            }
        }
        #endregion

        #region ����
        /// <summary>
        /// ����Ĭ��ֵ��
        /// </summary>
        internal void SetDefaultValueToValue()
        {
            if (Convert.ToString(_CellStruct.DefaultValue).Length > 0)
            {
                switch (DataType.GetGroup(_CellStruct.SqlType))
                {
                    case 2:
                        Value = DateTime.Now;
                        break;
                    case 4:
                        if (_CellStruct.DefaultValue.ToString().Length == 36)
                        {
                            Value = new Guid(_CellStruct.DefaultValue.ToString());
                        }
                        else
                        {
                            Value = Guid.NewGuid();
                        }
                        break;
                    default:
                        Value = _CellStruct.DefaultValue;
                        break;
                }
            }
        }
        /// <summary>
        /// �ѱ����أ�Ĭ�Ϸ���Valueֵ��
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return strValue ?? "";
        }
        /// <summary>
        /// �Ƿ�ֵ��ͬ[����д�÷���]
        /// </summary>
        public override bool Equals(object value)
        {
            bool valueIsNull = (value == null || value == DBNull.Value);
            if (cellValue.IsNull)
            {
                return valueIsNull;
            }
            if (valueIsNull)
            {
                return cellValue.IsNull;
            }
            return strValue.ToLower() == Convert.ToString(value).ToLower();
        }
        /// <summary>
        /// ת����
        /// </summary>
        internal MDataRow ToRow()
        {
            MDataRow row = new MDataRow();
            row.Add(this);
            return row;
        }
        #endregion

    }
    //��չ��������
    public partial class MDataCell
    {
        internal string ToXml(bool isConvertNameToLower)
        {
            string text = strValue;
            switch (DataType.GetGroup(_CellStruct.SqlType))
            {
                case 999:
                    MDataRow row = null;
                    MDataTable table = null;
                    Type t = Value.GetType();
                    if (!t.FullName.StartsWith("System."))//��ͨ����
                    {
                        row = new MDataRow(TableSchema.GetColumns(t));
                        row.LoadFrom(Value);
                    }
                    else if (Value is IEnumerable)
                    {
                        int len = StaticTool.GetArgumentLength(ref t);
                        if (len == 1)
                        {
                            table = MDataTable.CreateFrom(Value);
                        }
                        else if (len == 2)
                        {
                            row = MDataRow.CreateFrom(Value);
                        }
                    }
                    if (row != null)
                    {
                        text = row.ToXml(isConvertNameToLower);
                    }
                    else if (table != null)
                    {
                        text = string.Empty;
                        foreach (MDataRow r in table.Rows)
                        {
                            text += r.ToXml(isConvertNameToLower);
                        }
                        text += "\r\n    ";
                    }
                    return string.Format("\r\n    <{0}>{1}</{0}>", isConvertNameToLower ? ColumnName.ToLower() : ColumnName, text);
                default:

                    if (text.LastIndexOfAny(new char[] { '<', '>', '&' }) > -1 && !text.StartsWith("<![CDATA["))
                    {
                        text = "<![CDATA[" + text.Trim() + "]]>";
                    }
                    return string.Format("\r\n    <{0}>{1}</{0}>", isConvertNameToLower ? ColumnName.ToLower() : ColumnName, text);
            }

        }
    }
}


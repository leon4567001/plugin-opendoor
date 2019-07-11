using System;
using System.Collections.Generic;
using System.Text;

namespace CYQ.Data.Table
{
    /// <summary>
    /// ��������ѡ��
    /// </summary>
    public enum AcceptOp
    {
        /// <summary>
        /// �������루��ϵͳ����������ID��
        /// ��ִ�лῪ������
        /// </summary>
        Insert,
        /// <summary>
        /// �������루���û�ָ��ID���룩
        /// ��ִ�лῪ������
        /// </summary>
        InsertWithID,
        /// <summary>
        /// ��������
        /// ��ִ�лῪ������
        /// </summary>
        Update,
        /// <summary>
        /// �����Զ��������£�����������������ڣ�����£������ڣ�����룩
        /// ��ִ�в��Ὺ������
        /// </summary>
        Auto
    }
    /// <summary>
    /// MDataTable �� MDataRow SetState �Ĺ���ѡ��
    /// </summary>
    public enum BreakOp
    {
        /// <summary>
        /// δ���ã���������
        /// </summary>
        None = -1,
        /// <summary>
        /// ��������ֵΪNull�ġ�
        /// </summary>
        Null = 0,
        /// <summary>
        /// ��������ֵΪ�յġ�
        /// </summary>
        Empty = 1,
        /// <summary>
        /// ��������ֵΪNull��յġ�
        /// </summary>
        NullOrEmpty = 2
    }

    /// <summary>
    /// MDataRow �� JsonHelper �����ݵĹ���ѡ��
    /// </summary>
    public enum RowOp
    {
        /// <summary>
        /// δ���ã�������У�����Nullֵ����
        /// </summary>
        None = -1,
        /// <summary>
        /// ������У���������Nullֵ����
        /// </summary>
        IgnoreNull = 0,
        /// <summary>
        /// ������в���״̬��ֵ
        /// </summary>
        Insert = 1,
        /// <summary>
        /// ������и���״̬��ֵ
        /// </summary>
        Update = 2
    }

    /// <summary>
    /// תʵ�����ò���
    /// </summary>
    internal enum SysType
    {
        /// <summary>
        /// ��������
        /// </summary>
        Base = 1,
        /// <summary>
        /// ö������
        /// </summary>
        Enum = 2,
        /// <summary>
        /// ����
        /// </summary>
        Array = 3,
        /// <summary>
        /// ��������
        /// </summary>
        Collection = 4,
        /// <summary>
        /// ����
        /// </summary>
        Generic = 5,
        /// <summary>
        /// �Զ�����
        /// </summary>
        Custom = 99
    }
}

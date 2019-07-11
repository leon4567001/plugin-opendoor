using System;
using System.Collections.Generic;
using System.Text;

namespace CYQ.Data.Aop
{
    /// <summary>
    /// ����ڲ����ݿ��������ö��
    /// </summary>
    public enum AopEnum
    {
        /// <summary>
        /// ��ѯ������¼���� [����Ϊ˳��Ϊ��Aop.AopEnum.Fill, out result, _TableName, aop_where, aop_otherInfo]
        /// </summary>
        Select,
        /// <summary>
        /// ���뷽�� [����Ϊ˳��Ϊ��Aop.AopEnum.Insert, out result, _TableName, aop_Row, aop_otherInfo]
        /// </summary>
        Insert,
        /// <summary>
        /// ���·���  [����Ϊ˳��Ϊ��Aop.AopEnum.Update, out result, _TableName, aop_Row, aop_otherInfo]
        /// </summary>
        Update,
        /// <summary>
        /// ɾ������  [����Ϊ˳��Ϊ��Aop.AopEnum.Fill, out result, _TableName,aop_Row,aop_where, aop_otherInfo]
        /// </summary>
        Delete,
        /// <summary>
        /// ��ѯһ����¼���� [����Ϊ˳��Ϊ��Aop.AopEnum.Fill, out result, _TableName, aop_where, aop_otherInfo]
        /// </summary>
        Fill,
        /// <summary>
        /// ȡ��¼���� [����Ϊ˳��Ϊ��Aop.AopEnum.GetCount, out result, _TableName, aop_where, aop_otherInfo]
        /// </summary>
        GetCount,
        /// <summary>
        /// MProc��ѯ����List<MDataTable>���� [����Ϊ˳��Ϊ��AopEnum.ExeMDataTableList, out result, procName, isProc, DbParameterCollection, aopInfo]
        /// </summary>
        ExeMDataTableList,
        /// <summary>
        /// MProc��ѯ����MDataTable���� [����Ϊ˳��Ϊ��AopEnum.ExeMDataTable, out result, procName, isProc, DbParameterCollection, aopInfo]
        /// </summary>
        ExeMDataTable,
        /// <summary>
        /// MProcִ�з�����Ӱ���������� [����Ϊ˳��Ϊ��AopEnum.ExeNonQuery, out result, procName, isProc, DbParameterCollection, aopInfo]
        /// </summary>
        ExeNonQuery,
        /// <summary>
        /// MProcִ�з����������з��� [����Ϊ˳��Ϊ��AopEnum.ExeScalar, out result, procName, isProc, DbParameterCollection, aopInfo]
        /// </summary>
        ExeScalar
    }
    /// <summary>
    /// Aop�����Ĵ�����
    /// </summary>
    public enum AopResult
    {
        /// <summary>
        /// �������ִ��ԭ���¼���������Aop.End�¼�
        /// </summary>
        Default,
        /// <summary>
        /// �����������ִ��ԭ���¼���Aop.End�¼�
        /// </summary>
        Continue,
        /// <summary>
        /// �����������ԭ���¼�,����ִ��Aop End�¼�
        /// </summary>
        Break,
        /// <summary>
        /// �������ֱ������ԭ�к�����ִ��
        /// </summary>
        Return,
    }

    /// <summary>
    /// Aop����ѡ��
    /// </summary>
    public enum AopOp
    {
        /// <summary>
        /// ������
        /// </summary>
        OpenAll,
        /// <summary>
        /// �����ڲ�Aop�����Զ����棬�ر��ⲿAop��
        /// </summary>
        OnlyInner,
        /// <summary>
        /// �����ⲿAop���ر��Զ����棩
        /// </summary>
        OnlyOuter,
        /// <summary>
        /// ���ⶼ�أ��Զ�������ⲿAop��
        /// </summary>
        CloseAll
    }
}

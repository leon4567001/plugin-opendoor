using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Caching;

namespace CYQ.Data.Cache
{
    /// <summary>
    /// ����������Ϣ
    /// </summary>
    public class CacheDependencyInfo
    {
        /// <summary>
        /// �����õĴ�����
        /// </summary>
        public int callCount = 0;
        /// <summary>
        /// ���������ʱ��
        /// </summary>
        public DateTime createTime;
        /// <summary>
        /// ������ٷ���
        /// </summary>
        public Double cacheMinutes = 0;
        private DateTime cacheChangeTime = DateTime.MinValue;
        private CacheDependency fileDependency = null;
        public CacheDependencyInfo(CacheDependency dependency, double cacheMinutes)
        {
            if (dependency != null)
            {
                fileDependency = dependency;
                cacheChangeTime = fileDependency.UtcLastModified;
            }
            createTime = DateTime.Now;
            this.cacheMinutes = cacheMinutes;
        }
        /// <summary>
        /// ϵͳ�ļ������Ƿ����ı�
        /// </summary>
        public bool IsChanged
        {
            get
            {
                if (fileDependency != null && (fileDependency.HasChanged || cacheChangeTime != fileDependency.UtcLastModified))
                {
                    cacheChangeTime = fileDependency.UtcLastModified;
                    return true;
                }
                return false;
            }
        }

        internal bool UserChange = false;
        /// <summary>
        /// ��ʶ�������ĸ���״̬
        /// </summary>
        /// <param name="isChange"></param>
        internal void SetState(bool isChange)
        {
            UserChange = IsChanged ? false : isChange;
        }
    }
}

using System;
using System.ComponentModel;
using System.Collections;

namespace CYQ.Data.Table
{

    /// <summary>
    /// 仅用于Winform的列表绑定。
    /// </summary>
    [Serializable]
    internal partial class MDataView : IListSource
    {
        MDataTable table;
        public MDataView(ref MDataTable dt)
        {
            table = dt;
        }

        
        #region IListSource 成员

        public bool ContainsListCollection
        {
            get
            {
                return true;
            }
        }

        public IList GetList()
        {
            return table.Rows;
        }

        #endregion
    }
    //internal partial class MDataView : IEnumerable
    //{

    //    public IEnumerator GetEnumerator()
    //    {
    //        return new System.Data.Common.DbEnumerator(table);
    //    }
    //}
}

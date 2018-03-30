using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMS.Framework.ExcelUtils
{
    /// <summary>
    /// 导入时有消息检查错误实体
    /// </summary>
    public class ImportError
    {
        public ImportError(string error) : this(null, null, error) { }
        public ImportError(int? columnIndex,string columnName,string error):this(columnIndex,columnName,null,error){}
        public ImportError(int? columnIndex, string columnName,int? rowIndex,string error)
        {
            if (columnIndex.HasValue)
            {
                Column = Utils.ExcelColumnIndexToName(columnIndex.Value);
            }
            ColumnName = columnName;
            Row = rowIndex;
            Error = error;
        }
        /// <summary>
        /// 列
        /// </summary>
        public string Column { get; set; }
        /// <summary>
        /// 列名
        /// </summary>
        public string ColumnName { get; set; }
        /// <summary>
        /// 行
        /// </summary>
        public int? Row { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string Error { get; set; }
        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorString
        {
            get
            {
                return $"{(Row.HasValue ? Row.Value + "行" : "")}{(!string.IsNullOrWhiteSpace(Column) ? Column + '列' : "")}{(!string.IsNullOrWhiteSpace(ColumnName) ? ColumnName : "")}{Error}";
            }
        }

        public bool Equals(ImportError other)
        {
            var isEquals= Column==other.Column&& ColumnName==other.ColumnName&& Row==other.Row && Error==other.Error;
            return isEquals;
        }
        //重载自定义类的Equals方法  
        public override bool Equals(object obj)
        {
            if (obj is ImportError)
                return Equals((ImportError)obj);
            return base.Equals(obj);
        }
    }
}

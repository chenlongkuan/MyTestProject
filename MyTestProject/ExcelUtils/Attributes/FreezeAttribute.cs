using System;

namespace TMS.Framework.Attributes
{
    /// <summary>
    ///冻结属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class FreezeAttribute : Attribute
    {
        /// <summary>
        /// 要冻结的列数
        /// </summary>
        public int ColSplit { get; set; } = 0;

        /// <summary>
        /// 要冻结的行数（冻结列为0）
        /// </summary>
        public int RowSplit { get; set; } = 1;

        /// <summary>
        /// 右边区域可见的首列序号，从1开始计算
        /// </summary>
        public int LeftMostColumn { get; set; } = 0;

        /// <summary>
        /// 下边区域可见的首行序号，也是从1开始计算
        /// </summary>
        public int TopRow { get; set; } = 1;
    }
}



using System;

namespace TMS.Framework.Attributes
{
    /// <summary>
    /// 表示一些简单统计数据的自定义属性。
    /// </summary>
    /// <remarks>
    /// 仅用于垂直，而不用于水平统计。在当前版本中 
    /// 不允许应用多个 <see cref="StatisticsAttribute"/>属性在一个类上.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class StatisticsAttribute : Attribute
    {
        /// <summary>
        /// 统计名称
        /// </summary>
        /// <remarks>
        /// 当前版本中，默认的统计位置是最后一行，第一个单元格
        /// </remarks>
        public string Name { get; set; }

        /// <summary>
        ///  统计方法，例如SUM，Average等用于垂直统计的方法
        /// </summary>
        public string Formula { get; set; }

        /// <summary>
        /// 设置统计列的列
        /// 如果 <see cref="Formula"/> 是 SUM, <see cref="Columns"/> 是 [1,3],统计方法SUM将会涵盖列是1至3列的第一行至最后一行数据.
        /// </summary>
        public int[] Columns { get; set; }
    }
}

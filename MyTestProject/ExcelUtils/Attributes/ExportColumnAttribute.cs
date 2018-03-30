
using System;
using System.Collections.Generic;

namespace TMS.Framework.Attributes
{
    /// <summary>
    /// Excel导入导出实体的字段属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ExcelColumnAttribute : Attribute
    {
        /// <summary>
       /// 字段标题
       /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// 字段列序
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 是否不为空，导入实体string类型不为空时，需设置此项为true
        /// </summary>
        public bool IsRequired { get; set; }
        /// <summary>
        /// 布尔类型真值意义
        /// </summary>
        public string TrueMeaningOfBoolean { get; set; }
        /// <summary>
        /// 布尔类型假值意义
        /// </summary>
        public string FalseMeaningOfBoolean { get; set; }
        /// <summary>
        /// 类型格式化,datetime和decimal的格式化 格式为："{0:******}"
        /// </summary>
        public string FormatString { get; set; }
        /// <summary>
        /// 整型的状态含义
        /// </summary>
        /// <remarks>请填写Dictionary的Json，例如：{\"0\":\"未审核\",\"-1\":\"未通过\",\"1\":\"已通过\"}</remarks>
        public string IntegerStateMeaning { get; set; }

        /// <summary>
        /// 是否多项导出
        /// </summary>
        public bool IsManyExport { get; set; }

        /// <summary>
        /// 数据库长度
        /// </summary>
        public int DbLength { get; set; }
    }
}

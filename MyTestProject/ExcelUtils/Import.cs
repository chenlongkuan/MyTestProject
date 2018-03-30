using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using TMS.Framework.Attributes;

namespace TMS.Framework.ExcelUtils
{
    /// <summary>
    /// 导入
    /// </summary>
    public static class Import
    {
        #region 添加数据验证错误消息到集合中
        /// <summary>
        /// 添加数据验证错误消息到集合中
        /// </summary>
        /// <param name="list">数据完整性验证错误消息集合</param>
        /// <param name="error">错误消息</param>
        public static void AddError(IList<ImportError> list, string error)
        {
            AddError(list, null, null, error);
        }
        /// <summary>
        /// 添加数据验证错误消息到集合中
        /// </summary>
        /// <param name="list">数据完整性验证错误消息集合</param>
        /// <param name="columnIndex">列序号</param>
        /// <param name="columnName">列标题</param>
        /// <param name="error">错误消息</param>
        public static void AddError(IList<ImportError> list, int? columnIndex, string columnName, string error)
        {
            AddError(list, columnIndex, columnName, null, error);
        }
        /// <summary>
        /// 添加数据验证错误消息到集合中
        /// </summary>
        /// <param name="list">数据完整性验证错误消息集合</param>
        /// <param name="columnIndex">列序号</param>
        /// <param name="columnName">列标题</param>
        /// <param name="rowIndex">行序号</param>
        /// <param name="error">错误消息</param>
        public static void AddError(IList<ImportError> list, int? columnIndex = null, string columnName = null, int? rowIndex = null, string error = null)
        {
            if (list != null)
            {
                var item = new ImportError(columnIndex, columnName, rowIndex.HasValue ? rowIndex.Value + 1 : rowIndex, error);
                if (list.Contains(item) == false)
                {
                    list.Add(item);
                }
            }
        }
        #endregion

        #region 初始化IWorkbook接口
        /// <summary>
        /// 初始化IWorkbook接口
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private static IWorkbook InitializeWorkbook(string filePath)
        {
            if (Path.GetExtension(filePath).Equals(".xls"))
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return new HSSFWorkbook(file);
                }
            }
            else if (Path.GetExtension(filePath).Equals(".xlsx"))
            {
                using (var file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    return new XSSFWorkbook(file);
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region 获取单元格的值
        /// <summary>
        /// 获取单元格的值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        internal static object GetCellValue(this IRow row, int index)
        {
            var cell = row.GetCell(index);
            if (cell == null)
            {
                return null;
            }

            switch (cell.CellType)
            {
                case CellType.Blank: return "";
                case CellType.Numeric:
                    short format = cell.CellStyle.DataFormat;
                    //对时间格式（2015.12.5、2015/12/5、2015-12-5等）的处理  
                    //if (format == 14 || format == 31 || format == 57 || format == 58 || format == 177)
                    if(DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue;
                    else
                        return cell.NumericCellValue;
                case CellType.String: return cell.StringCellValue;
                default:
                    return null;
            }
        }
        #endregion

        #region 将Excel读取到List中
        /// <summary>
        /// 将Excel读取到List中
        /// </summary>
        /// <typeparam name="T">模板实体</typeparam>
        /// <param name="filePath">excel路径</param>
        /// <param name="errors">数据完整性验证错误消息集合，不为null时就进行数据完整性验证</param>
        /// <returns>返回结果集</returns>
        public static List<T> ExcelToList<T>(string filePath, IList<ImportError> errors = null) where T : class, new()
        {
            if (!File.Exists(filePath))
            {
                AddError(errors, "上传Excel失败");
                return null;
            }

            //获取模板的属性
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            if (properties == null || properties.Length == 0)
            {
                AddError(errors, "模板中不存在字段");
                return null;
            }

            // 是否有可导入的字段数量
            var canImportCols = 0;
            //Excel导入实体的字段属性数组
            var attributes = new ExcelColumnAttribute[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var attr = properties[i].GetCustomAttributes(typeof(ExcelColumnAttribute), true) as ExcelColumnAttribute[];
                if (attr != null && attr.Length > 0)
                {
                    attributes[i] = attr[0];
                    canImportCols += 1;
                }
                else
                {
                    attributes[i] = null;
                }
            }

            if (canImportCols == 0)
            {
                AddError(errors, "模板中不含要导入的字段");
                return null;
            }


            var workbook = InitializeWorkbook(filePath);
            if (workbook == null)
            {
                AddError(errors, "上传文件不是有效的Excel文件");
                return null;
            }

            // 获取第一个sheet表格
            var sheet = workbook.GetSheetAt(0);

            if (sheet.LastRowNum < 1)//总行数小于1时
            {
                AddError(errors, "没有要导入的数据");
                return null;
            }

            // 获取表格行
            var rows = sheet.GetRowEnumerator();
            //表格标题行
            IRow headerRow = null;
            var list = new List<T>();
            int idx = 0;

            while (rows.MoveNext())
            {
                var row = rows.Current as IRow;

                if (idx == 0)
                {
                    headerRow = row;
                    if (errors != null)//需验证数据完整性，验证列数量是否匹配
                    {
                        var mathCount = 0;//模板与excel匹配的列数量
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var column = attributes[i];//attr

                            if (column == null)
                            {
                                continue;
                            }

                            //是否已匹配模板与excel列
                            var isMath = headerRow.Cells.Any(s => !string.IsNullOrWhiteSpace(s.StringCellValue) && s.StringCellValue.Equals(column.Title) && s.ColumnIndex.Equals(column.Index));
                            if (isMath)
                            {
                                mathCount++;
                            }
                            else
                            {
                                AddError(errors, column.Index, column.Title, "未找到列");
                            }
                        }
                        if (canImportCols != mathCount)//模板与excel列数量不匹配
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    var isBlank = IsBlankRow(row, properties.Length);
                    if (isBlank == false)
                    {
                        var item = new T();
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var prop = properties[i];//属性
                            var column = attributes[i];//attr

                            if (column == null)
                            {
                                if (prop.Name == "RowIndex")
                                {
                                    prop.SetValue(item, row.RowNum, null);
                                }
                                continue;
                            }

                            //prop.

                            var value = row.GetCellValue(column.Index);
                            // 可以为空的数据类型
                            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            //类型是否可为空
                            var isNullable = Utils.IsNullableType(prop.PropertyType);
                            if (propType == typeof(bool))
                            {
                                #region 转换bool类型
                                if (value == null || value.ToString() == "")
                                {
                                    if (!isNullable)
                                    {
                                        AddError(errors, column.Index, column.Title, idx, "不能为空");
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrWhiteSpace(column.TrueMeaningOfBoolean) && !string.IsNullOrWhiteSpace(column.FalseMeaningOfBoolean))
                                    {
                                        if (value.ToString().Equals(column.TrueMeaningOfBoolean))
                                        {
                                            prop.SetValue(item, true, null);
                                        }
                                        else if (value.ToString().Equals(column.FalseMeaningOfBoolean))
                                        {
                                            prop.SetValue(item, false, null);
                                        }
                                        else
                                        {
                                            AddError(errors, column.Index, column.Title, idx, "不是有效的布尔值");
                                        }
                                    }
                                    else
                                    {
                                        if (value.ToString().Equals("是"))
                                        {
                                            prop.SetValue(item, true, null);
                                        }
                                        else if (value.ToString().Equals("否"))
                                        {
                                            prop.SetValue(item, false, null);
                                        }
                                        else
                                        {
                                            AddError(errors, column.Index, column.Title, idx, "不是有效的布尔值");
                                        }
                                    }
                                }
                                #endregion
                            }
                            else if (propType == typeof(DateTime))
                            {
                                #region 转换日期类型
                                if (value == null || value.ToString() == "")
                                {
                                    if (!isNullable)
                                        AddError(errors, column.Index, column.Title, idx, "不能为空");
                                }
                                else
                                {
                                    try
                                    {
                                        var safeValue = Convert.ChangeType(value, propType, CultureInfo.CurrentCulture);
                                        var safeDate = Convert.ToDateTime(safeValue);
                                        prop.SetValue(item, safeDate, null);
                                    }
                                    catch
                                    {
                                        AddError(errors, column.Index, column.Title, idx, "不是有效的日期类型");
                                    }
                                }
                                #endregion
                            }
                            else if (propType.IsEnum)
                            {
                                #region 转换枚举类型
                                if (value == null || value.ToString() == "")
                                {
                                    if (!isNullable)
                                        AddError(errors, column.Index, column.Title, idx, "不能为空");
                                }
                                else
                                {
                                    var enumValue = Utils.GetEnumValue(propType, value.ToString());
                                    if (enumValue == null)
                                    {
                                        AddError(errors, column.Index, column.Title, idx, "不是有效的选项");
                                    }
                                    else
                                    {
                                        prop.SetValue(item, enumValue, null);
                                    }
                                }
                                #endregion
                            }
                            else if (propType == typeof(int))
                            {
                                #region 转换int类型
                                if (value == null || value.ToString() == "")
                                {
                                    if (!isNullable)
                                        AddError(errors, column.Index, column.Title, idx, "不能为空");
                                }
                                else
                                {
                                    try
                                    {
                                        var safeValue = Convert.ChangeType(value, propType);
                                        prop.SetValue(item, safeValue, null);
                                    }
                                    catch
                                    {
                                        AddError(errors, column.Index, column.Title, idx, "不是有效的整型");
                                    }
                                }
                                #endregion
                            }
                            else if (propType == typeof(decimal))
                            {
                                #region 转换数值类型
                                if (value == null || value.ToString() == "")
                                {
                                    if (!isNullable)
                                        AddError(errors, column.Index, column.Title, idx, "不能为空");
                                }
                                else
                                {
                                    try
                                    {
                                        var safeValue = Convert.ChangeType(value, propType);
                                        if (!string.IsNullOrWhiteSpace(column.FormatString))
                                        {
                                            prop.SetValue(item, string.Format(column.FormatString, value), null);
                                        }
                                        else
                                        {
                                            prop.SetValue(item, safeValue, null);
                                        }
                                    }
                                    catch
                                    {
                                        AddError(errors, column.Index, column.Title, idx, "不是有效的数值");
                                    }
                                }
                                #endregion
                            }
                            else if (propType == typeof(string))
                            {
                                #region 转换string类型
                                if (value == null || value.ToString() == "")
                                {
                                    if (column.IsRequired)
                                        AddError(errors, column.Index, column.Title, idx, "不能为空");
                                }
                                else
                                {
                                    //判断字段长度
                                    var val = value.ToString().Trim(' ');
                                    if (val.Length > column.DbLength && column.DbLength>0)
                                    {
                                        AddError(errors, column.Index, column.Title, idx, "长度不能大于"+column.DbLength);
                                    }
                                    else
                                    {
                                        prop.SetValue(item, val, null);
                                    }
                                }
                                #endregion
                            }
                            else
                            {
                                prop.SetValue(item, value, null);
                            }
                        }
                        list.Add(item);
                    }
                }

                idx++;
            }

            return list;
        }

        /// <summary>
        /// 是否空行
        /// </summary>
        /// <param name="row"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        private static bool IsBlankRow(IRow row, int length)
        {
            var isBlank = true;
            for (int i = 0; i < length; i++)
            {
                var value = row.GetCellValue(i);
                if (value != null && string.IsNullOrWhiteSpace(value.ToString()) == false)
                {
                    isBlank = false;
                    break;
                }
            }
            return isBlank;
        }



        #endregion
    }
}

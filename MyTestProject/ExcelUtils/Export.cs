using Newtonsoft.Json;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Web;
using TMS.Framework.Attributes;

namespace TMS.Framework.ExcelUtils
{
    /// <summary>
    /// 导出
    /// </summary>
    public static class Export
    {
        /// <summary>
        /// 将数据集合写入Excel并保存到服务器上
        /// </summary>
        /// <typeparam name="T">数据集类型</typeparam>
        /// <param name="data">数据集</param>
        /// <param name="book"></param>
        /// <param name="sheet"></param>
        /// <param name="startRow">起始行数</param>
        /// <returns>文件相对路径</returns>
        public static string ToExcelFile<T>(this IList<T> source,string savePath, IWorkbook book = null, ISheet sheet = null, int startRow = 0) where T : class, new()
        {
            var isxlsx = source.Count > 60000;//是否使用excel2007版本
            #region 初始化 book 和 sheet
            if (book == null)
            {
                if (!isxlsx)
                {
                    book = new HSSFWorkbook();
                }
                else
                {
                    book = new XSSFWorkbook();
                }
            }
            if (sheet == null)
            {
                sheet = book.CreateSheet();
            }
            #endregion

     
            source.ToSheetContent(book, sheet, startRow);

            #region 保存excel文件到服务器上
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);//创建新路径
            }
            var _fileName = "电子发票导出-"+DateTime.Now.ToString("yyyyMMddHHmmss") + (isxlsx ? ".xlsx" : ".xls");
            var filePath = savePath + _fileName;

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                book.Write(fs);
            }
            #endregion

            return filePath;
        }

        /// <summary>
        /// 将数据集读取到流中
        /// </summary>
        /// <typeparam name="T">数据集类型</typeparam>
        /// <param name="data">数据集</param>
        /// <param name="workbook"></param>
        /// <param name="sheet"></param>
        /// <param name="startRow">起始行数</param>
        /// <returns></returns>
        public static byte[] ToExcelContent<T>(this IList<T> source, IWorkbook book = null, ISheet sheet = null, int startRow = 0) where T : class, new()
        {
            #region 初始化 book 和 sheet
            if (book == null)
            {
                book = new HSSFWorkbook();
            }
            if (sheet == null)
            {
                sheet = book.CreateSheet();
            }
            #endregion

            source.ToSheetContent(book, sheet, startRow);

            using (var ms = new MemoryStream())
            {
                book.Write(ms);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 填充表格内容
        /// </summary>
        /// <typeparam name="T">数据集类型</typeparam>
        /// <param name="source">数据集</param>
        /// <param name="workbook"></param>
        /// <param name="sheet"></param>
        /// <param name="startRow">起始行数</param>
        /// <param name="fCellStyle">数据样式</param>
        public static void ToSheetContent<T>(this IList<T> source, IWorkbook workbook, ISheet sheet, int startRow, HSSFCellStyle fCellStyle = null) where T : class, new()
        {
            //属性
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty);
            //Excel导入实体的字段属性数组
            var attributes = new ExcelColumnAttribute[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                var attr = properties[i].GetCustomAttributes(typeof(ExcelColumnAttribute), true) as ExcelColumnAttribute[];
                if (attr != null && attr.Length > 0)
                {
                    attributes[i] = attr[0];
                }
                else
                {
                    attributes[i] = null;
                }
            }
            #region 填充标题
            // 标题样式
            var title_style = workbook.CreateCellStyle();
            title_style.Alignment = HorizontalAlignment.Center;
            title_style.VerticalAlignment = VerticalAlignment.Center;
            title_style.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            title_style.FillPattern = FillPattern.Bricks;
            title_style.FillBackgroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;

            // 标题行
            var title_row = sheet.CreateRow(startRow);
            for (var i = 0; i < properties.Length; i++)
            {
                var column = attributes[i];
                if (column == null)
                    continue;

                var cell = title_row.CreateCell(column.Index);
                cell.CellStyle = title_style;
                cell.SetCellValue(column.Title);
            }
            #endregion

            //设置文本格式
            //4.创建CellStyle与DataFormat并加载格式样式
            IDataFormat dataformat = workbook.CreateDataFormat();

            //【Tips】
            // 1.使用@ 或 text 都可以
            ICellStyle style0 = workbook.CreateCellStyle();
            style0.DataFormat = dataformat.GetFormat("@");

            #region 填充数据
            var rowIndex = startRow + 1;
            foreach (var item in source)
            {
                var row = sheet.CreateRow(rowIndex);
                for (var i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var column = attributes[i];
                    if (column == null)
                        continue;

                    var value = property.GetValue(item, null);
                    var cell = row.CreateCell(column.Index);
                    cell.CellStyle = style0;
                    if (value is ValueType)
                    {
                        if (value == null)
                        {
                            continue;
                        }
                        // 可以为空的数据类型
                        var propType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                        if (propType == typeof(bool))
                        {
                            if (!string.IsNullOrEmpty(column.TrueMeaningOfBoolean) && !string.IsNullOrEmpty(column.FalseMeaningOfBoolean))
                            {
                                cell.SetCellValue(((bool)value) ? column.TrueMeaningOfBoolean : column.FalseMeaningOfBoolean);
                            }
                            else
                            {
                                cell.SetCellValue(((bool)value) ? "是" : "否");
                            }
                        }
                        else if (propType == typeof(DateTime))
                        {
                            //var dateCellStyle = workbook.CreateCellStyle();
                            //var dateFormat = workbook.CreateDataFormat();
                            //dateCellStyle.DataFormat = dateFormat.GetFormat(!string.IsNullOrEmpty(column.FormatString) ? column.FormatString : "yyyy-MM-dd");
                            //cell.CellStyle = dateCellStyle;
                            //cell.SetCellValue(Convert.ToDateTime(value));

                            cell.SetCellValue(string.Format(!string.IsNullOrWhiteSpace(column.FormatString) ? column.FormatString : "{0:yyyy-MM-dd}", value));
                        }
                        else if (propType.IsEnum)
                        {
                            //cell.SetCellValue(((Enum)value).ToDescription());
                        }
                        else if (propType == typeof(int))
                        {
                            //整型状态意义解读
                            if (!string.IsNullOrEmpty(attributes[i].IntegerStateMeaning))
                            {
                                var stateMeaningDic = JsonConvert.DeserializeObject<Dictionary<int, string>>(column.IntegerStateMeaning);
                                foreach (var dicItem in stateMeaningDic)
                                {
                                    if (dicItem.Key == Convert.ToInt32(value))
                                    {
                                        cell.SetCellValue(dicItem.Value);
                                    }
                                }
                            }
                            else
                            {
                                cell.SetCellValue(Convert.ToInt32(value));
                            }
                        }
                        else if (propType == typeof(decimal))
                        {
                            cell.SetCellValue(Convert.ToDouble(string.Format(!string.IsNullOrWhiteSpace(column.FormatString) ? column.FormatString : "{0:0.####}", value)));
                        }
                        else
                        {
                            cell.SetCellValue(value.ToString());
                        }
                    }
                    else
                    {
                        cell.SetCellValue(value + "");
                    }
                }
                rowIndex++;
            }
            #endregion
            #region 统计和冻结

            if (rowIndex > 0)
            {
                // 统计行
                var statistics = typeof(T).GetCustomAttributes(typeof(StatisticsAttribute), true) as StatisticsAttribute[];
                if (statistics != null && statistics.Length > 0)
                {
                    var first = statistics[0];
                    var lastRow = sheet.CreateRow(rowIndex);
                    var cell = lastRow.CreateCell(0);
                    cell.SetCellValue(first.Name);
                    foreach (var column in first.Columns)
                    {
                        cell = lastRow.CreateCell(column);
                        cell.CellFormula = $"{first.Formula}({GetCellPosition(startRow + 1, column)}:{GetCellPosition(rowIndex - 1, column)})";
                    }
                }

                // 冻结
                var fattrs = typeof(T).GetCustomAttributes(typeof(FreezeAttribute), true) as FreezeAttribute[];
                if (fattrs != null && fattrs.Length > 0)
                {
                    var freeze = fattrs[0];
                    sheet.CreateFreezePane(freeze.ColSplit, freeze.RowSplit, freeze.LeftMostColumn, freeze.TopRow);
                }
            }
            #endregion
        }

        public static string GetCellPosition(int row, int col)
        {
            if (col >= 26)
            {
                col = Convert.ToInt32('A') + col - 26;
                row = row + 1;
                var a = "A" + (char)col + row.ToString();
                return a;
            }
            else
            {
                col = Convert.ToInt32('A') + col;
                row = row + 1;
                var a = (char)col + row.ToString();
                return a;
            }
        }
        /// <summary>
        /// 获取Excel ContentType
        /// </summary>
        /// <param name="isxlsx">是excel2007</param>
        /// <returns></returns>
        public static string GetExcelContentType(bool isxlsx)
        {
            return isxlsx ? "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" : "application/vnd.ms-excel";
        }
    }
}

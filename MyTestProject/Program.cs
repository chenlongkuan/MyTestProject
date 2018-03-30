using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Microsoft.VisualBasic.Devices;
using TMS.Framework.Attributes;

namespace MyTestProject
{
    class Program
    {
        static void Main(string[] args)
        {
            //GeographyFenceAlgorithm.testAlgorithm();
            var rootPath = @"C:\e-invoice\";
            DirectoryInfo root = new DirectoryInfo(rootPath);
            var exportList=new List<EInvoice>();
            foreach (FileInfo f in root.GetFiles())
            {
                if (f.Extension==".pdf")
                {
                    var item = ExtractTextFromPDFPage(rootPath, f.Name, 1);
                    exportList.Add(item);
                }
             
            }

            TMS.Framework.ExcelUtils.Export.ToExcelFile(exportList);

        }

        public static EInvoice ExtractTextFromPDFPage(string pdfFilePath,string filename, int pageNumber)
        {
            PdfReader reader = new PdfReader(pdfFilePath+filename);
            string text = PdfTextExtractor.GetTextFromPage(reader, pageNumber);
            try { reader.Close(); }
            catch { }
            //System.IO.File.WriteAllText(pdfFilePath+"1.txt",text);
            var invoiceNo = text.Substring(text.IndexOf("发票号码") + 6, text.IndexOf("开票日期") - text.IndexOf("发票号码") - 7);
            var newfileName = invoiceNo + ".pdf";
            var plateNumber = text.Substring(text.IndexOf("通行费") + 3, 7);
            var prePriceIndex = text.IndexOf(@"（小写）
￥") + 6;
            var nextPriceIndex = text.LastIndexOf(@"名　　　　称:");
            Console.WriteLine(prePriceIndex);
            Console.WriteLine(nextPriceIndex);
            var price = text.Substring(prePriceIndex, nextPriceIndex - prePriceIndex);
            Console.WriteLine("new file name:" + newfileName);
            Console.WriteLine(plateNumber);
            Console.WriteLine("price:"+price);
            var detailStartIndex = text.IndexOf("税　额") + 3;
            var detailInfoStr = text.Substring(detailStartIndex, text.IndexOf("合 计") - detailStartIndex);
           
            var detailArr = detailInfoStr.Split(' ');
            var propertyName = detailArr[0].Replace(plateNumber+"货车","").Replace(plateNumber+"客车","");
            Console.WriteLine("propertyName:"+ propertyName);
            var passStart = detailArr[1];
            Console.WriteLine("passStart:"+passStart);
            var passEnd = detailArr[2];
            Console.WriteLine("passEnd:"+passEnd);
            var invoicePrice = 0M;
            var taxRatio = "";
            var taxPrice = 0M;
            if (text.Contains("不征税"))
            {
                invoicePrice = decimal.Parse(detailArr[3].Replace("不征税＊＊＊",""));
                taxRatio = "不征税";
            }
            else
            {
                invoicePrice = decimal.Parse(detailArr[3]);
                taxRatio = detailArr[4];
                taxPrice = decimal.Parse(detailArr[5]);
            }
            Console.WriteLine("invoicePrice:" + invoicePrice);
            Console.WriteLine("taxRatio:"+taxRatio);
            Console.WriteLine("taxPrice:" + taxPrice);

            var carType = detailInfoStr.Substring(detailInfoStr.IndexOf(plateNumber) + 7, 2);
         
            if (filename!=newfileName)
            {
                Computer MyComputer = new Computer();
                MyComputer.FileSystem.RenameFile(pdfFilePath + filename, newfileName);
            }


            return new EInvoice()
            {
                InvoiceNo = invoiceNo,
                PlateNumber = plateNumber,
                CarType = carType,
                Price = decimal.Parse(price),
                PropertyName = propertyName,
                PassStartDate = passStart,
                PassEndDate = passEnd,
                InvoicePrice = invoicePrice,
                TaxRatio = taxRatio,
                TaxPrice = taxPrice
            };

        }

        public class EInvoice
        {
            [ExcelColumn(Index = 0,Title = "发票号码")]
            public string InvoiceNo { get; set; }

            [ExcelColumn(Index = 1, Title = "项目名称")]
            public string PropertyName { get; set; }

            [ExcelColumn(Index = 2,Title = "车牌号")]
            public string PlateNumber { get; set; }

            [ExcelColumn(Index = 3,Title = "车辆类型")]
            public string CarType { get; set; }
            
            [ExcelColumn(Index = 4,Title = "通行时间起")]
            public string PassStartDate { get; set; }

            [ExcelColumn(Index = 5,Title = "通行时间止")]
            public string PassEndDate { get; set; }

            [ExcelColumn(Index = 6,Title = "金额")]
            public decimal InvoicePrice { get; set; }

            [ExcelColumn(Index = 7,Title="税率")]
            public string TaxRatio { get; set; }

            [ExcelColumn(Index = 8,Title = "税额")]
            public decimal TaxPrice { get; set; }

            [ExcelColumn(Index = 9,Title = "价税合计")]
            public decimal Price { get; set; }  




        }
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Net;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Security.Claims;
using System.ComponentModel;

namespace TMS.Framework
{
    public class UserIdentity
    {
        public UserIdentity(int id, string name, string actor)
        {
            this.OptionerUserId = id;
            this.OptionerUserName = name;
            OptionerUserActor = actor;
        }
        public int OptionerUserId { get; private set; }
        public string OptionerUserName { get; private set; }
        /// <summary>
        /// 当前用户角色
        /// </summary>
        public string OptionerUserActor { get; private set; }
        /// <summary>
        /// 是否超级管理员
        /// </summary>
        public bool IsAdmin
        {
            get
            {
                return OptionerUserId == 1&&OptionerUserName== "超级管理员";
            }
        }
    }

    /// <summary>
    /// 辅助工具
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// 页码
        /// </summary>
        public static int PageSize = 100;

        /// <summary>
        /// 格式化时间戳
        /// </summary>
        /// <param name="timeSpan"></param>
        /// <returns></returns>
        public static string FormatTimeSpan(TimeSpan timeSpan)
        {
            var dateTime = (new DateTime(1970, 1, 1)).Add(timeSpan);
            return dateTime.ToString("HH:mm:ss");
        }

        /// <summary>
        /// 数字转中文
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string NumberToChineseYuan(decimal num)
        {
            var orignalNum = num;
            string str1 = "零壹贰叁肆伍陆柒捌玖";            //0-9所对应的汉字 
            string str2 = "万仟佰拾亿仟佰拾万仟佰拾元角分"; //数字位所对应的汉字 
            string str3 = "";    //从原num值中取出的值 
            string str4 = "";    //数字的字符串形式 
            string str5 = "";  //人民币大写金额形式 
            int i;    //循环变量 
            int j;    //num的值乘以100的字符串长度 
            string ch1 = "";    //数字的汉语读法 
            string ch2 = "";    //数字位的汉字读法 
            int nzero = 0;  //用来计算连续的零值是几个 
            int temp;            //从原num值中取出的值 

            num = Math.Round(Math.Abs(num), 2,MidpointRounding.AwayFromZero);    //将num取绝对值并四舍五入取2位小数 
            str4 = ((long)(num * 100)).ToString();        //将num乘100并转换成字符串形式 
            j = str4.Length;      //找出最高位 
            if (j > 15) { return "溢出"; }
            str2 = str2.Substring(15 - j);   //取出对应位数的str2的值。如：200.55,j为5所以str2=佰拾元角分 

            //循环取出每一位需要转换的值 
            for (i = 0; i < j; i++)
            {
                str3 = str4.Substring(i, 1);          //取出需转换的某一位的值 
                temp = Convert.ToInt32(str3);      //转换为数字 
                if (i != (j - 3) && i != (j - 7) && i != (j - 11) && i != (j - 15))
                {
                    //当所取位数不为元、万、亿、万亿上的数字时 
                    if (str3 == "0")
                    {
                        ch1 = "";
                        ch2 = "";
                        nzero = nzero + 1;
                    }
                    else
                    {
                        if (str3 != "0" && nzero != 0)
                        {
                            ch1 = "零" + str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                    }
                }
                else
                {
                    //该位是万亿，亿，万，元位等关键位 
                    if (str3 != "0" && nzero != 0)
                    {
                        ch1 = "零" + str1.Substring(temp * 1, 1);
                        ch2 = str2.Substring(i, 1);
                        nzero = 0;
                    }
                    else
                    {
                        if (str3 != "0" && nzero == 0)
                        {
                            ch1 = str1.Substring(temp * 1, 1);
                            ch2 = str2.Substring(i, 1);
                            nzero = 0;
                        }
                        else
                        {
                            if (str3 == "0" && nzero >= 3)
                            {
                                ch1 = "";
                                ch2 = "";
                                nzero = nzero + 1;
                            }
                            else
                            {
                                if (j >= 11)
                                {
                                    ch1 = "";
                                    nzero = nzero + 1;
                                }
                                else
                                {
                                    ch1 = "";
                                    ch2 = str2.Substring(i, 1);
                                    nzero = nzero + 1;
                                }
                            }
                        }
                    }
                }
                if (i == (j - 11) || i == (j - 3))
                {
                    //如果该位是亿位或元位，则必须写上 
                    ch2 = str2.Substring(i, 1);
                }
                str5 = str5 + ch1 + ch2;

                if (i == j - 1 && str3 == "0")
                {
                    //最后一位（分）为0时，加上“整” 
                    str5 = str5 + '整';
                }
            }
            if (num == 0)
            {
                str5 = "零元整";
            }
            if (orignalNum < 0)
            {
                str5 = "负"+str5;
            }
            return str5;
        }

        public static string NumberToChinese(decimal number)
        {
            string res = "";
            var isNegative = number < 0; // 是否是负数
            string str = Math.Abs(number).ToString();

            var len = str.Length;
            for (int i = 0; i < len; i++)
            {
                string schar = str.Substring(i, 1);

                switch (schar)
                {
                    case "1": res += "一"; break;
                    case "2": res += "二"; break;
                    case "3": res += "三"; break;
                    case "4": res += "四"; break;
                    case "5": res += "五"; break;
                    case "6": res += "六"; break;
                    case "7": res += "七"; break;
                    case "8": res += "八"; break;
                    case "9": res += "九"; break;
                    default: res += "零"; break;
                }
                if (len - i > 1)
                {
                    switch (len - i)
                    {
                        case 2:
                        case 6:
                        case 10: res += "十"; break;
                        case 3:
                        case 7:
                        case 11: res += "百"; break;
                        case 4:
                        case 8:
                        case 12: res += "千"; break;
                        case 5:
                        case 13: res += "万"; break;
                        case 9: res += "亿"; break;
                        default: res += ""; break;
                    }
                }
            }

            if (isNegative)
            {
                res = "负" + res;
            }

            return res;
        }

        /// <summary>
        /// 截取字符串后几位
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SubLast(this string str,int length)
        {
            if (!string.IsNullOrWhiteSpace(str))
            {
                return str.Substring(str.Length - length);
            }
            return null;
        }


        /// <summary>
        /// 判断类型是否可为空
        /// </summary>
        /// <param name="theType"></param>
        /// <returns></returns>
        public static bool IsNullableType(Type theType)
        {
            return (theType.IsGenericType && theType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }

        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
		/// 获取字段Description
		/// </summary>
		/// <param name="fieldInfo">FieldInfo</param>
		/// <returns>DescriptionAttribute[] </returns>
		public static DescriptionAttribute[] GetDescriptAttr(this FieldInfo fieldInfo)
        {
            if (fieldInfo != null)
            {
                return (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            }
            return null;
        }
        /// <summary>
        /// 根据Description获取枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="description"></param>
        /// <returns></returns>
        public static T GetEnumName<T>(string description)
        {
            Type _type = typeof(T);
            foreach (FieldInfo field in _type.GetFields())
            {
                DescriptionAttribute[] _curDesc = field.GetDescriptAttr();
                if (_curDesc != null && _curDesc.Length > 0)
                {
                    if (_curDesc[0].Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException(string.Format("{0} 未能找到对应的枚举.", description), "Description");
        }
        /// <summary>
        /// 根据Description获取枚举的值
        /// </summary>
        /// <param name="_type"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static object GetEnumValue(Type _type,string description)
        {
            foreach (FieldInfo field in _type.GetFields())
            {
                DescriptionAttribute[] _curDesc = field.GetDescriptAttr();
                if (_curDesc != null && _curDesc.Length > 0)
                {
                    if (_curDesc[0].Description == description)
                        return field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return field.GetValue(null);
                }
            }
            return null;
        }

        public static bool IsNullOrEmpty(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return true;
            strValue = strValue.Trim();
            if (string.IsNullOrEmpty(strValue))
                return true;
            return false;
        }
        public static bool IsURL(string strUrl)
        {
            if (string.IsNullOrEmpty(strUrl))
                return false;

            Regex RegexBr = new Regex(@"(\r\n)", RegexOptions.IgnoreCase);
            return Regex.IsMatch(strUrl, @"^(http|https)\://([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$");
        }
  
        public static string MD5(string str)
        {
            byte[] b = Encoding.Default.GetBytes(str);
            b = new MD5CryptoServiceProvider().ComputeHash(b);
            string ret = "";
            for (int i = 0; i < b.Length; i++)
            {
                ret = ret + b[i].ToString("x").PadLeft(2, '0');
            }
            return ret.ToUpper();
        }
        /// <summary>
        /// 获取随机码
        /// </summary>
        /// <param name="codeCount">随机码位数</param>
        /// <returns>返回随机数</returns>
        public static string CreateRandomCode(int codeCount)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,"
                + "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z,"
                + "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z";

            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            int temp = -1;

            Random rand = new Random();
            for (int i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(62);
                if (temp == t)
                {
                    return CreateRandomCode(codeCount);
                }
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;
        }

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public static string CreateRandomNumber(int count)
        {
            var result = "";
            var rand = new Random();
            for (int i = 0; i < count; i++)
            {
                result += rand.Next(10).ToString();
            }
            return result;
        }

        public static string GenerateSalt(int len)
        {
            SymmetricAlgorithm VI = SymmetricAlgorithm.Create();
            VI.GenerateKey();
            string str = Convert.ToBase64String(VI.Key);
            if (str.Length > len)
                return Convert.ToBase64String(VI.Key).Substring(0, len);
            return str;
        }
     
        public static T JsonConvertTo<T>(string json)
        {
            if (IsNullOrEmpty(json))
                return default(T);
            try
            {
                T obj = JsonConvert.DeserializeObject<T>(json);
                return obj;
            }
            catch
            {
                throw new Exception("JsonConvertTo 失败反序列化");
            }
        }
        public static string ToJson<T>(T obj)
        {
            if (obj != null)
            {
                return JsonConvert.SerializeObject(obj);
            }
            return null;
        }
    
        /// <summary>
        /// 枚举转换成List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<EnumberEntity> EnumToList<T>()
        {
            List<EnumberEntity> list = new List<EnumberEntity>();

            foreach (var e in Enum.GetValues(typeof(T)))
            {
                EnumberEntity m = new EnumberEntity();
                object[] objArr = e.GetType().GetField(e.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (objArr != null && objArr.Length > 0)
                {
                    DescriptionAttribute da = objArr[0] as DescriptionAttribute;
                    m.Desction = da.Description;
                }
                if (!string.IsNullOrWhiteSpace(m.Desction))
                {
                    m.EnumValue = Convert.ToInt32(e);
                    m.EnumName = e.ToString();
                    list.Add(m);
                }
            }
            return list;
        }

        /// <summary>
        /// 验证国内车牌号
        /// </summary>
        /// <param name="vehicleNumber"></param>
        /// <returns></returns>
        public static bool IsVehicleNumber(string vehicleNumber)
        {
            //string express = @"^[京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼使领A-Z]{1}[A-Z]{1}[警京津沪渝冀豫云辽黑湘皖鲁新苏浙赣鄂桂甘晋蒙陕吉闽贵粤青藏川宁琼]{0,1}[A-Z0-9]{4}[A-Z0-9挂学警港澳]{1}$";
            //return Regex.IsMatch(vehicleNumber, express);
            return true;
        }

        /// <summary>
        /// 身份证号码验证
        /// </summary>
        /// <param name="str_idcard"></param>
        /// <returns></returns>
        public static bool IsIDcard(string str_idcard)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_idcard, @"(^\d{18}$)|(^\d{15}$)");
        }

        /// <summary>
        /// 手机号码验证
        /// </summary>
        /// <param name="phoneNo"></param>
        /// <returns></returns>
        public static bool IsMobile(string phoneNo)
        {
            return Regex.IsMatch(phoneNo, @"^0?(13\d|14[5,7]|15[0-3,5-9]|17[0,6-8]|18\d)\d{8}$");
        }

        /// <summary>
        /// Excel列字母转换成数字
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public static int ExcelColumnNameToIndex(string columnName)
        {
            if (!Regex.IsMatch(columnName.ToUpper(), @"[A-Z]+")) { throw new Exception("invalid parameter"); }

            int index = 0;
            char[] chars = columnName.ToUpper().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                index += ((int)chars[i] - (int)'A' + 1) * (int)Math.Pow(26, chars.Length - i - 1);
            }
            return index - 1;
        }

        /// <summary>
        /// Excel数字转换成列字母
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string ExcelColumnIndexToName(int index)
        {
            if (index < 0) { throw new Exception("invalid parameter"); }

            List<string> chars = new List<string>();
            do
            {
                if (chars.Count > 0) index--;
                chars.Insert(0, ((char)(index % 26 + (int)'A')).ToString());
                index = (int)((index - index % 26) / 26);
            } while (index > 0);

            return String.Join(string.Empty, chars.ToArray());
        }

        /// <summary>
        /// 获取字符串尾部数字
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static int? GetLastNumber(string input)
        {
            var regex = new Regex(@"\d+$");
            var match = regex.Match(input);
            if (!string.IsNullOrEmpty(match.Value))
            {
                int value = Convert.ToInt32(match.Value);
                return value;
            }
            return null;
        }
    }

    /// <summary>
    /// 枚举实体
    /// </summary>
    public class EnumberEntity
    {
        /// <summary>  
        /// 枚举的描述  
        /// </summary>  
        public string Desction { set; get; }

        /// <summary>  
        /// 枚举名称  
        /// </summary>  
        public string EnumName { set; get; }

        /// <summary>  
        /// 枚举对象的值  
        /// </summary>  
        public int EnumValue { set; get; }
    }

    /// <summary>
    /// 操作返回类
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// 操作返回类
        /// </summary>
        public OperationResult()
        {
            status = true;
        }
        /// <summary>
        /// 状态
        /// </summary>
        public bool status { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public string msg { get; set; }
        /// <summary>
        /// 扩展信息
        /// </summary>
        public object extend { get; set; }
    }
}

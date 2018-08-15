using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Shenzhou.Framwork.Models.Enum;

namespace Shenzhou.Framwork
{
    /// <summary>
    /// 数据通讯公共方法
    /// </summary>
    public class DatacomHelper
    {
        /// <summary>
        /// 将byte数组转换成字符串，每个元素按指定进制获得值，每一元素间以指定字符分割
        /// </summary>
        /// <param name="bytes">要转换的byte数组</param>
        /// <param name="separator">分隔符</param>
        /// <param name="length">要转换的长度</param>
        /// <param name="dec">字节转换的进制</param>
        /// <returns></returns>
        public static string BytesToString(byte[] bytes, string separator, int length, Decimals dec = Decimals.Decimalization)
        {
            string strResult = "";
            if (bytes.Count() > 0)
            {
                for (int i = 0; i < (length <= bytes.Count() ? length : bytes.Count()); i++)
                {
                    strResult = strResult + (i > 0 ? separator : "") + Convert.ToString(bytes[i], GetDecimals(dec));
                }
            }
            return strResult.Trim();
        }

        /// <summary>
        /// 数组转换成字符串
        /// </summary>
        /// <typeparam name="T">要转换的数组类型</typeparam>
        /// <param name="t">要转换的数组</param>
        /// <param name="separator">分隔符</param>
        /// <param name="length">要转换的长度，小于等于零表示所有元素，默认为零</param>
        /// <returns></returns>
        public static string ArrayToString<T>(T[] t, string separator, int length = 0)
        {
            string strResult = "";
            if (length <= 0)
            {
                length = t.Count();
            }
            if (t.Count() > 0)
            {
                for (int i = 0; i < (length <= t.Count() ? length : t.Count()); i++)
                {
                    strResult = strResult + (i > 0 ? separator : "") + t[i].ToString();
                }
            }
            return strResult;
        }

        /// <summary>
        ///  字节数组转成字符串
        /// </summary>
        /// <param name="encoding">转换依照的字符编码规范</param>
        /// <param name="byteData">要转换的字节</param>
        /// <param name="index">要开始转换的字节在数组中的位置，从零开始计算</param>
        /// <param name="count">要转换的字节数</param>
        /// <returns></returns>
        public static string BytesToString(Encoding encoding, byte[] byteData, int index, int count)
        {
            return encoding.GetString(byteData, index, count);
        }

        /// <summary>
        /// 将字节数组中每个元素中每位转成整型数组，值为 0 或 1，结果以LittleEndian序排列
        /// </summary>
        /// <param name="data">字节数组</param>
        /// <param name="isBigEndian">字节数组</param>
        /// <returns></returns>
        public static int[] ByteToBit(byte[] data, bool isBigEndian = false)
        {
            byte[] temp = data;
            if (isBigEndian)
            {
                temp = data.Reverse().ToArray();
            }

            int[] result = new int[temp.Length * 8];
            for (int i = 0; i < temp.Length; i++)
            {
                int[] tmp = new int[8];
                tmp[0] = (temp[i] & 0x01) == 0x01 ? 1 : 0;
                tmp[1] = (temp[i] & 0x02) == 0x02 ? 1 : 0;
                tmp[2] = (temp[i] & 0x04) == 0x04 ? 1 : 0;
                tmp[3] = (temp[i] & 0x08) == 0x08 ? 1 : 0;
                tmp[4] = (temp[i] & 0x10) == 0x10 ? 1 : 0;
                tmp[5] = (temp[i] & 0x20) == 0x20 ? 1 : 0;
                tmp[6] = (temp[i] & 0x40) == 0x40 ? 1 : 0;
                tmp[7] = (temp[i] & 0x80) == 0x80 ? 1 : 0;

                tmp.CopyTo(result, i * 8);
            }

            return result;
        }

        /// <summary>
        ///  字符串转成字节数组
        /// </summary>
        /// <param name="encoding">转换依照的字符编码规范</param>
        /// <param name="data">要转换的字符串</param>
        /// <returns></returns>
        public static byte[] StringToBytes(Encoding encoding, string data)
        {
            //Encoding.BigEndianUnicode.ToString(.GetBytes()
            return encoding.GetBytes(data);
        }

        /// <summary>
        ///  整型数字转成字节数组
        /// </summary>
        /// <param name="data">要转换的整型数字</param>
        /// <param name="size">字节数组大小，1~4之间</param>
        /// <param name="isBigEndian">返回的结果是否采用大字节序模式</param>
        /// <returns></returns>
        public static byte[] IntToBytes(int data, int size = 4, bool isBigEndian = false)
        {
            if (size > 4)
            {
                throw new Exception("字节数组大小需在1~4之间");
            }
            if (BitConverter.IsLittleEndian)
            {
                if (isBigEndian)
                {
                    return System.BitConverter.GetBytes(data).Take(size).Reverse().ToArray();
                }
                else
                {
                    return System.BitConverter.GetBytes(data).Take(size).ToArray();
                }
            }
            else
            {
                if (!isBigEndian)
                {
                    return System.BitConverter.GetBytes(data).Take(size).Reverse().ToArray();
                }
                else
                {
                    return System.BitConverter.GetBytes(data).Take(size).ToArray();
                }
            }
        }

        /// <summary>
        ///  字节数组转成整型数字
        /// </summary>
        /// <param name="data">要转换的字节数组</param>
        /// <param name="isBigEndian">转换的字节数组是否采用了大字节序模式</param>
        /// <returns></returns>
        public static int BytesToInt(byte[] data, bool isBigEndian = false)
        {
            if (data.Length > 4)
            {
                throw new Exception("Invalid byte array!");
            }
            byte[] temp = new byte[4];
            if (BitConverter.IsLittleEndian)
            {
                if (isBigEndian)
                {
                    temp = new byte[4 - data.Length].Concat(data).Reverse().ToArray();
                }
                else
                {
                    temp = new byte[4 - data.Length].Concat(data).ToArray();
                }
            }
            else
            {
                if (!isBigEndian)
                {
                    temp = new byte[4 - data.Length].Concat(data).Reverse().ToArray();
                }
                else
                {
                    temp = new byte[4 - data.Length].Concat(data).ToArray();
                }
            }
            return System.BitConverter.ToInt32(temp, 0);
        }

        /// <summary>
        /// 将ASCII码转换为字符
        /// </summary>
        /// <param name="asciiCode"></param>
        /// <returns></returns>
        public static string AscllToString(int asciiCode)
        {
            if (asciiCode >= 0 && asciiCode <= 255)
            {
                ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] byteArray = new byte[] { (byte)asciiCode };
                string strCharacter = asciiEncoding.GetString(byteArray);
                return (strCharacter);
            }
            else
            {
                throw new Exception("ASCII Code is not valid.");
            }
        }

        /// <summary>
        /// 字符转ascii码
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public static int CharToAscii(char character)
        {
            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            int intAsciiCode = (int)asciiEncoding.GetBytes(character.ToString())[0];
            return (intAsciiCode);
        }

        /// <summary>
        /// 字符串转成ascii码字符串，以指定分隔符分割，ASCII码以指定进制显示，默认十进制
        /// </summary>
        /// <param name="character">要转换的字符串</param>
        /// <param name="separator">分隔符</param>
        /// <param name="desc">指定进制</param>
        /// <returns></returns>
        private static string StringToAscii(string character, string separator, Decimals desc = Decimals.Decimalization)
        {
            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
            byte[] intAsciiCode = asciiEncoding.GetBytes(character);
            return BytesToString(intAsciiCode, separator, intAsciiCode.Length, desc);
        }

        /// <summary>
        /// 字符串转16进制字节流
        /// </summary>
        /// <param name="strString"></param>
        /// <returns></returns>
        private static byte[] StringToHexadecimal(string strString)
        {
            byte[] strByte = Encoding.Default.GetBytes(strString);//按照指定编码将string变成字节数组
            string strHexadecimal = "";
            byte[] bytHexadecimal = new byte[1024];
            for (int i = 0; i < strString.Length; i++)//逐字节变为16进制字符
            {
                strHexadecimal += Convert.ToString(int.Parse(strByte[i].ToString()), 16);
            }

            bytHexadecimal = StringHexadecimalToByte(strHexadecimal);

            return bytHexadecimal;
        }

        /// <summary>
        /// 16进制的字符串转为字节流
        /// </summary>
        /// <param name="strHexadecimal">要转换的16进制字符串</param>
        /// <returns></returns>
        private static byte[] StringHexadecimalToByte(string strHexadecimal)
        {
            byte[] bytHexadecimal = new byte[strHexadecimal.Length / 2];
            for (int i = 0; i < strHexadecimal.Length; i += 2)
            {
                bytHexadecimal[i / 2] = (byte)Convert.ToByte(strHexadecimal.Substring(i, 2), 16);
            }
            return bytHexadecimal;
        }

        /// <summary>
        /// 将进制枚举类型转换成相应的整型
        /// </summary>
        /// <param name="dec">要转换的进制枚举类型</param>
        /// <returns></returns>
        public static int GetDecimals(Decimals dec)
        {
            switch (dec)
            {
                case Decimals.Decimalization:
                    return 10;
                case Decimals.Binary:
                    return 2;
                case Decimals.Hexadecimal:
                    return 16;
                default:
                    return 0;
            }
        }


    }
}

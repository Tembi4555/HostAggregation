using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.HelpersService.Helpers
{
    public class ByteArrayToStringHelper
    {
        /// <summary>
        /// Перевод массива byte в массив строк
        /// </summary>
        /// <param name="dataInBytes"></param>
        /// <returns></returns>
        public static string[] ConvertArrStringInArrByte(byte[] dataInBytes)
        {

            if (dataInBytes.Length > 0)
            {
                string str = System.Text.Encoding.Default.GetString(dataInBytes);
                return str?.Split("\r\n");
            }


            return new string[] { };
        }
        public static string ConvertWithDefaultEncoding(byte[] dataInBytes)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw = Stopwatch.StartNew();

            string str = Encoding.Default.GetString(dataInBytes);

            Console.WriteLine($"Декодирование с помощью Default заняло {sw.ElapsedMilliseconds}");
            sw.Stop();
            return str;
        }

        public static string ConvertWithUTF8Encoding(byte[] dataInBytes)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw = Stopwatch.StartNew();

            string str = Encoding.UTF8.GetString(dataInBytes);

            Console.WriteLine($"Декодирование с помощью UTF8 заняло {sw.ElapsedMilliseconds}");
            sw.Stop();
            return str;
        }

        public static string ConvertWithCharsBuffer(byte[] dataInBytes)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw = Stopwatch.StartNew();

            char[] charBuffer = new char[dataInBytes.Length];

            int charsWritten = Encoding.UTF8.GetChars(dataInBytes, 0, dataInBytes.Length,  charBuffer, 0);

            string str = new string(charBuffer, 0, charsWritten);

            Console.WriteLine($"Декодирование с помощью charBuffer заняло {sw.ElapsedMilliseconds}");
            sw.Stop();
            return str;
        }
    }
}

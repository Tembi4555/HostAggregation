using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.FileManagementService.Helpers
{
    public class ByteArrayToStringArray
    {
        /// <summary>
        /// Перевод массива byte в массив строк
        /// </summary>
        /// <param name="dataInBytes"></param>
        /// <returns></returns>
        public static string[] ArrStringFromByte(byte[] dataInBytes)
        {

            if (dataInBytes.Length > 0)
            {
                string str = System.Text.Encoding.Default.GetString(dataInBytes);
                return str?.Split("\r\n");
                //Comment
            }


            return new string[] { };
        }
    }
}

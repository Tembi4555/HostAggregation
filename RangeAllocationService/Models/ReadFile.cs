using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeAllocationService.Models
{
    public class ReadFile
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public DateTime CreationTime { get; set; }

        public Double Size { get; set; }

        public byte[]? DataFromFile { get; set; }
        /*public string[] ArrStringFromByte
        {
            get
            {
                if (DataFromFile.Length > 0)
                {
                    string str = System.Text.Encoding.Default.GetString(DataFromFile);
                    return str?.Split("\r\n");
                }


                return new string[] { };
            }
        }*/
    }
}

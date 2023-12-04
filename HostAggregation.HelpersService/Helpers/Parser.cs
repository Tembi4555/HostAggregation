using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace HostAggregation.HelpersService.Helpers
{
    public class Parser
    {
        public static string[] StringToArrayString(string stringFromParse, char[] separators)
        {
            string[] elements = stringFromParse.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return elements;
        }
    }
}

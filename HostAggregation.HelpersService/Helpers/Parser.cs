using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public static string[] StringToArrayString(string stringFromParse, string separators)
        {
            string[] elements = stringFromParse.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return elements;
        }

        public static List<int> GetIntsFromString(string stringWithNumbers)
        {
            Regex number = new Regex(@"-?\d+");
            List<int> ints = number.Matches(stringWithNumbers)
                       .Cast<Match>()
                       .Select(m => Int32.Parse(m.Value))
                       .ToList();
            return ints;
        }
    }
}

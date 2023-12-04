using FileManagementService.Models;
using HostAggregation.HelpersService.Helpers;
using HostAggregation.RangeAllocationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Range = HostAggregation.RangeAllocationService.Models.Range;

namespace HostAggregation.RangeAllocationService.Helpers
{
    public class Parser
    {
        public static IEnumerable<HostRangeFull> GetListHostRangeFullFromReadFile(IEnumerable<ReadFile> readFiles)
        {
            List<HostRangeFull> res = new List<HostRangeFull>();
            foreach (ReadFile readFile in readFiles)
            {
                string dataFromFile = ByteArrayToStringHelper.ConvertWithDefaultEncoding(readFile.DataFromFile);
                string[] splitStringByEnter = HelpersService.Helpers.Parser.StringToArrayString(dataFromFile, new char[] { '\r', '\n' });

                for(int i = 0; i < splitStringByEnter.Length; i++)
                {
                    string[] splitStrinByComma = HelpersService.Helpers.Parser.StringToArrayString(splitStringByEnter[i], ", ");
                    List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(splitStrinByComma, readFile.ShortName, i);
                    res.AddRange(hostRangeFull);
                }
            }

            return res;
        }

        public static string StringFromHostRangeShort(List<HostRangeShort> hostsRangeShort)
        {

            string result = "";
            foreach(HostRangeShort rangeShort in hostsRangeShort.OrderBy(n => n.HostNumber))
            {
                result = result + rangeShort.HostName + ":";
                foreach(Range range in rangeShort.Ranges)
                {
                    result = result + "[" + range.Start + "," + range.End + "],";
                }
                result = result.TrimEnd(',') + "\r\n";
            }
            return result;
        }

        private static List<HostRangeFull> GetHostRangeFullFromStringArray(string[] arrayFromHostRange, string fileName, int stringNumber)
        {
            List<HostRangeFull> hostsRangeFull = new();
            HostRangeFull hostRangeFull = new HostRangeFull();
            hostRangeFull.FileName = fileName;
            hostRangeFull.NumberStringInFile = stringNumber;
            string[] hosts = GetHostsNameFromStringArr(arrayFromHostRange);

            foreach (string str in arrayFromHostRange)
            {
                if (str?.Trim()?.StartsWith("comment") == true)
                {
                    hostRangeFull.Comment = str.Replace("comment:", "");
                }

                if(str?.Trim()?.StartsWith("header") == true)
                {
                    hostRangeFull.Header = str.Replace("header:", "");
                }

                if (str?.Trim()?.StartsWith("type") == true)
                {
                    if (str.Contains("include"))
                        hostRangeFull.ExInClusionFlag = ExInClusionFlag.Include;

                    if (str.Contains("exclude"))
                        hostRangeFull.ExInClusionFlag = ExInClusionFlag.Exclude;
                }

                if (str?.Trim()?.StartsWith("range") == true)
                {
                    List<int> ints = HelpersService.Helpers.Parser.GetIntsFromString(str);
                    Range range = new Range();
                    if(ints.Count > 1)
                    {
                        range.Start = ints[0];
                        range.End = ints[1];
                    }
                    
                    if(range.IsValid)
                        hostRangeFull.Ranges.Add(range);
                }
            }
            foreach (string host in hosts)
            {
                HostRangeFull hostRangeFullResult = new HostRangeFull()
                {
                    HostName = host,
                    Ranges = hostRangeFull.Ranges,
                    Header = hostRangeFull.Header,
                    Comment = hostRangeFull.Comment,
                    ExInClusionFlag = hostRangeFull.ExInClusionFlag,
                    FileName = hostRangeFull.FileName,
                    NumberStringInFile = hostRangeFull.NumberStringInFile
                };
                hostsRangeFull.Add(hostRangeFullResult);
            }
            return hostsRangeFull;
        }

        private static string[] GetHostsNameFromStringArr(string[] strWithHosts)
        {
            string hostFull = strWithHosts.Where(s => s?.Trim()?.StartsWith("hosts") == true).FirstOrDefault();
            string[] hosts = HelpersService.Helpers.Parser.StringToArrayString(hostFull, new char[] { ',',')','(' });
            return hosts.Where(s => s != "hosts:").ToArray();
        }
    }
}

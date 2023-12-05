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
                string[] splitStringByEnter = HelpersService.Helpers.Parser.StringToArrayString(readFile.DataInString, new char[] { '\r', '\n' });
                List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(splitStringByEnter, readFile.ShortName);
                res.AddRange(hostRangeFull);

            }

            return res;
        }

        public static IEnumerable<HostRangeFull> GetListHostRangeFullFromReadFileWithParallel(IEnumerable<ReadFile> readFiles)
        {
            int procCount = Environment.ProcessorCount;
            int fileCount = 0;
            List<HostRangeFull> res = new List<HostRangeFull>();
            if(readFiles.Count() < procCount)
            {
                foreach (ReadFile readFile in readFiles)
                {
                    string[] splitStringByEnter = HelpersService.Helpers.Parser.StringToArrayString(readFile.DataInString, new char[] { '\r', '\n' });
                    List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(splitStringByEnter, readFile.ShortName);
                    res.AddRange(hostRangeFull);

                }
            }
            else
            {
                Parallel.ForEach(readFiles, (readFile) =>
                {
                    string[] splitStringByEnter = HelpersService.Helpers.Parser.StringToArrayString(readFile.DataInString, new char[] { '\r', '\n' });
                    List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(splitStringByEnter, readFile.ShortName);
                    res.AddRange(hostRangeFull);

                });
                
            }

            return res;
        }

        public static string StringFromHostRangeShort(List<HostRangeShort> hostsRangeShort)
        {

            string result = "";
            foreach(HostRangeShort rangeShort in hostsRangeShort.OrderBy(n => n.HostName))
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
                    ExInClusionFlag = hostRangeFull.ExInClusionFlag,
                    FileName = hostRangeFull.FileName,
                    NumberStringInFile = hostRangeFull.NumberStringInFile
                };
                hostsRangeFull.Add(hostRangeFullResult);
            }
            return hostsRangeFull;
        }

        private static List<HostRangeFull> GetHostRangeFullFromStringArray(string[] arrayFromHostRange, string fileName)
        {
            List<HostRangeFull> hostsRangeFull = new();
            HostRangeFull hostRangeFull = new HostRangeFull();
            hostRangeFull.FileName = fileName;
            for(int i = 0; i < arrayFromHostRange.Length; i++)
            {
                string[] splitStrinByComma = HelpersService.Helpers.Parser.StringToArrayString(arrayFromHostRange[i], ", ");
                hostRangeFull.NumberStringInFile = i;
                string[] hosts = GetHostsNameFromStringArr(splitStrinByComma);

                foreach (string str in splitStrinByComma)
                {
                    if (/*str?.Trim()?.StartsWith("type")*/ str?.ToLower()?.Contains("type") == true)
                    {
                        if (str?.ToLower()?.Contains("include") == true)
                            hostRangeFull.ExInClusionFlag = ExInClusionFlag.Include;

                        if (str?.ToLower()?.Contains("exclude") == true)
                            hostRangeFull.ExInClusionFlag = ExInClusionFlag.Exclude;
                    }

                    if (/*str?.Trim()?.StartsWith("range")*/ str?.ToLower()?.Contains("range") == true)
                    {
                        List<int> ints = HelpersService.Helpers.Parser.GetIntsFromString(str);
                        Range range = new Range();
                        if (ints.Count > 1)
                        {
                            range.Start = ints[0];
                            range.End = ints[1];
                        }

                        if (range.IsValid)
                            hostRangeFull.Ranges.Add(range);
                    }
                }
                foreach (string host in hosts)
                {
                    HostRangeFull hostRangeFullResult = new HostRangeFull()
                    {
                        HostName = host,
                        Ranges = hostRangeFull.Ranges,
                        ExInClusionFlag = hostRangeFull.ExInClusionFlag,
                        FileName = hostRangeFull.FileName,
                        NumberStringInFile = hostRangeFull.NumberStringInFile
                    };
                    hostsRangeFull.Add(hostRangeFullResult);
                }
            }
            
            return hostsRangeFull;
        }


        private static string[] GetHostsNameFromStringArr(string[] strWithHosts)
        {
            //string hostFull = strWithHosts.Where(s => s?.Trim()?.StartsWith("hosts") == true).FirstOrDefault();
            string hostFull = strWithHosts.Where(s => s?.ToLower()?.Contains("hosts") == true).FirstOrDefault();
            string[] hosts = HelpersService.Helpers.Parser.StringToArrayString(hostFull, new char[] { ',',')','(' });
            return hosts.Where(s => s != "hosts:").ToArray();
        }
    }
}

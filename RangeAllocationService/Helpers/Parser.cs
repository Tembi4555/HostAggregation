using FileManagementService.Models;
using HostAggregation.HelpersService.Helpers;
using HostAggregation.RangeAllocationService.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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
        /// <summary>
        /// Получить список 
        /// </summary>
        /// <param name="readFiles"></param>
        /// <returns></returns>
        public static IEnumerable<HostRangesFull> GetListHostRangeFullFromReadFile(IEnumerable<ReadFile> readFiles)
        {
            List<HostRangesFull> res = new List<HostRangesFull>();
            foreach (ReadFile readFile in readFiles)
            {
                string str = readFile.DataInString;
                List<HostRangesFull> hostRangeFull = GetHostRangeFullFromStringArray(str, readFile.ShortName);
                res.AddRange(hostRangeFull);
            }

            return res;
        }

        public static IEnumerable<HostRangesFull> GetListHostRangeFullFromReadFileWithParallel(IEnumerable<ReadFile> readFiles)
        {
            int procCount = Environment.ProcessorCount;
            int fileCount = 0;
            List<HostRangesFull> res = new List<HostRangesFull>();
            if(readFiles.Count() < procCount)
            {
                foreach (ReadFile readFile in readFiles)
                {
                    string str = readFile.DataInString;
                    List<HostRangesFull> hostRangeFull = GetHostRangeFullFromStringArray(str, readFile.ShortName);
                    res.AddRange(hostRangeFull);

                }
            }
            else
            {
                Parallel.ForEach(readFiles, (readFile) =>
                {
                    List<HostRangesFull> hostRangeFull = GetHostRangeFullFromStringArray(readFile.DataInString, readFile.ShortName);
                    lock(res)
                    {
                        res.AddRange(hostRangeFull);
                    }
                });
                
            }

            return res;
        }

        /// <summary>
        /// Перевод модели HostRangesBase в строку для сохранения в файл.
        /// </summary>
        /// <param name="hostsRangeShort"></param>
        /// <returns></returns>
        public static string StringFromHostRangeShort(List<HostRangesBase> hostsRangeShort)
        {
            var hostInGroup = hostsRangeShort.GroupBy(n => n.HostName);
            string result = "";
            foreach(var rangeShort in hostInGroup)
            {
                result = result + rangeShort.Key + ":";
                foreach (var host in rangeShort)
                {
                    result = result + "[" + host.Ranges[0] + "," + host.Ranges[1] + "],";
                }
                result = result.TrimEnd(',') + "\r\n";
            }
            return result;
        }

        private static List<HostRangesFull> GetHostRangeFullFromStringArray(string[] arrayFromHostRange, string fileName, int stringNumber)
        {
            List<HostRangesFull> hostsRangeFull = new();
            ExInClusionFlag flag = ExInClusionFlag.Undefined;
            string[] hosts = GetHostsNameFromStringArr(arrayFromHostRange);
            List<int> ints = new List<int>();

            foreach (string str in arrayFromHostRange)
            {
                if (str?.Trim()?.StartsWith("type") == true)
                {
                    if (str.Contains("include"))
                        flag = ExInClusionFlag.Include;

                    if (str.Contains("exclude"))
                        flag = ExInClusionFlag.Exclude;
                }

                if (str?.Trim()?.StartsWith("range") == true)
                {
                    ints = HelpersService.Helpers.Parser.GetIntsFromString(str);
                }
            }
            foreach (string host in hosts)
            {
                HostRangesFull hostRangeFullResult = new HostRangesFull()
                {
                    HostName = host,
                    
                    ExInClusionFlag = flag,
                    FileName = fileName,
                    NumberStringInFile = stringNumber
                };
                if(ints.Count == 2)
                {
                    hostRangeFullResult.Ranges[0] = ints[0];
                    hostRangeFullResult.Ranges[1] = ints[1];
                }
                hostsRangeFull.Add(hostRangeFullResult);
            }
            return hostsRangeFull;
        }

        private static List<HostRangesFull> GetHostRangeFullFromStringArray(string dataStr, string fileName)
        {
            string[] arrayFromHostRange = dataStr.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            List<HostRangesFull> hostsRangeFull = new();
            
            for (int i = 0; i < arrayFromHostRange.Length; i++)
            {
                ExInClusionFlag flag = ExInClusionFlag.Undefined;
                
                List<int> ints = new List<int>();
                string[] splitStrinByComma = HelpersService.Helpers.Parser.StringToArrayString(arrayFromHostRange[i], ", ");
                string[] hosts = GetHostsNameFromStringArr(splitStrinByComma);
                foreach (string str in splitStrinByComma)
                {
                    if (str?.Trim()?.StartsWith("type") == true)
                    {
                        if (str.Contains("include"))
                            flag = ExInClusionFlag.Include;

                        if (str.Contains("exclude"))
                            flag = ExInClusionFlag.Exclude;
                    }

                    if (str?.Trim()?.StartsWith("range") == true)
                    {
                        ints = HelpersService.Helpers.Parser.GetIntsFromString(str);
                    }
                }
                foreach (string host in hosts)
                {
                    HostRangesFull hostRangeFullResult = new HostRangesFull()
                    {
                        HostName = host,
                        ExInClusionFlag = flag,
                        FileName = fileName,
                        NumberStringInFile = i
                    };
                    if (ints.Count == 2)
                    {
                        hostRangeFullResult.Ranges[0] = ints[0];
                        hostRangeFullResult.Ranges[1] = ints[1];
                    }
                    hostsRangeFull.Add(hostRangeFullResult);
                }
            }

            return hostsRangeFull;
        }

        private static bool IsValidRange(int?[] range)
        {
            if (range[1] < range[0] || range[0] == null || range[1] == null)
                return false;
            else
                return true;
        }

        private static string[] GetHostsNameFromStringArr(string[] strWithHosts)
        {
            string hostFull = strWithHosts.Where(s => s?.ToLower()?.Contains("hosts") == true).FirstOrDefault();
            string[] hosts = HelpersService.Helpers.Parser.StringToArrayString(hostFull, new char[] { ',',')','(' })
                .Where(s => !s.StartsWith("hosts"))
                .ToArray();

            return hosts;
        }
    }
}

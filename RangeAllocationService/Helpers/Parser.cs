﻿using FileManagementService.Models;
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
        public static IEnumerable<HostRangeFull> GetListHostRangeFullFromReadFile(IEnumerable<ReadFile> readFiles)
        {
            List<HostRangeFull> res = new List<HostRangeFull>();
            foreach (ReadFile readFile in readFiles)
            {
                string str = readFile.DataInString;
                List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(str, readFile.ShortName);
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
                    string str = readFile.DataInString;
                    List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(str, readFile.ShortName);
                    res.AddRange(hostRangeFull);

                }
            }
            else
            {
                Parallel.ForEach(readFiles, (readFile) =>
                {
                    List<HostRangeFull> hostRangeFull = GetHostRangeFullFromStringArray(readFile.DataInString, readFile.ShortName);
                    lock(res)
                    {
                        res.AddRange(hostRangeFull);
                    }
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
                foreach(int?[] range in rangeShort.Ranges)
                {
                    result = result + "[" + range[0] + "," + range[1] + "],";
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
                    int?[] range = new int?[2];
                    if(ints.Count > 1)
                    {
                        range[0] = ints[0];
                        range[1] = ints[1];
                    }

                    if (IsValidRange(range))
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
            List<HostRangeFull> hostsRangeFullList = new();
            

            for (int i = 0; i < arrayFromHostRange.Length; i++)
            {
                string[] splitStrinByComma = HelpersService.Helpers.Parser.StringToArrayString(arrayFromHostRange[i], ", ");

                HostRangeFull hostRangeFull = new HostRangeFull();
                hostRangeFull.FileName = fileName;
                hostRangeFull.NumberStringInFile = i;

                string[] hosts = GetHostsNameFromStringArr(splitStrinByComma);

                foreach (string str in splitStrinByComma)
                {
                    if (str?.ToLower()?.Contains("type") == true)
                    {
                        if (str?.ToLower()?.Contains("include") == true)
                            hostRangeFull.ExInClusionFlag = ExInClusionFlag.Include;

                        if (str?.ToLower()?.Contains("exclude") == true)
                            hostRangeFull.ExInClusionFlag = ExInClusionFlag.Exclude;
                    }

                    if (str?.ToLower()?.Contains("range") == true)
                    {
                        List<int> ints = HelpersService.Helpers.Parser.GetIntsFromString(str);
                        int?[] range = new int?[2];
                        if (ints.Count > 1)
                        {
                            range[0] = ints[0];
                            range[1] = ints[1];
                        }

                        if (IsValidRange(range))
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
                    hostsRangeFullList.Add(hostRangeFullResult);
                }
            }
            
            return hostsRangeFullList;
        }

        private static List<HostRangeFull> GetHostRangeFullFromStringArray(string dataStr, string fileName)
        {
            string[] arrayFromHostRange = dataStr.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            List<HostRangeFull> hostsRangeFull = new();
            

            for (int i = 0; i < arrayFromHostRange.Length; i++)
            {
                HostRangeFull hostRangeFull = new HostRangeFull();
                hostRangeFull.FileName = fileName;
                hostRangeFull.NumberStringInFile = i;

                string[] splitStrinByComma = HelpersService.Helpers.Parser.StringToArrayString(arrayFromHostRange[i], ", ");
                
                string[] hosts = GetHostsNameFromStringArr(splitStrinByComma);

                foreach (string str in splitStrinByComma)
                {
                    if (str?.ToLower()?.Contains("type") == true)
                    {
                        if (str?.ToLower()?.Contains("include") == true)
                            hostRangeFull.ExInClusionFlag = ExInClusionFlag.Include;

                        if (str?.ToLower()?.Contains("exclude") == true)
                            hostRangeFull.ExInClusionFlag = ExInClusionFlag.Exclude;
                    }

                    if (str?.ToLower()?.Contains("range") == true)
                    {
                        List<int> ints = HelpersService.Helpers.Parser.GetIntsFromString(str);
                        int?[] range = new int?[2];
                        if (ints.Count > 1)
                        {
                            range[0] = ints[0];
                            range[1] = ints[1];
                        }

                        if (IsValidRange(range))
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
            string[] hosts = HelpersService.Helpers.Parser.StringToArrayString(hostFull, new char[] { ',',')','(' });
            return hosts.Where(s => s != "hosts:").ToArray();
        }
    }
}

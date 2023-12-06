using HostAggregation.RangeAllocationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.RangeAllocationService
{
    public class HostRanking
    {
        public static List<HostRangeShort> GetRankingHost(IEnumerable<HostRangeFull> hostsFromFileList)
        {
            IOrderedEnumerable<IGrouping<string, HostRangeFull>> validAndGroupHost = hostsFromFileList.Where(v => v.IsValid)
                .OrderBy(h => h.HostName)
                .ThenBy(s => s.NumberStringInFile)
                .GroupBy(f => f.FileName)
                .OrderBy(k => k.Key);

            List<HostRangeShort> hostRangeShorts = new List<HostRangeShort>();

            foreach (IGrouping<string, HostRangeFull> groupByFileName in validAndGroupHost)
            {
                
                //var ranges = groupByFileName.SelectMany(h => h.Ranges);
                List<int?[]> includeRangeList = new List<int?[]>();
                List<int?[]> excludeRangeList = new List<int?[]>();
                foreach (HostRangeFull hostRangeFull in groupByFileName)
                {
                    HostRangeShort hostRangeShort = new HostRangeShort();
                    hostRangeShort.HostName = groupByFileName.First().HostName;

                    if (hostRangeFull.ExInClusionFlag == ExInClusionFlag.Include)
                    {
                        if(includeRangeList.Count() < 1)
                        {
                            includeRangeList.Add(hostRangeFull.Ints);
                            continue;
                        }
                            
                        if (RangeAbsorptionInclude(includeRangeList, hostRangeFull.Ints))
                            continue;
                        else
                        {
                            break;
                        }
                    }
                    hostRangeShort.Ranges.AddRange(includeRangeList);
                    hostRangeShorts.Add(hostRangeShort);
                    includeRangeList.Clear();
                    excludeRangeList.Clear();
                }
                
            }

            return hostRangeShorts;
        }

        /// <summary>
        /// Полное вхождение одного интервала в другой
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static bool RangeAbsorptionInclude(List<int?[]> rangeList, int?[] includeRange)
        {
            for(int i = 0; i < rangeList.Count(); i++)
            {
                if (rangeList[i][0] <= includeRange[0] && rangeList[i][1] >= includeRange[1])
                    return true;
                if (rangeList[i][0] >= includeRange[0] && rangeList[i][1] <= includeRange[1])
                {
                    rangeList[i] = includeRange;
                    return true;
                }
            }
            
            return false;
        }
    }
}

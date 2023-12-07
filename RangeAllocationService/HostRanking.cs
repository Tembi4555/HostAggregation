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
        public static List<HostRangesBase> GetRankingHost(IEnumerable<HostRangesFull> hostsFromFileList)
        {
            IOrderedEnumerable<IGrouping<string, HostRangesFull>> validAndGroupHost = hostsFromFileList.Where(v => v.IsValid)
                .OrderBy(h => h.HostName)
                .ThenBy(s => s.NumberStringInFile)
                .GroupBy(f => f.FileName)
                .OrderBy(k => k.Key);

            List<HostRangesBase> hostRangeBases = new List<HostRangesBase>();

            foreach (IGrouping<string, HostRangesFull> groupByFileName in validAndGroupHost)
            {
                var currentHrb = hostRangeBases.Where(h => h.HostName == groupByFileName.Key);
                hostRangeBases.RemoveAll(currentHrb);
                foreach (HostRangesFull hostRangeFull in groupByFileName)
                {
                    if(currentHrb.Count() == 0)
                        hostRangeBases.Add(hostRangeFull);

                    if(hostRangeFull.ExInClusionFlag == ExInClusionFlag.Exclude)
                    {
                        continue;
                    }
                    else if (hostRangeFull.ExInClusionFlag == ExInClusionFlag.Include)
                    {   
                        if (RangeAbsorptionInclude(currentHrb, hostRangeFull.Ranges)) // полное слияние
                            continue;
                        else if(PartialIntersection(currentHrb, hostRangeFull.Ranges))
                            continue;
                        else
                        {
                            currentHrb.Append(hostRangeFull);
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                hostRangeBases.AddRange(currentHrb);
            }

            return hostRangeBases;
        }

        /// <summary>
        /// Полное вхождение одного интервала в другой
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static bool RangeAbsorptionInclude(IEnumerable<HostRangesBase> hostRangeBases, int?[] includeRange)
        {
            foreach(HostRangesBase hrb in hostRangeBases.Where(i => i.ExInClusionFlag == ExInClusionFlag.Include))
            {
                if (hrb.Ranges[0] <= includeRange[0] && hrb.Ranges[1] >= includeRange[1])
                    return true;
                if (hrb.Ranges[0] >= includeRange[0] && hrb.Ranges[1] <= includeRange[1])
                {
                    hrb.Ranges = includeRange;
                    return true;
                }
            }
            
            return false;
        }

        private static bool PartialIntersection(IEnumerable<HostRangesBase> hostRangeBases, int?[] includeRange)
        {
            foreach (HostRangesBase hrb in hostRangeBases.Where(i => i.ExInClusionFlag == ExInClusionFlag.Include))
            {
                if (hrb.Ranges[0] <= includeRange[0] && hrb.Ranges[1] <= includeRange[1])
                {
                    hrb.Ranges[1] = includeRange[1];
                    return true;
                }

                if (hrb.Ranges[0] >= includeRange[0] && hrb.Ranges[1] >= includeRange[1])
                {
                    hrb.Ranges[0] = includeRange[0];
                    return true;
                }
            }

            return false;
        }
    }
}

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

            List<HostRangesBase> includeHostRangeBases = new List<HostRangesBase>();
            List<HostRangesBase> excludeHostRangeBases = new List<HostRangesBase>();

            foreach (IGrouping<string, HostRangesFull> groupByFileName in validAndGroupHost)
            {
                foreach (HostRangesFull hostRangeFull in groupByFileName)
                {
                    if(hostRangeFull.ExInClusionFlag == ExInClusionFlag.Exclude)
                    {
                        if(excludeHostRangeBases.Count() == 0)
                            excludeHostRangeBases.Add(hostRangeFull);
                        RangeAbsorption(excludeHostRangeBases, hostRangeFull);
                    }
                    else if(hostRangeFull.ExInClusionFlag == ExInClusionFlag.Include)
                    {
                        if (includeHostRangeBases.Count() == 0)
                            includeHostRangeBases.Add(hostRangeFull);

                        var listForRemove = RangeAbsorption(includeHostRangeBases, hostRangeFull);

                        if(listForRemove.Count() > 0)
                        {
                            foreach(var hostRange in listForRemove)
                            {
                                includeHostRangeBases.Remove(hostRange);
                            }
                            
                        }
                        
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            return includeHostRangeBases;
        }

        /// <summary>
        /// Полное вхождение одного интервала в другой
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static List<HostRangesBase> RangeAbsorption(List<HostRangesBase> hostRangeBases, HostRangesBase includeRange)
        {
            List<HostRangesBase> listForRemove = new List<HostRangesBase>();
            for(int i = 0; i < hostRangeBases.Count(); i++)
            {
                // Полное включение includeRange в границы hostRangeBases[i].Ranges
                if (hostRangeBases[i].Ranges[0] <= includeRange.Ranges[0] 
                    && hostRangeBases[i].Ranges[1] >= includeRange.Ranges[1]) 
                {
                    break;
                }
                // Полное включение hostRangeBases[i].Ranges в границы includeRange
                if (hostRangeBases[i].Ranges[0] >= includeRange.Ranges[0] 
                    && hostRangeBases[i].Ranges[1] <= includeRange .Ranges[1])
                {
                    listForRemove.Add(hostRangeBases[i]);
                }
            }
            if(listForRemove.Count()> 0)
                hostRangeBases.Add(includeRange);
            return listForRemove;
        }

        /// <summary>
        /// Пересечение нитервалов
        /// </summary>
        /// <param name="hostRangeBases"></param>
        /// <param name="includeRange"></param>
        /// <returns></returns>
        private static bool PartialIntersection(IEnumerable<HostRangesBase> hostRangeBases, int?[] includeRange)
        {
            foreach (HostRangesBase hrb in hostRangeBases.Where(i => i.ExInClusionFlag == ExInClusionFlag.Include))
            {
                if (hrb.Ranges[0] <= includeRange[0] && hrb.Ranges[1] >= includeRange[0] && hrb.Ranges[1] <= includeRange[1])
                {
                    hrb.Ranges[1] = includeRange[1];
                    return true;
                }

                if (hrb.Ranges[0] >= includeRange[0] && hrb.Ranges[0] <= includeRange[1] && hrb.Ranges[1] >= includeRange[1])
                {
                    hrb.Ranges[0] = includeRange[0];
                    return true;
                }
            }

            return false;
        }

        private static void GetInclusionExclusion(IEnumerable<HostRangesBase> inclusions, IEnumerable<HostRangesBase> exclusions)
        {
            /*for (int i = 0; i < exclusions.Count(); i++)
            {
                for(int j = 0; j < inclusions.Count(); j++)
                {
                    if (exclusions.ToList()[i].Ranges[0] <= )
                }
            }*/
            /*foreach(var exc in exclusions)
            {
                foreach(var inc in inclusions)
                {
                    if (exc.Ranges[0] <= inc.Ranges[0] && exc.Ranges[1] >= inc.Ranges[1])
                    {
                        
                    }

                    if (hrb.Ranges[0] >= includeRange[0] && hrb.Ranges[1] <= includeRange[1])
                    {
                        hrb.Ranges = includeRange;
                    }
                }
            }*/
        }
    }
}

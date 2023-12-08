using HostAggregation.RangeAllocationService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                        AggregationInclusionExclusion(includeHostRangeBases, excludeHostRangeBases, excludeHostRangeBases
                            .Last());
                    }
                    else if(hostRangeFull.ExInClusionFlag == ExInClusionFlag.Include)
                    {
                        if (includeHostRangeBases.Count() == 0)
                            includeHostRangeBases.Add(hostRangeFull);
                        RangeAbsorption(includeHostRangeBases, hostRangeFull);
                        AggregationInclusionExclusion(includeHostRangeBases, excludeHostRangeBases, includeHostRangeBases
                            .Last());
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
        private static void RangeAbsorption(List<HostRangesBase> hostRangeBases, HostRangesBase includeRange)
        {
            IEnumerable<HostRangesBase> sortedList = hostRangeBases.Where(h => h.HostName == includeRange.HostName);
            //bool addIncludeRange = false;
            bool noAdd = false;
            foreach (var hostRange in sortedList)
            {
                // Полное включение includeRange в границы hostRangeBases[i].Ranges
                if (hostRange.Ranges[0] <= includeRange.Ranges[0]
                    && hostRange.Ranges[1] >= includeRange.Ranges[1])
                {
                    noAdd = true;
                    break;
                }
                // Полное включение hostRangeBases[i].Ranges в границы includeRange
                else if (hostRange.Ranges[0] >= includeRange.Ranges[0]
                    && hostRange.Ranges[1] <= includeRange.Ranges[1])
                {
                    hostRangeBases.Remove(hostRange);
                }
                else
                    PartialIntersection(hostRangeBases, includeRange);

            }
            if( !noAdd )
            {
                hostRangeBases.Add(includeRange);
            }
        }

        /// <summary>
        /// Пересечение нитервалов
        /// </summary>
        /// <param name="hostRangeBases"></param>
        /// <param name="includeRange"></param>
        /// <returns></returns>
        private static void PartialIntersection(IEnumerable<HostRangesBase> hostRangeBases, HostRangesBase includeRange)
        {
            IEnumerable<HostRangesBase> sortedList = hostRangeBases.Where(h => h.HostName == includeRange.HostName);
            foreach (HostRangesBase hrb in sortedList)
            {
                if (hrb.Ranges[0] <= includeRange.Ranges[0] && hrb.Ranges[1] >= includeRange.Ranges[0] && hrb.Ranges[1] <= includeRange.Ranges[1])
                {
                    hrb.Ranges[1] = includeRange.Ranges[1];
                }

                if (hrb.Ranges[0] >= includeRange.Ranges[0] && hrb.Ranges[0] <= includeRange.Ranges[1] && hrb.Ranges[1] >= includeRange.Ranges[1])
                {
                    hrb.Ranges[0] = includeRange.Ranges[0];
                }
            }
        }

        /// <summary>
        /// Агрегация между интервалами включения, исключения
        /// </summary>
        /// <param name="inclusions"></param>
        /// <param name="exclusions"></param>
        /// <param name="lastElementInList"></param>
        private static void AggregationInclusionExclusion(List<HostRangesBase> inclusions, List<HostRangesBase> exclusions, 
            HostRangesBase lastElementInList)
        {
            /*IEnumerable<HostRangesBase> inclusionsSortedList = inclusions
                .Where(h => h.HostName == lastElementInList.HostName);
            IEnumerable<HostRangesBase> exclusionsSortedList = exclusions
                .Where(h => h.HostName == lastElementInList.HostName);*/

            if(lastElementInList.ExInClusionFlag == ExInClusionFlag.Exclude)
            {
                if(IntervalMatching(inclusions, lastElementInList))
                {
                    exclusions.Remove(lastElementInList);
                }

                List<HostRangesBase> newIntervals = AbsorptionInAggragation(inclusions, lastElementInList);

                if(newIntervals.Count() > 0)
                {
                    exclusions.Remove(lastElementInList);
                    exclusions.AddRange(newIntervals);
                }
            }
            else
            {
                if (IntervalMatching(exclusions, lastElementInList))
                {
                    inclusions.Remove(lastElementInList);
                }

                List<HostRangesBase> newIntervals = AbsorptionInAggragation(exclusions, lastElementInList);

                if (newIntervals.Count() > 0)
                {
                    inclusions.Remove(lastElementInList);
                    inclusions.AddRange(newIntervals);
                }

            }
        }

        /// <summary>
        /// Полное совпадение границ интервалов. Элементы взаимоисключаются из обоих списков
        /// </summary>
        /// <param name="hostRangesBases"></param>
        /// <param name="checkElement"></param>
        /// <returns></returns>
        private static bool IntervalMatching(List<HostRangesBase> hostRangesBases, HostRangesBase checkElement)
        {
            HostRangesBase host = hostRangesBases
                .Where(h => h.HostName == checkElement.HostName && 
                    h.Ranges[0] == checkElement.Ranges[0] && h.Ranges[1] == checkElement.Ranges[1]).FirstOrDefault();
            if (host != null)
            {
                hostRangesBases.Remove(host);
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Проверяемый элемент поглащает один или несколько диазонов из списка.
        /// </summary>
        /// <returns>Возвращается список новых диапазонов после деления</returns>
        private static List<HostRangesBase> AbsorptionInAggragation(List<HostRangesBase> hostRangesBases, HostRangesBase checkElement)
        {
            // checkElement включает в себя один или несколько диапазонов листа HostRangesBase.
            List<HostRangesBase> checkElementMoreThanHostRanges = hostRangesBases
                .Where(h => h.HostName == checkElement.HostName && 
                    h.Ranges[0] >= checkElement.Ranges[0] && h.Ranges[1] <= checkElement.Ranges[1])
                .OrderBy(h => h.Ranges[0])
                .ToList();
            // Диапазон листа HostRangesBase включает в себя checkElement.
            List<HostRangesBase> checkElementLessThanHostRanges = hostRangesBases
                .Where(h => h.HostName == checkElement.HostName && 
                    h.Ranges[0] <= checkElement.Ranges[0] && h.Ranges[1] >= checkElement.Ranges[1])
                .OrderBy(h => h.Ranges[0])
                .ToList();
            List<HostRangesBase> newElements = new();
            //List<int?[]> newRangesList = new List<int?[]>();
            //if(checkElementMoreThanHostRanges.Count() > 0) 
            //{
            //    foreach (HostRangesBase hrb in checkElementMoreThanHostRanges)
            //    {
            //        int?[] ints = new int?[2];

            //        hostRangesBases.Remove(hrb);
            //        if (newRangesList.Count() == 0)
            //        {
            //            if (hrb.Ranges[0] == checkElement.Ranges[0])
            //            {
            //                ints[0] = hrb.Ranges[0];
            //            }
            //            else
            //                ints[0] = checkElement.Ranges[0];
            //        }
            //        else
            //        {

            //        }
            //        newRangesList.Add(ints);
            //    }
            //    foreach(var i in newRangesList)
            //    {
            //        HostRangesBase hrb = new HostRangesBase()
            //        {
            //            HostName = checkElement.HostName,
            //            ExInClusionFlag = checkElement.ExInClusionFlag,
            //            Ranges = i
            //        };
            //        newElements.Add(hrb);
            //    }
            //}

            //if (checkElementLessThanHostRanges.Count() > 0)
            //{
            //    foreach (HostRangesBase hrb in checkElementLessThanHostRanges)
            //    {
            //        int?[] ints = new int?[2];
            //        hostRangesBases.Remove(hrb);
            //        if (hrb.Ranges[0] == checkElement.Ranges[0])
            //        {
            //            ints[0] = hrb.Ranges[0];
            //        }
            //    }
            //}

            return newElements;
        }

    }
}

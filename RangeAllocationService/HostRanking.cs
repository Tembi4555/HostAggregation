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

            List<HostRangesBase> resultList = new List<HostRangesBase>();
            foreach(IGrouping<string, HostRangesFull> groupByFileName in validAndGroupHost)
            {
                foreach (HostRangesFull hostRangeFull in groupByFileName)
                {
                    if(resultList.Count() == 0)
                        resultList.Add(hostRangeFull);

                    bool haveEqualElement = EqualRanges(resultList, hostRangeFull);
                    if(!haveEqualElement)
                    {
                        bool haveIncledesItemChecking = IncludesItemChecking(resultList, hostRangeFull);
                        if(!haveIncledesItemChecking)
                        {
                            bool crossingOrEntering = CrossingOrEnteringIntoCheckingElement(resultList, hostRangeFull);
                            if(!crossingOrEntering)
                                resultList.Add(hostRangeFull);
                        }
                    }
                    
                }
            }

            return resultList;
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
        /// Находит в списке элемент полностью равный проверяемому элементу
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static bool EqualRanges(List<HostRangesBase> hostRangeBases, HostRangesBase checkedElement)
        {
            HostRangesBase equalElement = hostRangeBases
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] == checkedElement.Ranges[0]
                    && h.Ranges[1] == checkedElement.Ranges[1]).FirstOrDefault();

            if(equalElement != null)
            {
                if(equalElement.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    hostRangeBases.Remove(equalElement);
                    // Добавить в журнал, что элемент из строки файла name строки i был удален полностью равным элементом.
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Находит элемент в списке с диапазоном, полностю включающем диапазон проверяемого элемента
        /// </summary>
        /// <returns></returns>
        private static bool IncludesItemChecking(List<HostRangesBase> hostRangeBases, HostRangesBase checkedElement) 
        {
            HostRangesBase incledeElement = hostRangeBases
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] <= checkedElement.Ranges[0]
                    && h.Ranges[1] >= checkedElement.Ranges[1]).FirstOrDefault();

            if (incledeElement != null)
            {
                if (incledeElement.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    hostRangeBases.Remove(incledeElement);
                    hostRangeBases.AddRange(PartitionSegmentIfInclusion(incledeElement, 
                        new List<HostRangesBase> { checkedElement }));
                    // Добавить в журнал, что элемент из файла name строки i был разделен элементами строки i файла name на интервалы.
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Разделение отрезка на сегменты вырезая диапазоны
        /// </summary>
        /// <param name="segmentForPartition"></param>
        /// <param name="separatinSegments"></param>
        /// <returns></returns>
        private static List<HostRangesBase> PartitionSegmentIfInclusion(HostRangesBase segmentForPartition, 
            List<HostRangesBase> separatinSegments)
        {
            List<HostRangesBase> resultSegments = new List<HostRangesBase>();
            int? startPosition = segmentForPartition.Ranges[0];
            int? endPosition = separatinSegments.FirstOrDefault()?.Ranges[0];
            List<HostRangesBase> orderedSepSgments = separatinSegments.OrderBy(r => r.Ranges[0]).ToList();
            for(int i = 0; i < orderedSepSgments.Count(); i++)
            {
                // Продумать включение диапазонов
                if (startPosition != endPosition)
                {
                    HostRangesBase newEl = new HostRangesBase();
                    newEl.HostName = segmentForPartition.HostName;
                    newEl.ExInClusionFlag = segmentForPartition.ExInClusionFlag;
                    newEl.Ranges[0] = startPosition;
                    newEl.Ranges[1] = endPosition;
                    resultSegments.Add(newEl);
                }
                if (i+1 < orderedSepSgments.Count())
                {
                    startPosition = orderedSepSgments[i].Ranges[1];
                    endPosition = orderedSepSgments[i + 1].Ranges[0];
                }
                
            }
            startPosition = separatinSegments.LastOrDefault()?.Ranges[1];
            endPosition = segmentForPartition.Ranges[1];
            if (startPosition != endPosition)
            {
                HostRangesBase lastEl = new HostRangesBase();
                lastEl.HostName = segmentForPartition.HostName;
                lastEl.ExInClusionFlag = segmentForPartition.ExInClusionFlag;
                lastEl.Ranges[0] = startPosition;
                lastEl.Ranges[1] = endPosition;
                resultSegments.Add(lastEl);
            }

            return resultSegments;
        }

        /// <summary>
        /// Нахождение диапазонов, которые полностью входят в проверяемый элемент или пересекают его.
        /// </summary>
        /// <returns></returns>
        private static bool CrossingOrEnteringIntoCheckingElement(List<HostRangesBase> hostRangeBases, 
            HostRangesBase checkedElement)
        {
            HostRangesBase crossingInStart = hostRangeBases.Where(h => h.Ranges[0] < checkedElement.Ranges[0]
                && checkedElement.Ranges[0] <= h.Ranges[1] && h.Ranges[1] <= checkedElement.Ranges[1])
                .FirstOrDefault();

            if (crossingInStart != null)
            {
                hostRangeBases.Remove(crossingInStart);
                if(crossingInStart.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    HostRangesBase[] outherJoin = OutherJoin(crossingInStart, checkedElement);
                    hostRangeBases.Add(outherJoin[0]);
                    checkedElement = outherJoin[1];
                }
                else
                {

                }

            }
            HostRangesBase crossingInEnd = hostRangeBases.Where(h => h.Ranges[0] > checkedElement.Ranges[0]
                && checkedElement.Ranges[1] >= h.Ranges[0] && h.Ranges[1] >= checkedElement.Ranges[1])
                .FirstOrDefault();

            List<HostRangesBase> enteringElements = hostRangeBases
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] >= checkedElement.Ranges[0]
                    && h.Ranges[1] <= checkedElement.Ranges[1]).OrderBy(r => r.Ranges[0]).ToList();

            if (enteringElements.Count() > 0)
            {
                foreach(HostRangesBase element in enteringElements)
                {
                    if(element.ExInClusionFlag != checkedElement.ExInClusionFlag)
                    {

                    }
                }
            }


            if (enteringElements.Count() == 0 && crossingInStart == null && crossingInEnd == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Разделение пересекающихся диапазонов.
        /// </summary>
        /// <param name="leftElement">Элемент стоящий слева в диапазоне пересечения</param>
        /// <param name="rightElement">Элемент стоящий справа в диапазоне пересечения</param>
        /// <returns>index[0] - левый элемент, index[1] - правый эдемент</returns>
        /// <exception cref="NotImplementedException"></exception>
        private static HostRangesBase[] OutherJoin(HostRangesBase leftElement, HostRangesBase rightElement)
        {

            /*if (leftElement.Ranges[1] == rightElement.Ranges[0])
            {

            }*/
            leftElement.Ranges[1] = 
        }

        /// <summary>
        /// Полное вхождение одного интервала в другой
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static void Absorption(List<HostRangesBase> hostRangeBases, HostRangesBase includeRange)
        {
            IEnumerable<HostRangesBase> hrbList = hostRangeBases
                .Where(h => h.HostName == includeRange.HostName && h.Ranges[0] >= includeRange.Ranges[0]
                    && h.Ranges[1] <= includeRange.Ranges[1]);

            if(hrbList.Count() > 0)
            {

            }
            else
            {
                PartialIntersection(hostRangeBases, includeRange);
            }
            //bool addIncludeRange = false;
            /*bool noAdd = false;
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
            if (!noAdd)
            {
                hostRangeBases.Add(includeRange);
            }*/
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
            

            return newElements;
        }

    }
}

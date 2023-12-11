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
        private static List<HostRangesBase> resultList = new List<HostRangesBase>();
        public static List<HostRangesBase> GetRankingHost(IEnumerable<HostRangesFull> hostsFromFileList)
        {
            /*IOrderedEnumerable<IGrouping<string, HostRangesFull>> validAndGroupHost = hostsFromFileList.Where(v => v.IsValid)
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
            }*/
            List<HostRangesFull> validAndGroupHost = hostsFromFileList.Where(v => v.IsValid)
                .OrderBy(h => h.FileName)
                .ThenBy(s => s.NumberStringInFile)
                .ToList();
            int count = 0;
            resultList.Add(validAndGroupHost.FirstOrDefault());
            validAndGroupHost.RemoveAt(0);
            //List<HostRangesBase> resultList = new List<HostRangesBase>();
            foreach (HostRangesFull hostRangeFull in validAndGroupHost)
            {
                count++;

                if (count == 11)
                {
                    var a = count;
                }

                bool haveEqualElement = EqualRanges(/*resultList,*/ hostRangeFull);
                if (!haveEqualElement)
                {
                    bool haveIncledesItemChecking = IncludesItemChecking(/*resultList,*/ hostRangeFull);
                    if (!haveIncledesItemChecking)
                    {
                        bool crossingOrEntering = CrossingOrEnteringIntoCheckingElement(/*resultList,*/ hostRangeFull);
                        if (!crossingOrEntering)
                            resultList.Add(hostRangeFull);
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
        private static bool EqualRanges(/*List<HostRangesBase> hostRangeBases,*/ HostRangesBase checkedElement)
        {
            HostRangesBase equalElement = resultList
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] == checkedElement.Ranges[0]
                    && h.Ranges[1] == checkedElement.Ranges[1]).FirstOrDefault();

            if(equalElement != null)
            {
                if(equalElement.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    resultList.Remove(equalElement);
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
        private static bool IncludesItemChecking(/*List<HostRangesBase> hostRangeBases,*/ HostRangesBase checkedElement) 
        {
            HostRangesBase includeElement = resultList
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] <= checkedElement.Ranges[0]
                    && h.Ranges[1] >= checkedElement.Ranges[1]).FirstOrDefault();

            if (includeElement != null)
            {
                if (includeElement.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    resultList.Remove(includeElement);
                    resultList.AddRange(PartitionSegmentIfInclusion(includeElement, 
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
                    newEl.Ranges[1] = endPosition - 1;
                    resultSegments.Add(newEl);
                }
                if (i+1 < orderedSepSgments.Count())
                {
                    startPosition = orderedSepSgments[i].Ranges[1] + 1;
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
                lastEl.Ranges[0] = startPosition + 1;
                lastEl.Ranges[1] = endPosition;
                resultSegments.Add(lastEl);
            }

            return resultSegments;
        }

        /// <summary>
        /// Нахождение диапазонов, которые полностью входят в проверяемый элемент или пересекают его.
        /// </summary>
        /// <returns></returns>
        private static bool CrossingOrEnteringIntoCheckingElement(/*List<HostRangesBase> hostRangeBases, */
            HostRangesBase checkedElement)
        {
            HostRangesBase crossingInStart = resultList.Where(h => h.HostName == checkedElement.HostName && 
                h.Ranges[0] < checkedElement.Ranges[0]
                && checkedElement.Ranges[0] <= h.Ranges[1] && h.Ranges[1] <= checkedElement.Ranges[1])
                .FirstOrDefault();

            if (crossingInStart != null)
            {
                resultList.Remove(crossingInStart);
                if(crossingInStart.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    HostRangesBase[] outherJoin = OutherJoin(crossingInStart, checkedElement);
                    if (outherJoin[0].Ranges.Length != 0)
                        resultList.Add(outherJoin[0]);
                    
                    checkedElement = outherJoin[1];
                }
                else
                {
                    checkedElement = FullJoin(crossingInStart, checkedElement);
                }

            }
            HostRangesBase crossingInEnd = resultList.Where(h => h.HostName == checkedElement.HostName 
                && h.Ranges[0] > checkedElement.Ranges[0]
                && checkedElement.Ranges[1] >= h.Ranges[0] && h.Ranges[1] >= checkedElement.Ranges[1])
                .FirstOrDefault();

            if (crossingInEnd != null)
            {
                resultList.Remove(crossingInEnd);
                if (crossingInEnd.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    HostRangesBase[] outherJoin = OutherJoin(checkedElement, crossingInEnd);
                    if (outherJoin[1].Ranges.Length != 0)
                        resultList.Add(outherJoin[1]);

                    checkedElement = outherJoin[0];
                }
                else
                {
                    checkedElement = FullJoin(checkedElement, crossingInEnd);
                }
            }

            List<HostRangesBase> enteringElements = resultList
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] >= checkedElement.Ranges[0]
                    && h.Ranges[1] <= checkedElement.Ranges[1]).OrderBy(r => r.Ranges[0]).ToList();

            if (enteringElements.Count() > 0)
            {
                foreach (HostRangesBase element in enteringElements)
                {
                    resultList.Remove(element);
                }
                if (enteringElements.FirstOrDefault(e => e.ExInClusionFlag != checkedElement.ExInClusionFlag)!= null)
                {
                    resultList.AddRange(PartitionSegmentIfInclusion(checkedElement, 
                        enteringElements.Where(e => e.ExInClusionFlag != checkedElement.ExInClusionFlag).ToList()));
                }
                else
                {
                    resultList.Add(checkedElement);
                }
            }
            else
            {
                resultList.Add(checkedElement);
            }

            if (enteringElements.Count() == 0 && crossingInStart == null && crossingInEnd == null)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Полное объединение двух интервалов
        /// </summary>
        /// <param name="crossingInStart"></param>
        /// <param name="checkedElement"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private static HostRangesBase FullJoin(HostRangesBase leftElement, HostRangesBase rightElement)
        {
            leftElement.Ranges[1] = rightElement.Ranges[1];
            return leftElement;
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
            int? temp = rightElement.Ranges[0];
            rightElement.Ranges[0] = leftElement.Ranges[1] + 1;
            leftElement.Ranges[1] = temp - 1;
            
            if (leftElement.Ranges[0] == rightElement.Ranges[0])
            {
                leftElement.Ranges = new int?[] { };
            }
            if (leftElement.Ranges[1] == rightElement.Ranges[1])
            {
                rightElement.Ranges = new int?[] { };
            }
            
            return new HostRangesBase[2] { leftElement, rightElement};
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

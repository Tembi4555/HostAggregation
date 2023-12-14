using FileManagementService.Models;
using HostAggregation.RangeAllocationService.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.RangeAllocationService
{
    public class HostRanking
    {
        private static List<HostRangesBase> _resultList = new List<HostRangesBase>();

        public static List<HostRangesBase> GetRankingHost(IEnumerable<HostRangesFull> hostsFromFileList)
        {
            List<HostRangesFull> validAndGroupHost = hostsFromFileList.Where(v => v.IsValid)
                .OrderBy(h => h.FileName)
                .ThenBy(s => s.NumberStringInFile)
                .ToList();
            int count = 0;
            _resultList.Add(validAndGroupHost.FirstOrDefault());
            if(validAndGroupHost.Count() > 0)
                validAndGroupHost.RemoveAt(0);

            foreach (HostRangesFull hostRangeFull in validAndGroupHost)
            {
                count++;

                //resultList.OrderBy(r => r.Ranges[0]);

                //JoinOrSeparated(hostRangeFull);
                bool haveEqualElement = EqualRanges(hostRangeFull);
                if (!haveEqualElement)
                {
                    bool haveIncledesItemChecking = IncludesItemChecking(hostRangeFull);
                    if (!haveIncledesItemChecking)
                    {
                        CrossingOrEnteringIntoCheckingElement(hostRangeFull);
                    }
                }
            }
            _resultList = JoinNeighboringElement();
            return _resultList;
        }

        public static List<HostRangesBase> GetRankingHost(IEnumerable<ReadFile> readFiles)
        {
            foreach (ReadFile readFile in readFiles)
            {
                string str = readFile.DataInString;

                string[] arrayFromHostRange = str.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < arrayFromHostRange.Length; i++)
                {
                    ExInClusionFlag flag = ExInClusionFlag.Undefined;

                    List<int> ints = Helpers.Parser.GetIntervalFromString(arrayFromHostRange[i]);

                    string[] hosts = Helpers.Parser.GetHostsNameFromString(arrayFromHostRange[i]);

                    if (arrayFromHostRange[i].Contains("include"))
                        flag = ExInClusionFlag.Include;

                    if (arrayFromHostRange[i].Contains("exclude"))
                        flag = ExInClusionFlag.Exclude;

                    foreach (string host in hosts)
                    {
                        HostRangesFull hostRangeFullResult = new HostRangesFull()
                        {
                            HostName = host,
                            ExInClusionFlag = flag,
                            FileName = readFile.ShortName,
                            NumberStringInFile = i
                        };
                        if (ints.Count == 2)
                        {
                            hostRangeFullResult.Ranges[0] = ints[0];
                            hostRangeFullResult.Ranges[1] = ints[1];
                        }
                        if(hostRangeFullResult.IsValid)
                        {
                            bool haveEqualElement = EqualRanges(hostRangeFullResult);
                            if (!haveEqualElement)
                            {
                                bool haveIncledesItemChecking = IncludesItemChecking(hostRangeFullResult);
                                if (!haveIncledesItemChecking)
                                {
                                    CrossingOrEnteringIntoCheckingElement(hostRangeFullResult);
                                }
                            }
                        }
                        else
                        {
                            LogService.Log.AddError($"{readFile.ShortName};{i};{hostRangeFullResult.InValidMessage}");
                        }
                    }
                }
            }
            _resultList = JoinNeighboringElement();
            return _resultList;
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

        private static void JoinOrSeparated(HostRangesBase checkedElement)
        {
            List<HostRangesBase> listForRemove = new List<HostRangesBase>();
            List<HostRangesBase> listForAdd = new List<HostRangesBase>();
            for (int i = 0; i < _resultList.Count(); i++)
            {
                if (_resultList[i].HostName == checkedElement.HostName 
                    && _resultList[i].Ranges[0] == checkedElement.Ranges[0]
                    && _resultList[i].Ranges[1] == checkedElement.Ranges[1])
                {
                    if (_resultList[i].ExInClusionFlag != checkedElement.ExInClusionFlag)
                    {
                        _resultList.Remove(_resultList[i]);
                        // Добавить в журнал, что элемент из строки файла name строки i был удален полностью равным элементом.
                    }
                    break;
                }


                if (_resultList[i].HostName == checkedElement.HostName 
                    && _resultList[i].Ranges[0] <= checkedElement.Ranges[0]
                    && _resultList[i].Ranges[1] >= checkedElement.Ranges[1])
                {
                    if (_resultList[i].ExInClusionFlag != checkedElement.ExInClusionFlag)
                    {
                        _resultList.Remove(_resultList[i]);
                        _resultList.AddRange(PartitionSegmentIfInclusion(_resultList[i], 
                            new List<HostRangesBase> { checkedElement }));
                        // Добавить в журнал, что элемент из файла name строки i был разделен элементами строки i файла name на интервалы.
                    }
                    break;
                }

                if (_resultList[i].HostName == checkedElement.HostName 
                    && _resultList[i].Ranges[0] < checkedElement.Ranges[0]
                    && checkedElement.Ranges[0] <= _resultList[i].Ranges[1] 
                    && _resultList[i].Ranges[1] <= checkedElement.Ranges[1])
                {
                    //resultList.Remove(resultList[i]);
                    listForRemove.Add(_resultList[i]);
                    if (_resultList[i].ExInClusionFlag != checkedElement.ExInClusionFlag)
                    {
                        HostRangesBase[] outherJoin = OutherJoin(_resultList[i], checkedElement);
                        if (outherJoin[0].Ranges.Length != 0)
                            listForAdd.Add(outherJoin[0]);
                            //resultList.Add(outherJoin[0]);

                        checkedElement = outherJoin[1];
                    }
                    else
                    {
                        checkedElement = FullJoin(_resultList[i], checkedElement);
                    }

                }

                if (_resultList[i].HostName == checkedElement.HostName
                    && _resultList[i].Ranges[0] > checkedElement.Ranges[0]
                    && checkedElement.Ranges[1] >= _resultList[i].Ranges[0] 
                    && _resultList[i].Ranges[1] >= checkedElement.Ranges[1])
                {
                    //resultList.Remove(resultList[i]);
                    listForRemove.Add(_resultList[i]);
                    if (_resultList[i].ExInClusionFlag != checkedElement.ExInClusionFlag)
                    {
                        HostRangesBase[] outherJoin = OutherJoin(checkedElement, _resultList[i]);
                        if (outherJoin[1].Ranges.Length != 0)
                            listForAdd.Add(outherJoin[1]);
                            //resultList.Add(outherJoin[1]);

                        checkedElement = outherJoin[0];
                    }
                    else
                    {
                        checkedElement = FullJoin(checkedElement, _resultList[i]);
                    }
                }

                if (_resultList[i].HostName == checkedElement.HostName 
                    && _resultList[i].Ranges[0] >= checkedElement.Ranges[0]
                    && _resultList[i].Ranges[1] <= checkedElement.Ranges[1])
                {
                    //resultList.Remove(resultList[i]);
                    listForRemove.Add(_resultList[i]);
                    
                    if (_resultList[i].ExInClusionFlag != checkedElement.ExInClusionFlag)
                    {
                        List<HostRangesBase> partitionList = PartitionSegmentIfInclusion(checkedElement,
                            new List<HostRangesBase> { _resultList[i] });
                        checkedElement = partitionList?.LastOrDefault();
                        partitionList.Remove(checkedElement);
                        listForAdd.AddRange(partitionList);
                    }
                    else
                    {
                        listForAdd.Add(checkedElement);
                    }
                }
                /*else
                {
                    //resultList.Add(checkedElement);
                    listForAdd.Add(checkedElement);
                }*/
            }
            foreach(HostRangesBase r in listForRemove)
            {
                _resultList.Remove(r);
            }

            if (listForAdd.Count() == 0)
                listForAdd.Add(checkedElement);

            _resultList.AddRange(listForAdd);
        }

        /// <summary>
        /// Находит в списке элемент полностью равный проверяемому элементу
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        private static bool EqualRanges(HostRangesFull checkedElement)
        {
            HostRangesBase equalElement = _resultList
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] == checkedElement.Ranges[0]
                    && h.Ranges[1] == checkedElement.Ranges[1]).FirstOrDefault();

            if(equalElement != null)
            {
                if(equalElement.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    _resultList.Remove(equalElement);
                    // Добавить в журнал, что элемент из строки файла name строки i был удален полностью равным элементом.
                    LogService.Log.AddInfo($"{equalElement.HostName}; Отрезок {equalElement.Ranges[0]},{equalElement.Ranges[1]} " +
                        $"был удален отрезок {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"из файла {checkedElement.FileName}, строка {checkedElement.NumberStringInFile}");
                }
                else
                {
                    LogService.Log.AddInfo($"{equalElement.HostName}; Отрезок {equalElement.Ranges[0]},{equalElement.Ranges[1]} " +
                        $"был пропущен, так как равен отрезку {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"из файла {checkedElement.FileName}, строка {checkedElement.NumberStringInFile}");
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
        private static bool IncludesItemChecking(HostRangesFull checkedElement) 
        {
            HostRangesBase includeElement = _resultList
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] <= checkedElement.Ranges[0]
                    && h.Ranges[1] >= checkedElement.Ranges[1]).FirstOrDefault();

            if (includeElement != null)
            {
                if (includeElement.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    _resultList.Remove(includeElement);
                    List<HostRangesBase> partition = PartitionSegmentIfInclusion(includeElement, 
                        new List<HostRangesBase> { checkedElement });
                    _resultList.AddRange(partition);
                    string ranges = String.Join(",",partition.SelectMany(h => h.Ranges).ToList());

                    // Добавить в журнал, что элемент из файла name строки i был разделен элементами строки i файла name на интервалы.
                    LogService.Log.AddInfo($"{checkedElement.HostName}; отрезок {includeElement.Ranges[0]},{includeElement.Ranges[1]} " +
                        $"разделен отрезком {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"из файла {checkedElement.FileName}, строка {checkedElement.NumberStringInFile}" +
                        $"на отрезки {ranges}");
                }
                else
                {
                    LogService.Log.AddInfo($"{checkedElement.HostName}; Отрезок {includeElement.Ranges[0]},{includeElement.Ranges[1]};" +
                        $"Включает отрезки {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"из файла {checkedElement.FileName}, строка {checkedElement.NumberStringInFile}");
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
        /// Объединение диапазонов типа 10,15 и 15,20
        /// </summary>
        private static List<HostRangesBase> JoinNeighboringElement()
        {
            List<HostRangesBase> orderResultList = _resultList
                .OrderBy(s => s.HostName)
                .ThenBy(s => s.Ranges[0])
                .Where(h => h.ExInClusionFlag == ExInClusionFlag.Include).ToList();

            for(int i = 0; i < orderResultList.Count() - 1; i++)
            {
                if (orderResultList[i].Ranges[1] + 1 == orderResultList[i + 1].Ranges[0])
                {
                    HostRangesBase joinEl = FullJoin(orderResultList[i], orderResultList[i + 1]);
                    orderResultList.RemoveRange(i, 2);

                    orderResultList.Insert(i, joinEl);
                }
                
            }
            return orderResultList;
        }

        /// <summary>
        /// Нахождение диапазонов, которые полностью входят в проверяемый элемент или пересекают его.
        /// </summary>
        /// <returns></returns>
        private static void CrossingOrEnteringIntoCheckingElement(HostRangesBase checkedElement)
        {
            HostRangesBase crossingInStart = _resultList.Where(h => h.HostName == checkedElement.HostName && 
                h.Ranges[0] < checkedElement.Ranges[0]
                && checkedElement.Ranges[0] <= h.Ranges[1] && h.Ranges[1] <= checkedElement.Ranges[1])
                .FirstOrDefault();

            if (crossingInStart != null)
            {
                _resultList.Remove(crossingInStart);
                if(crossingInStart.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    HostRangesBase[] outherJoin = OutherJoin(crossingInStart, checkedElement);
                    if (outherJoin[0].Ranges.Length != 0)
                        _resultList.Add(outherJoin[0]);
                    
                    checkedElement = outherJoin[1];
                    LogService.Log.AddInfo($"{checkedElement.HostName}; Пересечение отрезка {crossingInStart.Ranges[0]},{crossingInStart.Ranges[1]};" +
                        $"с {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"Разделены на два отрезка {outherJoin[0].Ranges[0]},{outherJoin[0].Ranges[1]}" +
                        $" и {outherJoin[1].Ranges[0]},{outherJoin[1].Ranges[1]}");
                }
                else
                {
                    checkedElement = FullJoin(crossingInStart, checkedElement);
                    LogService.Log.AddInfo($"{checkedElement.HostName}; Пересечение отрезка {crossingInStart.Ranges[0]},{crossingInStart.Ranges[1]};" +
                        $"с {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"Слияние в общий отрезок {checkedElement.Ranges[0]},{checkedElement.Ranges[1]}");
                }

            }
            HostRangesBase crossingInEnd = _resultList.Where(h => h.HostName == checkedElement.HostName 
                && h.Ranges[0] > checkedElement.Ranges[0]
                && checkedElement.Ranges[1] >= h.Ranges[0] && h.Ranges[1] >= checkedElement.Ranges[1])
                .FirstOrDefault();

            if (crossingInEnd != null)
            {
                _resultList.Remove(crossingInEnd);
                if (crossingInEnd.ExInClusionFlag != checkedElement.ExInClusionFlag)
                {
                    HostRangesBase[] outherJoin = OutherJoin(checkedElement, crossingInEnd);
                    if (outherJoin[1].Ranges.Length != 0)
                        _resultList.Add(outherJoin[1]);

                    checkedElement = outherJoin[0];
                    LogService.Log.AddInfo($"{checkedElement.HostName}; Пересечение отрезка {crossingInEnd.Ranges[0]},{crossingInEnd.Ranges[1]};" +
                        $"с {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"Разделены на два отрезка {outherJoin[0].Ranges[0]},{outherJoin[0].Ranges[1]}" +
                        $" и {outherJoin[1].Ranges[0]},{outherJoin[1].Ranges[1]}");
                }
                else
                {
                    checkedElement = FullJoin(checkedElement, crossingInEnd);
                    LogService.Log.AddInfo($"{checkedElement.HostName}; Пересечение отрезка {crossingInEnd.Ranges[0]},{crossingInEnd.Ranges[1]};" +
                        $"с {checkedElement.Ranges[0]},{checkedElement.Ranges[1]} " +
                        $"Слияние в общий отрезок {checkedElement.Ranges[0]},{checkedElement.Ranges[1]}");

                }
            }

            List<HostRangesBase> enteringElements = _resultList
                .Where(h => h.HostName == checkedElement.HostName && h.Ranges[0] >= checkedElement.Ranges[0]
                    && h.Ranges[1] <= checkedElement.Ranges[1]).OrderBy(r => r.Ranges[0]).ToList();

            if (enteringElements.Count() > 0)
            {
                foreach (HostRangesBase element in enteringElements)
                {
                    _resultList.Remove(element);
                }
                if (enteringElements.FirstOrDefault(e => e.ExInClusionFlag != checkedElement.ExInClusionFlag)!= null)
                {
                    List<HostRangesBase> listForSegmentation = enteringElements
                        .Where(e => e.ExInClusionFlag != checkedElement.ExInClusionFlag).ToList();

                    string segmentsString = String.Join(",", listForSegmentation.SelectMany(h => h.Ranges).ToList());

                    List<HostRangesBase> partition = PartitionSegmentIfInclusion(checkedElement, listForSegmentation);
                    _resultList.AddRange(partition);

                    string ranges = String.Join(",", partition.SelectMany(h => h.Ranges).ToList());

                    // Добавить в журнал, что элемент из файла name строки i был разделен элементами строки i файла name на интервалы.
                    LogService.Log.AddInfo($"{checkedElement.HostName}; Отрезок {checkedElement.Ranges[0]},{checkedElement.Ranges[1]};" +
                        $"Разделен отрезками {segmentsString} " +
                        $"на отрезки {ranges}");
                }
                else
                {
                    _resultList.Add(checkedElement);
                }
            }
            else
            {
                _resultList.Add(checkedElement);
            }
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

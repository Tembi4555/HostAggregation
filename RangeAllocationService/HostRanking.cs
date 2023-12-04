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
        public static List<HostRangeShort> GetRankingHost(IEnumerable<HostRangeFull> hostRangeFulls)
        {
            IEnumerable<IGrouping<string, HostRangeFull>> groupByHost = hostRangeFulls.GroupBy(h => h.HostName);
            List<HostRangeShort> shorts = new List<HostRangeShort>();

            foreach (var hosts in groupByHost)
            {
                bool hostNumb = int.TryParse(hosts.Key?.Replace("host", ""), out int hostNumber);
                HostRangeShort hostRangeShort = new HostRangeShort()
                {
                    HostName = hosts.Key,
                    HostNumber = hostNumber
                };

                var orderHost = hosts.Where(s => s.IsValid).OrderBy(h => h.NumberStringInFile);

                foreach (var host in orderHost)
                {
                    hostRangeShort.Ranges.AddRange(host.Ranges);
                }
                shorts.Add(hostRangeShort);
            }
            return shorts;
        }
    }
}

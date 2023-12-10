using HostAggregation.RangeAllocationService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.RangeAllocationService
{
    public static class Aggregation
    {
        private static List<HostRangesBase> resultList = new List<HostRangesBase>();



        /*function joinSegmentsWithOrder(inputSegments)
        {
            const candidateFirst = inputSegments.shift();
            const candidateSecond = inputSegments.shift();
            let candidates = [candidateFirst, candidateSecond];
            const isEveryExclude = (item) => item.type === "excl";
            if (!candidates.every(isEveryExclude))
            {
                candidates = joinSegments(candidates);
            }
            while (inputSegments.length)
            {
                let currentCundidate = inputSegments.shift();
                candidates = [...candidates, currentCundidate];
                if (!candidates.every(isEveryExclude))
                {
                    candidates = joinSegments(candidates);
                }
                // console.log(candidates);
            }
            if (candidates.every(isEveryExclude))
            {
                return [];
            }
            return candidates;
        }*/
    }
}


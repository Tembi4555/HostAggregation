using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeAllocationService.Models
{
    public class HostRangeFull : HostRangesBase
    {
        public string Comment { get; set; }
        public string Header { get; set; }
        public ExInClusionFlag ExInClusionFlag { get; set; }

    }

    public enum ExInClusionFlag
    {
        Include,
        Exclude
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeAllocationService.Models
{
    public abstract class HostRangesBase
    {
        public string HostName { get; set; }
        public IEnumerable<Range> Ranges { get; set; }
    }
}

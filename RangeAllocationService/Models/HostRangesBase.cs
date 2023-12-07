using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.RangeAllocationService.Models
{
    /// <summary>
    /// Базовый класс для наименования владельца и списка всех его диапазонов
    /// </summary>
    public class HostRangesBase
    {
        public string HostName { get; set; }
        //public List<Range> Ranges { get; set; } = new List<Range>();
        public int?[] Ranges { get; set; } = new int?[2];

        public ExInClusionFlag ExInClusionFlag { get; set; }

    }

    public enum ExInClusionFlag
    {
        Include,
        Exclude,
        Undefined
    }
}

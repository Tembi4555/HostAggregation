using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HostAggregation.RangeAllocationService.Models
{
    /// <summary>
    /// Базовый класс для наименования владельца и списка всех его диапазонов
    /// </summary>
    public class HostRangesBase //: IComparable
    {
        public string HostName { get; set; }
        //public List<Range> Ranges { get; set; } = new List<Range>();
        public int?[] Ranges { get; set; } = new int?[2];

        public ExInClusionFlag ExInClusionFlag { get; set; }

        /*public int CompareTo(object? obj)
        {
            if ((obj == null) || (!(obj is HostRangesBase)) || Ranges[0] == null || Ranges[1] == null)
                return 0;
            if(Ranges[0] > ((HostRangesBase)obj).Ranges[0])
                return -1;
            if (Ranges[0] < ((HostRangesBase)obj).Ranges[0])
                return 1;
            else
                return 0;
        }*/
    }

    public enum ExInClusionFlag
    {
        Include,
        Exclude,
        Undefined
    }
}

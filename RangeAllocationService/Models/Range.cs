using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.RangeAllocationService.Models
{
    /// <summary>
    /// Модель для диапазонов
    /// </summary>
    public class Range
    {
        public int? Start { get; set; }
        public int? End { get; set; }
        public bool IsValid 
        {
            get 
            {
                if (End < Start || End == null || Start == null)
                    return false;
                else
                    return true;
            }
        }
    }
}

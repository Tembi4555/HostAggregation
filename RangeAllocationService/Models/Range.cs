using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeAllocationService.Models
{
    public class Range
    {
        public int Start { get; set; }
        public int End { get; set; }
        public bool IsValid 
        {
            get 
            { 
                if(End < Start)
                    return false;
                else
                    return true;
            }
        }
    }
}

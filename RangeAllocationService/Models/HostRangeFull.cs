using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostAggregation.RangeAllocationService.Models
{
    /// <summary>
    /// Полная модель 
    /// </summary>
    public class HostRangeFull : HostRangesBase
    {
        public string Comment { get; set; }
        public string Header { get; set; }
        public ExInClusionFlag ExInClusionFlag { get; set; }
        public int NumberStringInFile { get; set; }
        public string FileName { get; set; }
        public bool IsValid 
        { 
            get
            {
                if(string.IsNullOrEmpty(HostName))
                    return false;
                if(string.IsNullOrEmpty(Comment))
                    return false;
                if(string.IsNullOrEmpty(Header))
                    return false;
                if(string.IsNullOrEmpty(FileName))
                    return false;
                return true;
            }
        }
    }

    public enum ExInClusionFlag
    {
        Include,
        Exclude
    }
}

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
        private string _invalidMessage = "";
        //public override List<int?[]> Ranges { get; set; }
        public ExInClusionFlag ExInClusionFlag { get; set; }
        public int NumberStringInFile { get; set; }
        public string FileName { get; set; }
        public bool IsValid 
        { 
            get
            {
                if (string.IsNullOrEmpty(HostName))
                {
                    _invalidMessage = "Отсутствует имя хоста";
                    return false;
                }   
                if (string.IsNullOrEmpty(FileName))
                {
                    _invalidMessage = "Отсутствует имя файла из которого была получена строка";
                    return false;
                }
                if(ExInClusionFlag == ExInClusionFlag.Undefined)
                {
                    _invalidMessage = "Отсутствует тип включения или исключения диапазона";
                    return false;
                }
                if (Ranges[1] < Ranges[0] || Ranges[0] == null || Ranges[1] == null)
                {
                    _invalidMessage = "Отсутствуют или некорректные диапазоны разбиения";
                    return false;
                }    
                    

                return true;
            }
        }

        public string InValidMessage 
        { 
            get { return _invalidMessage; }
        }

    }

    public enum ExInClusionFlag
    {
        Include,
        Exclude,
        Undefined
    }
}

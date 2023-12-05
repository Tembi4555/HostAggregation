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
        public string Comment { get; set; }
        public string Header { get; set; }
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
                if(string.IsNullOrEmpty(Comment))
                {
                    _invalidMessage = "Отсутствует комментарий";
                    return false;
                }
                if (string.IsNullOrEmpty(Header))
                {
                    _invalidMessage = "Отсутствует заголовок";
                    return false;
                }
                if (string.IsNullOrEmpty(FileName))
                {
                    _invalidMessage = "Отсутствует имя файла из которого была получена строка";
                    return false;
                }
                if(Ranges.Count < 1)
                {
                    _invalidMessage = "Отсутствуют диапазоны разбиения";
                    return false;
                }
                if(HostNumber == null)
                {
                    _invalidMessage = "Отсутствует номер хоста";
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
        Exclude
    }
}

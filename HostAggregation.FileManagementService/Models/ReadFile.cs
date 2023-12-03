using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileManagementService.Models
{
    public class ReadFile
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public DateTime CreationTime { get; set; }

        public Double Size { get; set; }

        public byte[]? DataFromFile { get; set; }
        
    }
}

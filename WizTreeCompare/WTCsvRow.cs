using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public class WTCsvRow
    {
        [Name("File Name")]
        public string FileName { get; set; }

        public long Size { get; set; }
        public long Allocated { get; set; }

        //[Format("yyyy-MM-dd HH:mm:ss")] //Lol, this changes based on your system settings... why I have no idea
        public DateTime? Modified { get; set; }

        public long? Attributes { get; set; }
    }
}

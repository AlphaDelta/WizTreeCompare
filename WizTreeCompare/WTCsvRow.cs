using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WizTreeCompare
{
    public class WTCsvRow
    {
        const int FILE_ATTRIBUTE_DIRECTORY = 0x00000010; //https://learn.microsoft.com/en-us/windows/win32/fileio/file-attribute-constants#FILE_ATTRIBUTE_DIRECTORY

        [Name("File Name")]
        public string FileName { get; set; }

        public long Size { get; set; }
        public long Allocated { get; set; }

        //[Format("yyyy-MM-dd HH:mm:ss")] //Lol, this changes based on your system settings... why I have no idea
        public DateTime? Modified { get; set; }

        public long? Attributes { get; set; }

        [Ignore]
        private bool? _IsDirectory = null;
        [Ignore]
        public bool IsDirectory => _IsDirectory ??=
                this.FileName.EndsWith(Path.DirectorySeparatorChar)
                || this.FileName.EndsWith(Path.AltDirectorySeparatorChar)
                || (this.Attributes & FILE_ATTRIBUTE_DIRECTORY) > 0;
    }
}

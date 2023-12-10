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

        [Index(0)]
        public string FileName { get; set; }

        [Index(1)]
        public long Size { get; set; }
        [Index(2)]
        public long Allocated { get; set; }

        //[Format("yyyy-MM-dd HH:mm:ss")] //Lol, this changes based on your system settings... why I have no idea
        [Index(3)]
        public DateTime? Modified { get; set; }

        [Index(4)]
        public long? Attributes { get; set; }

        internal const int HEADER_SIZE = 5;

        public static bool IsRowJunk(string v)
            => v.Contains(" wiztree ") && v.Contains('(');

        [Ignore]
        private bool? _IsDirectory = null;
        [Ignore]
        public bool IsDirectory => _IsDirectory ??=
                this.FileName.EndsWith(Path.DirectorySeparatorChar)
                || this.FileName.EndsWith(Path.AltDirectorySeparatorChar)
                || (this.Attributes & FILE_ATTRIBUTE_DIRECTORY) > 0;
    }
}

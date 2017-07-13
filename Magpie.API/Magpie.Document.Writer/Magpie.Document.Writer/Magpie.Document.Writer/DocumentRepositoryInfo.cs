using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Document.Writer
{
    public class DocumentRepositoryInfo
    {
        public string FileName { get; set; }

        public string FilePath { get; set; }

        public int WorkingSetId { get; set; }

        public Dictionary<string, string> PolicyGenerationPropertiesEntries { get; set; }
}


    public class DocumentRepositoryFile
    {
        public string DocumentTitle { get; set; }
        public string SaveError { get; set; }
    }
}

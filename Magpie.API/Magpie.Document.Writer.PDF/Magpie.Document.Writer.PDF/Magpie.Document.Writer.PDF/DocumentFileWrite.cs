using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Document.Writer.PDF
{
    public class DocumentFileWrite
    {
        internal void WriteToFile(byte[] fileByteArray, string fileName)
        {
            Stream streamDocument = new MemoryStream(fileByteArray);
            streamDocument.Position = 0;

            using (FileStream fileStream = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                streamDocument.CopyTo(fileStream);
                fileStream.Flush();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.Document.Writer
{
    public interface IDocumentWriter
    {        
        void  WriteDocument(Stream textDocument, DocumentRepositoryInfo documentRepositoryInfo);

        void WriteDocument(string textDocument, DocumentRepositoryInfo documentRepositoryInfo);

        void WriteDocument(Stream textDocument, string fileName);

        void WriteDocument(string textDocument, string fileName);
    }
}

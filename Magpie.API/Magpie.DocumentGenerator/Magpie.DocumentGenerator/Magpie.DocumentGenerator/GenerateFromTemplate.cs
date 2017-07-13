using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Magpie.Document.Writer;
using Magpie.Document.Writer.PDF;
using Magpie.Repository;
using Magpie.Model;

namespace Magpie.DocumentGenerator
{
    public class GenerateFromTemplate
    {
        private Dictionary<string, string> replacementDictionary = new Dictionary<string, string>();
        
        public void GenerateDocument(int documentId, string outputFileName)
        {
        }
        
        public List<DocumentRepositoryFile> GenerateDocument(string sourceDocumentDirectory, DocumentRepositoryInfo documentRepositoryInfo, AwsConfigInfo awsConfigInfo)
        {
            replacementDictionary = documentRepositoryInfo.PolicyGenerationPropertiesEntries;

            var workingSetDocument = new WorkingSetDocument();

            var documentRepositoryFileList = new List<DocumentRepositoryFile>();
            
            foreach (string fileName in Directory.EnumerateFiles(sourceDocumentDirectory, "*.txt"))
            {
                var documentByteArray = ConvertDocument(fileName);

                MemoryStream stream = new MemoryStream();
                stream.Write(documentByteArray, 0, documentByteArray.Length);

                if (awsConfigInfo == null)
                {
                    WritePdfToDisk(sourceDocumentDirectory, fileName, documentRepositoryInfo.FilePath, stream);

                     // WriteFileToDisk(sourceDocumentDirectory, fileName, stream);
                }
                else
                {
                    documentRepositoryInfo.FileName = Path.GetFileName(fileName);
                    WritePdfToAws(documentRepositoryInfo, awsConfigInfo, stream);

                    var documentRepositoryFile = new DocumentRepositoryFile();
                    documentRepositoryFile.DocumentTitle = documentRepositoryInfo.FileName;

                    documentRepositoryFileList.Add(documentRepositoryFile);
                }
            }

            return documentRepositoryFileList;
        }

        //private void WriteFileToDisk(string documentDirectory, string fileName, MemoryStream documentStream)
        //{
        //    documentStream.Position = 0;

        //    string newFileName = $"{documentDirectory}\\new\\{Path.GetFileName(fileName)}";

        //    FileStream fileStream = new FileStream(newFileName, FileMode.Create, FileAccess.Write);

        //    documentStream.WriteTo(fileStream);

        //    fileStream.Close();
        //    documentStream.Close();
        //}


        private void WritePdfToDisk(string documentDirectory, string fileName, string businessControlProfileName, MemoryStream documentStream)
        {
            documentStream.Position = 0;

            string ouputFileName = $"{documentDirectory}\\{businessControlProfileName}\\{Path.GetFileName(fileName)}";
            ouputFileName = ouputFileName.Replace(".txt", ".pdf");

            IDocumentWriter pdfWrite = new PDFWrite();

            pdfWrite.WriteDocument(documentStream, ouputFileName);
        }


        private void WritePdfToAws(DocumentRepositoryInfo documentRepositoryInfo, AwsConfigInfo awsConfigInfo, MemoryStream documentStream)
        {
            documentStream.Position = 0;

            IDocumentWriter pdfWrite = new PDFWrite(awsConfigInfo.AccessKey, awsConfigInfo.SecretKey, awsConfigInfo.BucketName);
            documentRepositoryInfo.FileName = documentRepositoryInfo.FileName.Replace(".txt", ".pdf");

            pdfWrite.WriteDocument(documentStream, documentRepositoryInfo);
        }


        private byte[] ConvertDocument(string fileName)
        {
            string line;
            MemoryStream memoryStream = null;

            List<string> allItems = new List<string>();

            using (memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream))
                {
                    StreamReader file = new StreamReader(fileName);

                    while ((line = file.ReadLine()) != null)
                    {
                        var lineFiltered = ReplaceDelimiter(line);
                        writer.WriteLine(lineFiltered);
                    }

                    file.Close();
                }
            }
            return memoryStream.ToArray();
        }


        private string ReplaceDelimiter(string line)
        {
            foreach (var item in replacementDictionary)
            {
                line = line.Replace(item.Key, item.Value);
            }
            return line;
        }

        private List<string> FindDelimiter(string line)
        {
            List<string> items = new List<string>();

            string value = "[";

            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = line.IndexOf(value, index);
                if (index == -1)
                {
                    break;
                }
               int endPos = line.IndexOf("]", index);
               items.Add(line.Substring(index, (endPos - index) + 1));
            }

            return items;
        }


        private string ConvertMHTLToHTML(string mhtFile)
        {
            var decoded_text = new StringBuilder();
            using (var reader = new StreamReader(mhtFile))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line != "Content-Transfer-Encoding: base64")
                    {
                        continue;
                    }

                    reader.ReadLine(); //chew up the blank line
                    while ((line = reader.ReadLine()) != String.Empty)
                        if (line != null)
                        {
                            decoded_text.Append(Encoding.UTF8.GetString(Convert.FromBase64String(line)));
                        }
                    break;
                }
            }

            return "";
        }
    }
}

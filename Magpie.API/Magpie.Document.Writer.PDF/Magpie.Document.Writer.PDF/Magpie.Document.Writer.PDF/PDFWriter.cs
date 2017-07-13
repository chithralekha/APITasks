using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Magpie.Document.Writer.PDF
{
    public class PDFWrite : IDocumentWriter
    {
        private string AwsAccessKey { get; set; }
        private string AwsSecretKey { get; set; }
        private string AwsBucketName { get; set; }

        public PDFWrite()
        {
        }

        public PDFWrite(string awsAccessKey, string awsSecretKey, string bucketName)
        {
            AwsAccessKey = awsAccessKey;
            AwsSecretKey = awsSecretKey;
            AwsBucketName = bucketName;
        }

        public void WriteDocument(Stream streamDocument, DocumentRepositoryInfo documentRepositoryInfo)
        {
            #region Preconditions

            if (streamDocument == null)
                throw new System.ArgumentNullException();

            if (documentRepositoryInfo == null)
                throw new System.ArgumentNullException();

            #endregion


            streamDocument.Position = 0;
            var reader = new StreamReader(streamDocument);

            string textDocument = reader.ReadToEnd();
            var pdfDocument = CreatePDF(textDocument);

            DocumentRepository documentRepository = new DocumentRepository(AwsAccessKey, AwsSecretKey, AwsBucketName);

            documentRepository.AddToRepository(pdfDocument, documentRepositoryInfo);
        }

        public void WriteDocument(string textDocument, DocumentRepositoryInfo documentRepositoryInfo)
        {
            #region Preconditions

            if (string.IsNullOrWhiteSpace(textDocument))
                throw new System.ArgumentNullException();
            
            if (documentRepositoryInfo == null)
                throw new System.ArgumentNullException();

            #endregion


            var pdfDocument = CreatePDF(textDocument);

            DocumentRepository documentRepository = new DocumentRepository(AwsAccessKey, AwsSecretKey, AwsBucketName);

            documentRepository.AddToRepository(pdfDocument, documentRepositoryInfo);

        }

        public void WriteDocument(Stream streamDocument, string fileName)
        {
            streamDocument.Position = 0;
            var reader = new StreamReader(streamDocument);

            string textDocument = reader.ReadToEnd();
            var pdfDocument = CreatePDF(textDocument);

            DocumentFileWrite fileWrite = new PDF.DocumentFileWrite();
            fileWrite.WriteToFile(pdfDocument, fileName);
        }


        public void WriteDocument(string textDocument, string fileName)
        {
            var pdfDocument = CreatePDF(textDocument);
            DocumentFileWrite fileWrite = new PDF.DocumentFileWrite();
            fileWrite.WriteToFile(pdfDocument, fileName);
        }


        private byte[] CreatePDF(string documentText)
        {
            using (MemoryStream output = new MemoryStream())
            {
                var document = new iTextSharp.text.Document(PageSize.A4, 25, 25, 30, 30);

                PdfWriter wri = PdfWriter.GetInstance(document, output);
                document.Open();

                document.Add(new Paragraph(documentText));
                document.Close();

                return output.ToArray();
            }
        }
    }
}

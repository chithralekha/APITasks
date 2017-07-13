using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Magpie.Document.Writer.PDF;
using Magpie.Document.Writer;
using System.Text;
using System.IO;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        private static readonly string awsAccessKey = "AKIAIAUKFRER6B3WN6RQ";  
        private static readonly string awsSecretKey = "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"; 
        private static readonly string bucketName = "magpie-documents"; 
        private static readonly string bucketUrl = "https://s3-us-west-2.amazonaws.com/magpie-documents/UPLOADS/";

        private string text = "Hello World";

        [TestMethod]
        public void FileWriteDocumentText()
        {
            try
            {
                string fileName = @"C:\_Magpie\FileWriteDocumentText.pdf";

                var pdfWrite = new PDFWrite();

                pdfWrite.WriteDocument(text, fileName);
            }

            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void FileWriteDocumentStream()
        {
            try
            {
                string fileName = @"C:\_Magpie\FileWriteDocumentStream.pdf";

                var pdfWrite = new PDFWrite();

                byte[] textByteArray = Encoding.ASCII.GetBytes(text);
                MemoryStream stream = new MemoryStream(textByteArray);

                pdfWrite.WriteDocument(stream, fileName);
            }

            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


        [TestMethod]
        public void AWSWriteDocumentText()
        {
            try
            {
                var pdfWrite = new PDFWrite(awsAccessKey, awsSecretKey, bucketName, bucketUrl);

                var documentRepositoryInfo = new DocumentRepositoryInfo();
                documentRepositoryInfo.BusinessControlProfileName = "Boston";
                documentRepositoryInfo.FileName = "TextDoc";

                pdfWrite.WriteDocument(text, documentRepositoryInfo);
            }

            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void AWSWriteDocumentStream()
        {
            try
            {
                var pdfWrite = new PDFWrite(awsAccessKey, awsSecretKey, bucketName, bucketUrl);

                var documentRepositoryInfo = new DocumentRepositoryInfo();
                documentRepositoryInfo.BusinessControlProfileName = "Dallas";
                documentRepositoryInfo.FileName = "TextDoc";

                byte[] textByteArray = Encoding.ASCII.GetBytes(text);
                MemoryStream stream = new MemoryStream(textByteArray);

                pdfWrite.WriteDocument(stream, documentRepositoryInfo);
            }

            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

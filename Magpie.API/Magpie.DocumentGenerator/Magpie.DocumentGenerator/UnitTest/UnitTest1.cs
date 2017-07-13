using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Magpie.DocumentGenerator;
using Magpie.Document.Writer;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        private readonly string awsAccessKey = "AKIAIAUKFRER6B3WN6RQ";
        private readonly string awsSecretKey = "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";
        private readonly string bucketName = "magpie-documents";
        private readonly string bucketUrl = "https://s3-us-west-2.amazonaws.com/magpie-documents/UPLOADS/";

        [TestMethod]
        public void ConvertDocumentAndWriteToFile()
        {
            try
            {
                DocumentRepositoryInfo documentRepositoryInfo = new DocumentRepositoryInfo();

                documentRepositoryInfo.BusinessControlProfileName = "Output";
   
                GenerateFromTemplate generateFromTemplate = new GenerateFromTemplate();

                generateFromTemplate.GenerateDocument(@"C:\Save\ProfileDocuments",  documentRepositoryInfo, null);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }

        [TestMethod]
        public void ConvertDocumentAndWriteToAws()
        {
            try
            {
                DocumentRepositoryInfo documentRepositoryInfo = new DocumentRepositoryInfo();

                AwsConfigInfo awsConfigInfo = new AwsConfigInfo(awsAccessKey, awsSecretKey, bucketName, bucketUrl);

                documentRepositoryInfo.BusinessControlProfileName = "Boston";

                GenerateFromTemplate generateFromTemplate = new GenerateFromTemplate();

                generateFromTemplate.GenerateDocument(@"C:\Save\ProfileDocuments", documentRepositoryInfo, awsConfigInfo);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }
}

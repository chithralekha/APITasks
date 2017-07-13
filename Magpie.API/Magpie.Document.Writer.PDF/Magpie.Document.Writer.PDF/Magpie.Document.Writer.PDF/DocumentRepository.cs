using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using System.IO;

namespace Magpie.Document.Writer.PDF
{
    internal class DocumentRepository
    {
        private string AccessKey { get; set; }
        private string SecretKey { get; set; }
        private string BucketName { get; set; }

        internal DocumentRepository(string awsAccessKey, string awsSecretKey, string bucketName)
        {
            AccessKey = awsAccessKey;
            SecretKey = awsSecretKey;
            BucketName = bucketName;
        }

        internal void AddToRepository(byte[] fileByteArray, DocumentRepositoryInfo documentRepositoryInfo)  
        {
            #region Preconditions

            if (fileByteArray == null)
                throw new System.ArgumentNullException();

            if (documentRepositoryInfo == null)
                throw new System.ArgumentNullException();

            #endregion

            try
            {
                IAmazonS3 client;

                using (client = new AmazonS3Client(AccessKey, SecretKey, RegionEndpoint.USWest2))
                {
                    var fileStream = new MemoryStream(fileByteArray);

                    var request = new PutObjectRequest()
                    {
                        BucketName = BucketName,
                        CannedACL = S3CannedACL.BucketOwnerFullControl,   //PERMISSION TO FILE PUBLIC ACCESIBLE
                        Key = $"{documentRepositoryInfo.FilePath}_{documentRepositoryInfo.FileName}",   //Key = $"UPLOADS/{documentRepositoryInfo.BusinessControlProfileName}_{documentRepositoryInfo.FileName}",  
                        InputStream = fileStream     //SEND THE FILE STREAM
                    };

                    PutObjectResponse response = client.PutObject(request);
                }
            }

            catch (AmazonS3Exception amazonS3Exception)
            {
                if (amazonS3Exception.ErrorCode != null && (amazonS3Exception.ErrorCode.Equals("InvalidAccessKeyId") || amazonS3Exception.ErrorCode.Equals("InvalidSecurity")))
                {
                    Console.WriteLine("Check the provided AWS Credentials.");
                }
                else
                {
                    Console.WriteLine($"Error occurred. Message:'{amazonS3Exception.Message}' when writing an object");
                }
                throw amazonS3Exception;
            }
            catch (Exception ex)
            {
                string res = ex.ToString();
                throw ex;
            }
        }
    }
}

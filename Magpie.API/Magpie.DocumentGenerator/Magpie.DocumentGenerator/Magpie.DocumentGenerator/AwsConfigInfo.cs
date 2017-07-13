using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magpie.DocumentGenerator
{
    public class AwsConfigInfo
    {
        public AwsConfigInfo(string accessKey, string secretKey, string bucketName)
        {
            AccessKey = accessKey;
            SecretKey = secretKey;
            BucketName = bucketName;
        }

        public string AccessKey { get; private set; }
        public string SecretKey { get; private set; }
        public string BucketName { get; private set; }    
    }
}

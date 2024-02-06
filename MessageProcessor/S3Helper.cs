using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MessageProcessor
{
    internal class S3Helper
    {
        readonly ConfigService configService = ConfigService.GetInstance();
        private readonly string _serviceUrl;

        public S3Helper()
        {
            this._serviceUrl = ConfigurationManager.AppSettings["ServiceUrl"];
        }

        public async Task CheckFileExists(string fileName)
        {
            using (var client = new HttpClient())
            {
                var request = new RequestModel();
                request.FileName = fileName;
                var finalPayload = JsonConvert.SerializeObject(request);
                var data = new System.Net.Http.StringContent(finalPayload, Encoding.UTF8, "application/json");
                var result = await client.PostAsync(this._serviceUrl, data);
                Console.WriteLine(result.StatusCode);
            }
        }

        public async Task<HttpStatusCode> Upload(Stream stream, string fileName)
        {
            var s3Client = !string.IsNullOrEmpty(configService.AccessKey) && !string.IsNullOrEmpty(configService.SecretKey)
                ? new AmazonS3Client(configService.AccessKey, configService.SecretKey, RegionEndpoint.GetBySystemName(configService.Region))
                : new AmazonS3Client(RegionEndpoint.GetBySystemName(configService.Region));
            try
            {
                PutObjectRequest request = new PutObjectRequest();
                request.BucketName = configService.BucketName;
                request.Key = fileName;
                request.InputStream = stream;                
                var response = await s3Client.PutObjectAsync(request);
                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine("File uploaded successfully");
                }
                else
                {
                    Console.WriteLine("File upload failed");
                }
                return response.HttpStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return HttpStatusCode.InternalServerError;
        }
    }
}

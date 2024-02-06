using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace S3WebAPI.Services
{
    public class AmazonS3Bucket : IAmazonS3Bucket
    {
        private readonly IAmazonS3 _amazonS3;
        private readonly ILogger<AmazonS3Bucket> _logger;
        private readonly AWSAppSettingsModel _awsAppSettings;

        public AmazonS3Bucket(IAmazonS3 amazonS3, ILogger<AmazonS3Bucket> logger, IOptions<AWSAppSettingsModel> awsAppSettings)
        {            
            this._amazonS3 = amazonS3;
            this._logger = logger;
            this._awsAppSettings = awsAppSettings.Value;
        }

        public async Task<bool> HasStorageExitsAsync(string fileOrUrlPath)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var fileExists = true;
            try
            {
                GetObjectMetadataRequest request = new()
                {
                    BucketName = _awsAppSettings.BucketName,
                    Key = $"{fileOrUrlPath}"
                };
                await _amazonS3.GetObjectMetadataAsync(request);
                stopwatch.Stop();
            }
            catch (System.Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex.Message);
                fileExists = false;
            }

            _logger.LogInformation($"Total time taken to complete loading of {fileOrUrlPath} :" + stopwatch.ElapsedMilliseconds);
            return fileExists;
        }
    }
}

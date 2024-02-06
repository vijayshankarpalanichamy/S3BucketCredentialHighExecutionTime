using Microsoft.AspNetCore.Mvc;
using S3WebAPI.Services;

namespace S3WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [ApiVersion("1.0")]
    public class S3BucketProxyController : ControllerBase
    {

        private readonly IAmazonS3Bucket _amazonS3Bucket;

        public S3BucketProxyController(IAmazonS3Bucket amazonS3Bucket)
        {

            _amazonS3Bucket = amazonS3Bucket;
        }

        [HttpPost]
        [Route("HasFileExists")]
        public async Task<IActionResult> HasFileExists(RequestModel requestModel)
        {
            await ValidateSecureURI(requestModel);
            return Ok();
        }

        private async Task ValidateSecureURI(RequestModel requestModel)
        {
           await ProcessS3Bucket(requestModel).WaitAsync(TimeSpan.FromMinutes(2));
           //DB Operations
        }

        private async Task ProcessS3Bucket(RequestModel requestModel)
        {
             if (!await _amazonS3Bucket.HasStorageExitsAsync(requestModel.FileName))
                throw new Exception("File Not exists");
        }
    }
}
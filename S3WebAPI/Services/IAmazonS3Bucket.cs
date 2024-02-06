namespace S3WebAPI.Services
{
    public interface IAmazonS3Bucket
    {
        Task<bool> HasStorageExitsAsync(string fileOrUrlPath);        
    }
}

# S3Bucket IAM Credential Issue

This outlines the process for replicating the issue of 'Amazon.Util.EC2InstanceMetadata - Unable to contact EC2 Metadata service to obtain a metadata token. Attempting to access IMDS without a token.' as well as the prolonged execution time of the GetObjectMetadataRequest Method.

**S3WebAPI and MessageProcessor Configuration Guide**
This guide provides instructions on how to configure and utilize the S3WebAPI solution alongside the MessageProcessor solution for managing S3 objects and their existence validation.

Prerequisites
- .NET Core SDK, .NET 4.8 installed on your machine.
- Access to an AWS account with permissions to interact with S3 services.
- Visual Studio or any other preferred IDE for .NET development.

Configuration Steps
1. Clone the Repositories the S3WebAPI and MessageProcessor solutions
2. The EC2 instance machine IAM enabled to access the S3Bucket
3. Configure S3WebAPI Solution
   1. Open the S3WebAPI.sln solution in your preferred IDE.
   2. Navigate to the appsettings.json file within the S3WebAPI project.
   3. Update the AWSAppSettings.Region and AWSAppSettings.BucketName fields with your desired AWS region and S3 bucket name, respectively.
4. Build and Run S3WebAPI Solution
   1. Build the solution to ensure that all dependencies are resolved.
   2. Run the S3WebAPI project with IISExpres. This will start the API server, allowing MessageProcessor to communicate with it.  
5. Configure MessageProcessor Solution
   1. Open the MessageProcessor.sln solution in your preferred IDE
   2. Navigate to the app.config file within the MessageProcessor project.
   3. Update the AWS Region, BucketName and the ServiceUrl(This should be Full URL of the S3WebAPI).
6. Build and Run MessageProcessor Solution
   1. The MessageProcessor concurrently generates and uploads 1000 files into the S3 Bucket. Upon successful upload, it initiates a call to the S3WebAPI to verify the existence of the file in the S3 Bucket.  
   
**Code Snippet**:
MessageProcessor to generate, uploads the file into S3Bucket, and validate the file is available in S3Bucket using the S3WebAPI.
```
static void Main(string[] args)
{
    S3Helper s3Helper = new S3Helper();
    Task[] tasks = new Task[1000];

    for (int index = 0; index < 1000; index++)
    {
        tasks[index] = NewMethod(s3Helper, index);
    }

    Task.WaitAll(tasks);
}

private static async Task NewMethod(S3Helper s3Helper, int index)
{
    var fileName = "securefiles/test_" + index + ".txt";
    if (await s3Helper.Upload(new MemoryStream(Encoding.UTF8.GetBytes(new string('*', 80896))), fileName) == System.Net.HttpStatusCode.OK)
    {
        await s3Helper.CheckFileExists(fileName);
    }
}
```
S3WebAPI
In Program.cs, Registerred the AWS Dependencies 
```
builder.Services.Configure<AWSAppSettingsModel>(builder.Configuration.GetSection("AWSAppSettings"));
builder.Services.AddDefaultAWSOptions(x =>
{
    var optionSettings = x.GetService<IOptions<AWSAppSettingsModel>>();
    AWSAppSettingsModel appSettings;
    if (optionSettings != null) 
    { 
        appSettings = optionSettings.Value;
    }
    else
    {
        appSettings = new AWSAppSettingsModel();
    }

     var awsOption = new Amazon.Extensions.NETCore.Setup.AWSOptions
    {
        Region = RegionEndpoint.GetBySystemName(appSettings.Region),
    };

    if (!string.IsNullOrWhiteSpace(appSettings.AccessKey))
    {
        awsOption.Credentials = new BasicAWSCredentials(appSettings.AccessKey, appSettings.SecretKey);
    }
    return awsOption;
});

builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddScoped<IAmazonS3Bucket, AmazonS3Bucket>();
```
Controller to Process the S3Bucket file existance validaiton
```
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
       //Actual program has non Async DB operations.
    }

    private async Task ProcessS3Bucket(RequestModel requestModel)
    {
         if (!await _amazonS3Bucket.HasStorageExitsAsync(requestModel.FileName))
            throw new Exception("File Not exists");
    }
}
```
HasStorageExitsAsync used to validate the file is available in the S3Bucket
```
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
```

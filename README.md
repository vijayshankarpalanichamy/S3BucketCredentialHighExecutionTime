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
   1. The MessageProcessor concurrently generates and uploads 1000 files to the S3 Bucket. Upon successful upload, it initiates a call to the S3WebAPI to verify the existence of the file in the S3 Bucket.  
   
   

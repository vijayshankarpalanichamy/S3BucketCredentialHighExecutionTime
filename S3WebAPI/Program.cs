using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using S3WebAPI;
using S3WebAPI.Services;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
var repository = LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof(Hierarchy));
XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));

builder.Logging.AddLog4Net();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddVersionedApiExplorer();
var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<CustomHeaderSwaggerAttribute>();
    options.OrderActionsBy(orderBy => orderBy.HttpMethod);
    options.ResolveConflictingActions(descriptions => descriptions.First());

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new OpenApiInfo
        {
            Title = "MDP Internal Version " + description.GroupName,
            Version = description.ApiVersion.MajorVersion.ToString() + "." + description.ApiVersion.MinorVersion
        });
    }
});

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
});
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
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
    }

    options.DisplayRequestDuration();
});

app.UseHttpsRedirection();
app.MapControllers();
app.UseRouting();
app.UseStaticFiles();

AWSConfigs.LoggingConfig.LogTo = LoggingOptions.Log4Net;
AWSConfigs.LoggingConfig.LogMetricsFormat = LogMetricsFormatOption.JSON;
AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
AWSConfigs.LoggingConfig.LogMetrics = true;
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();

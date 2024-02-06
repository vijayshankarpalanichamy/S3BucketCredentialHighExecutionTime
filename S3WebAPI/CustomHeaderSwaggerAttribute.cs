using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace S3WebAPI
{

        public class CustomHeaderSwaggerAttribute : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Parameters == null)
                    operation.Parameters = new List<OpenApiParameter>();

                var apiVersionParam = operation.Parameters.FirstOrDefault(x => x.Name.Equals("api-version"));
                var description = context.ApiDescription.ParameterDescriptions.First(p => p.Name == "api-version");

                if (apiVersionParam != null && description.DefaultValue != null)
                    apiVersionParam.Example = new OpenApiString(description.DefaultValue.ToString());
            }
        }
    
}

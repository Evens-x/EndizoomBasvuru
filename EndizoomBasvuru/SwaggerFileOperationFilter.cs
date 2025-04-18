using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace EndizoomBasvuru
{
    /// <summary>
    /// Dosya yükleme işlemlerini Swagger'da düzgün görüntülemek için özel filtre
    /// </summary>
    public class SwaggerFileOperationFilter : IOperationFilter
    {
        /// <summary>
        /// IFormFile ve IFormFile[] parametrelerini multipart/form-data olarak yapılandırır
        /// </summary>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasFormFileParam = context.MethodInfo.GetParameters()
                .Any(p => p.ParameterType == typeof(IFormFile) || 
                         p.ParameterType == typeof(IFormFile[]) ||
                         p.ParameterType.GetProperties().Any(prop => 
                             prop.PropertyType == typeof(IFormFile) || 
                             prop.PropertyType == typeof(IFormFile[])));

            if (!hasFormFileParam)
                return;

            // Multipart/form-data content type'ı belirleme
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = operation.RequestBody?.Content.Values
                            .FirstOrDefault()?.Schema ?? new OpenApiSchema()
                    }
                },
                Required = true
            };
            
            // Form parametrelerini doğru şekilde yapılandır
            foreach (var parameter in context.ApiDescription.ParameterDescriptions)
            {
                if (parameter.Source.Id == "Form" && parameter.ModelMetadata != null)
                {
                    var schema = context.SchemaGenerator.GenerateSchema(parameter.ModelMetadata.ModelType, context.SchemaRepository);
                    
                    if (operation.RequestBody.Content.TryGetValue("multipart/form-data", out var mediaType))
                    {
                        mediaType.Schema.Properties ??= new Dictionary<string, OpenApiSchema>();
                        
                        if (!mediaType.Schema.Properties.ContainsKey(parameter.Name))
                        {
                            mediaType.Schema.Properties.Add(parameter.Name, schema);
                            
                            if (parameter.IsRequired)
                            {
                                mediaType.Schema.Required ??= new HashSet<string>();
                                mediaType.Schema.Required.Add(parameter.Name);
                            }
                        }
                    }
                }
            }
        }
    }
} 
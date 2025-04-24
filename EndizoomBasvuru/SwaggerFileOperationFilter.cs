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

            // Örnek özelliği ekle
            var methodName = context.MethodInfo.Name;
            if (operation.RequestBody?.Content == null)
                return;
                
            // Multipart/form-data content type'ı belirleme
            operation.RequestBody = new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = operation.RequestBody?.Content.Values
                            .FirstOrDefault()?.Schema ?? new OpenApiSchema(),
                        Encoding = GetFormEncodings(context)
                    }
                },
                Required = true,
                Description = "Dosya yükleme ve form alanları içeren istek"
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
        
        /// <summary>
        /// Form alanları için encoding ayarlarını yapılandırır
        /// </summary>
        private Dictionary<string, OpenApiEncoding> GetFormEncodings(OperationFilterContext context)
        {
            var encodings = new Dictionary<string, OpenApiEncoding>();
            
            // Metot parametrelerini analiz et
            foreach (var parameter in context.MethodInfo.GetParameters())
            {
                if (parameter.ParameterType.GetProperties().Any(p => p.PropertyType == typeof(IFormFile) || p.PropertyType == typeof(IFormFile[])))
                {
                    foreach (var prop in parameter.ParameterType.GetProperties())
                    {
                        if (prop.PropertyType == typeof(IFormFile) || prop.PropertyType == typeof(IFormFile[]))
                        {
                            encodings[prop.Name] = new OpenApiEncoding 
                            { 
                                ContentType = "multipart/form-data" 
                            };
                        }
                    }
                }
            }
            
            return encodings;
        }
    }
} 
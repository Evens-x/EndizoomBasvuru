using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace EndizoomBasvuru.Swagger
{
    public class SwaggerSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(IFormFile))
            {
                schema.Type = "string";
                schema.Format = "binary";
            }
            else if (context.Type == typeof(IFormFile[]))
            {
                schema.Type = "array";
                schema.Items = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            
            // DTO'larda "camelCase" formatında alanları temizle
            if (context.Type.Namespace == "EndizoomBasvuru.Services.Models" && schema.Properties != null)
            {
                var propertyInfos = context.Type.GetProperties();
                
                var lowerCaseProps = schema.Properties.Keys
                    .Where(key => char.IsLower(key[0]))
                    .ToList();
                
                var pascalCaseProps = propertyInfos
                    .Select(p => p.Name)
                    .ToList();
                
                // Sadece PascalCase olanları sakla, camelCase olanları kaldır
                foreach (var propName in lowerCaseProps)
                {
                    // Eğer bu property'nin Pascal case versiyonu da varsa, küçük harfle başlayanı kaldır
                    if (pascalCaseProps.Any(p => string.Equals(p, propName, StringComparison.OrdinalIgnoreCase)))
                    {
                        schema.Properties.Remove(propName);
                    }
                }
            }
        }
    }
} 
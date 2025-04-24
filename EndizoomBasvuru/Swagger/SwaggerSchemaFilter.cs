using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using EndizoomBasvuru.Services.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
                return;
            }
            else if (context.Type == typeof(IFormFile[]))
            {
                schema.Type = "array";
                schema.Items = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
                return;
            }
            
            // DTO modelleri için örnek değerler otomatik olarak oluştur
            if (context.Type.Namespace == "EndizoomBasvuru.Services.Models" && schema.Properties != null)
            {
                AddExamplesForDto(schema, context);
                CleanupDuplicateProperties(schema, context);
            }
        }
        
        /// <summary>
        /// DTO sınıfları için otomatik örnek değer oluşturur
        /// </summary>
        private void AddExamplesForDto(OpenApiSchema schema, SchemaFilterContext context)
        {
            var exampleObject = new Microsoft.OpenApi.Any.OpenApiObject();
            var propertyInfos = context.Type.GetProperties();
            
            foreach (var property in propertyInfos)
            {
                // Her özellik için örnek değer oluşturma
                if (schema.Properties != null && schema.Properties.TryGetValue(property.Name, out var propSchema))
                {
                    // DescriptionAttribute veya DisplayAttribute'dan daha anlamlı örnek değerler almaya çalış
                    var description = property.GetCustomAttribute<DescriptionAttribute>()?.Description;
                    var displayName = property.GetCustomAttribute<DisplayAttribute>()?.Name;
                    
                    var exampleValue = GenerateExampleForProperty(property, description, displayName);
                    if (exampleValue != null)
                    {
                        propSchema.Example = exampleValue;
                        exampleObject[property.Name] = exampleValue;
                    }
                }
            }
            
            // Tüm örnek değerleri içeren bir örnek nesne ekle
            if (exampleObject.Count > 0)
            {
                schema.Example = exampleObject;
            }
        }
        
        /// <summary>
        /// Özellik tipine göre örnek değer üretir
        /// </summary>
        private Microsoft.OpenApi.Any.IOpenApiAny GenerateExampleForProperty(PropertyInfo property, string description = null, string displayName = null)
        {
            var propertyType = property.PropertyType;
            var isNullable = Nullable.GetUnderlyingType(propertyType) != null;
            
            if (isNullable)
            {
                propertyType = Nullable.GetUnderlyingType(propertyType);
            }
            
            // Required attribute varsa bunu değerlendir
            var isRequired = property.GetCustomAttribute<RequiredAttribute>() != null;
            
            // Özellik adına göre örnek değerler
            string propName = property.Name.ToLowerInvariant();
            
            // Email
            if (propName.Contains("email") || property.GetCustomAttribute<EmailAddressAttribute>() != null)
            {
                if (propName.Contains("company") || propName.Contains("firma"))
                    return new Microsoft.OpenApi.Any.OpenApiString("ornek@firma.com");
                else if (propName.Contains("admin"))
                    return new Microsoft.OpenApi.Any.OpenApiString("admin@endizoom.com");
                else if (propName.Contains("contact") || propName.Contains("iletisim"))
                    return new Microsoft.OpenApi.Any.OpenApiString("iletisim@ornek.com");
                else
                    return new Microsoft.OpenApi.Any.OpenApiString("ornek@mail.com");
            }
            
            // Şifre
            if (propName.Contains("password") || propName.Contains("sifre"))
            {
                return new Microsoft.OpenApi.Any.OpenApiString("G1zl1S1fre!");
            }
            
            // İsim
            if (propName.Contains("name") || propName.Contains("ad"))
            {
                if (propName.Contains("company") || propName.Contains("firma"))
                    return new Microsoft.OpenApi.Any.OpenApiString("Örnek Firma Ltd. Şti.");
                else if (propName.Contains("first") || propName.Contains("ad"))
                    return new Microsoft.OpenApi.Any.OpenApiString("Ahmet");
                else if (propName.Contains("last") || propName.Contains("soyad"))
                    return new Microsoft.OpenApi.Any.OpenApiString("Yılmaz");
                else
                    return new Microsoft.OpenApi.Any.OpenApiString("Örnek İsim");
            }
            
            // Vergi numarası
            if (propName.Contains("tax") || propName.Contains("vergi"))
            {
                return new Microsoft.OpenApi.Any.OpenApiString("1234567890");
            }
            
            // Telefon
            if (propName.Contains("phone") || propName.Contains("telefon"))
            {
                return new Microsoft.OpenApi.Any.OpenApiString("+90 555 123 4567");
            }
            
            // Bölge
            if (propName.Contains("region") || propName.Contains("bolge"))
            {
                return new Microsoft.OpenApi.Any.OpenApiString("İstanbul");
            }
            
            // Temel veri tipleri için
            if (propertyType == typeof(string))
            {
                return new Microsoft.OpenApi.Any.OpenApiString($"Örnek {displayName ?? property.Name}");
            }
            else if (propertyType == typeof(int) || propertyType == typeof(long))
            {
                return new Microsoft.OpenApi.Any.OpenApiInteger(1);
            }
            else if (propertyType == typeof(decimal) || propertyType == typeof(float) || propertyType == typeof(double))
            {
                return new Microsoft.OpenApi.Any.OpenApiDouble(100.50);
            }
            else if (propertyType == typeof(bool))
            {
                return new Microsoft.OpenApi.Any.OpenApiBoolean(true);
            }
            else if (propertyType == typeof(DateTime))
            {
                return new Microsoft.OpenApi.Any.OpenApiDateTime(DateTime.Now);
            }
            
            // Diğer tipler için null dön
            return null;
        }
        
        /// <summary>
        /// camelCase ve PascalCase arasındaki çakışmaları temizler
        /// </summary>
        private void CleanupDuplicateProperties(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties == null)
                return;
                
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
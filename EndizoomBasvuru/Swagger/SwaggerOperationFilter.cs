using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using EndizoomBasvuru.Services.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EndizoomBasvuru.Swagger
{
    public class SwaggerOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Metot parametrelerini incele
            var methodParams = context.MethodInfo.GetParameters();
            if (methodParams.Length == 0 || operation.RequestBody == null)
                return;

            // Request body için örnek değerler ekle
            foreach (var contentType in operation.RequestBody.Content)
            {
                // İşlem adı ve HTTP metodu
                var controllerName = context.MethodInfo.DeclaringType.Name.Replace("Controller", "");
                var methodName = context.MethodInfo.Name;
                var httpMethod = context.ApiDescription.HttpMethod?.ToUpper() ?? "POST";
                
                // Parametre özelliklerini bul (FromBody, FromForm vs.)
                foreach (var param in methodParams)
                {
                    // FromBody attribute'u olan parametreyi bul
                    var fromBodyAttr = param.GetCustomAttribute<FromBodyAttribute>();
                    var fromFormAttr = param.GetCustomAttribute<FromFormAttribute>();
                    
                    if (fromBodyAttr != null || fromFormAttr != null)
                    {
                        // Örnek değer oluştur
                        var paramType = param.ParameterType;
                        var example = CreateExampleForType(paramType);
                        
                        if (example != null)
                        {
                            // Örnek JSON değer olarak ekle
                            var exampleJson = JsonSerializer.Serialize(example, new JsonSerializerOptions 
                            { 
                                WriteIndented = true,
                                PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                            });
                            
                            // Swagger'a örnek ekle
                            contentType.Value.Example = new OpenApiString(exampleJson);
                            
                            // Zenginleştirilmiş açıklama
                            operation.Description = $"API Endpoint Açıklaması: {controllerName}/{methodName}\n\n" +
                                                  $"HTTP Metod: {httpMethod}\n\n" +
                                                  $"Örnek İstek Gövdesi:\n```json\n{exampleJson}\n```";
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Verilen tip için örnek bir nesne oluşturur
        /// </summary>
        private object CreateExampleForType(Type type)
        {
            if (type == null)
                return null;
                
            // Temel tipler için
            if (type == typeof(string))
                return "Örnek Metin";
                
            if (type == typeof(int) || type == typeof(int?))
                return 123;
                
            if (type == typeof(long) || type == typeof(long?))
                return 12345L;
                
            if (type == typeof(bool) || type == typeof(bool?))
                return true;
                
            if (type == typeof(DateTime) || type == typeof(DateTime?))
                return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
            if (type == typeof(decimal) || type == typeof(decimal?))
                return 123.45m;
                
            if (type == typeof(float) || type == typeof(float?))
                return 123.45f;
                
            if (type == typeof(double) || type == typeof(double?))
                return 123.45d;
                
            // Koleksiyonlar için
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                var arrayInstance = Array.CreateInstance(elementType, 1);
                arrayInstance.SetValue(CreateExampleForType(elementType), 0);
                return arrayInstance;
            }
            
            if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(List<>) || 
                                       type.GetGenericTypeDefinition() == typeof(IEnumerable<>) || 
                                       type.GetGenericTypeDefinition() == typeof(ICollection<>)))
            {
                var listType = typeof(List<>).MakeGenericType(type.GetGenericArguments()[0]);
                var list = Activator.CreateInstance(listType);
                var addMethod = listType.GetMethod("Add");
                
                // Bir örnek eleman ekle
                var elementType = type.GetGenericArguments()[0];
                var exampleElement = CreateExampleForType(elementType);
                addMethod.Invoke(list, new[] { exampleElement });
                
                return list;
            }
            
            // Kompleks tipler için yeni bir örnek oluştur
            if (!type.IsValueType && type != typeof(string))
            {
                try 
                {
                    // Varsayılan constructor ile örnek oluştur
                    var instance = Activator.CreateInstance(type);
                    
                    // Özellikleri doldur
                    foreach (var prop in type.GetProperties())
                    {
                        // Sadece yazılabilir özellikleri doldur
                        if (prop.CanWrite)
                        {
                            // Özellik adına göre akıllı değerler atama
                            var propType = prop.PropertyType;
                            var propName = prop.Name.ToLowerInvariant();
                            object propValue = null;
                            
                            // E-posta alanları için
                            if (propName.Contains("email"))
                            {
                                if (propName.Contains("company"))
                                    propValue = "ornek@firma.com";
                                else if (propName.Contains("contact"))
                                    propValue = "iletisim@ornek.com";
                                else
                                    propValue = "ornek@mail.com";
                            }
                            // Şifre alanları için
                            else if (propName.Contains("password"))
                            {
                                propValue = "Ornek123!";
                            }
                            // İsim alanları için
                            else if (propName.Contains("name"))
                            {
                                if (propName.Contains("company"))
                                    propValue = "Örnek Firma Ltd.";
                                else if (propName.Contains("first"))
                                    propValue = "Ahmet";
                                else if (propName.Contains("last"))
                                    propValue = "Yılmaz";
                                else
                                    propValue = "Örnek İsim";
                            }
                            // Telefon alanları için
                            else if (propName.Contains("phone"))
                            {
                                propValue = "+90 555 123 4567";
                            }
                            // Vergi numarası için
                            else if (propName.Contains("tax"))
                            {
                                propValue = "1234567890";
                            }
                            // Diğer alanlar için jenerik değer
                            else
                            {
                                propValue = CreateExampleForType(propType);
                            }
                            
                            // Değeri ayarla (null değilse)
                            if (propValue != null)
                            {
                                try
                                {
                                    prop.SetValue(instance, propValue);
                                }
                                catch
                                {
                                    // Tip dönüşümü hatalarını sessizce atla
                                }
                            }
                        }
                    }
                    
                    return instance;
                }
                catch
                {
                    // Nesne oluşturma hataları için null dön
                    return null;
                }
            }
            
            return null;
        }
    }
} 
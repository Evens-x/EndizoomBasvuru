using EndizoomBasvuru.Data.Context;
using EndizoomBasvuru.Repository;
using EndizoomBasvuru.Repository.Interfaces;
using EndizoomBasvuru.Services;
using EndizoomBasvuru.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using EndizoomBasvuru;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Plesk için çevre değişkenleri (environment variables) üzerinden bağlantı dizesini kontrol et
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dbHostEnv = Environment.GetEnvironmentVariable("DB_HOST");
var dbNameEnv = Environment.GetEnvironmentVariable("DB_NAME");
var dbUserEnv = Environment.GetEnvironmentVariable("DB_USER");
var dbPassEnv = Environment.GetEnvironmentVariable("DB_PASS");

// Eğer çevre değişkenleri tanımlanmışsa, bağlantı dizesini güncelle
if (!string.IsNullOrEmpty(dbHostEnv) && !string.IsNullOrEmpty(dbNameEnv) && 
    !string.IsNullOrEmpty(dbUserEnv) && !string.IsNullOrEmpty(dbPassEnv))
{
    connectionString = $"Host={dbHostEnv};Port=5432;Database={dbNameEnv};Username={dbUserEnv};Password={dbPassEnv};Ssl Mode=Require;";
}

// PostgreSQL veritabanı bağlantısı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // FormFile özelliklerinin JSON serileştirilmesi sırasında döngüsel referans hatalarını önle
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Repository registrations
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICompanyImageRepository, CompanyImageRepository>();

// Service registrations
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<TokenService>();

// Log dosyaları için yapılandırma - basit dosya loglaması
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddEventSourceLogger();

// Loglama seviyelerini ayarla
builder.Logging.SetMinimumLevel(LogLevel.Information);

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? "EndizoomDefaultSecureKeyForDevelopment1234"))
    };
});

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
// Swagger'ı .NET 9 için yapılandırma
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { 
        Title = "Endizoom API", 
        Version = "v1",
        Description = "Endizoom firma başvuru ve belge yönetim API'si"
    });
    
    // IFormFile sorunu için basitleştirilmiş yaklaşım - binary içerik olarak işle
    c.MapType<IFormFile>(() => new OpenApiSchema { 
        Type = "string", 
        Format = "binary" 
    });
    
    // IFormFile[] için eşleme
    c.MapType<IFormFile[]>(() => new OpenApiSchema { 
        Type = "array",
        Items = new OpenApiSchema { 
            Type = "string", 
            Format = "binary" 
        }
    });
    
    // Form verilerinin doğru bir şekilde işlenmesi için
    c.CustomSchemaIds(type => type.FullName);
    
    // Dosya yükleme API'lerini doğru işleyebilmek için
    c.DescribeAllParametersInCamelCase();
    c.SupportNonNullableReferenceTypes();
    
    // FormFile için özelleştirilmiş operasyon filtresi
    c.OperationFilter<SwaggerFileOperationFilter>();
    
    // JWT kimlik doğrulaması için Swagger yapılandırması
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Gerekli klasörlerin oluşturulması
EnsureDirectoriesExist();

// Veritabanını migration ile oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        // Veritabanını ve seed verilerini oluştur
        dbContext.Database.Migrate();
        
        // Admin kullanıcılarının zaten var olup olmadığını kontrol et
        if (!await dbContext.Admins.AnyAsync())
        {
            // Eğer admin kullanıcısı yoksa, veri tabanı yeni oluşturulmuş demektir
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Veritabanı ilk kez oluşturuldu. Temel admin kullanıcıları eklendi.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migrationları uygulanırken bir hata oluştu.");
    }
}

// Hata sayfalarını yapılandır
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";
        
        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exception, "Beklenmeyen bir hata oluştu: {Message}", exception?.Message);

        await context.Response.WriteAsJsonAsync(new
        {
            statusCode = context.Response.StatusCode,
            message = "Bir hata oluştu. Lütfen daha sonra tekrar deneyin."
        });
    });
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Endizoom API v1");
        // Swagger UI'ı kök URL'de aç
        c.RoutePrefix = string.Empty;
    });
}

// HTTPS için ayarları yapılandır
app.UseHttpsRedirection();
app.UseHsts();

// Enable CORS
app.UseCors("AllowAll");

// Static files için yapılandırma - uploads klasörünü erişilebilir yap
app.UseStaticFiles(); // wwwroot klasörü için
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Gerekli klasörlerin oluşturulduğundan emin ol
void EnsureDirectoriesExist()
{
    var paths = new[]
    {
        Path.Combine(Directory.GetCurrentDirectory(), "logs"),
        Path.Combine(Directory.GetCurrentDirectory(), "uploads"),
        Path.Combine(Directory.GetCurrentDirectory(), "uploads", "companies"),
        Path.Combine(Directory.GetCurrentDirectory(), "uploads", "documents")
    };

    foreach (var path in paths)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}

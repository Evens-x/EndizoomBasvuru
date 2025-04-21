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
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Security.Claims;
using System.Reflection;
using EndizoomBasvuru.Swagger;

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
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Geliştirme ortamında false olabilir
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? "EndizoomDefaultSecureKeyForDevelopment1234")),
        ClockSkew = TimeSpan.Zero, // Token süresi için tolerans sıfır
        RoleClaimType = ClaimTypes.Role, // Standart .NET role claim type
        NameClaimType = ClaimTypes.Name
    };
    
    // JWT Events
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogError("Authentication failed: {Message}", context.Exception.Message);
            
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Token validated successfully");
            
            // Rol bilgisini kontrol et
            var userRoles = context.Principal.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            logger.LogInformation("User roles: {Roles}", string.Join(", ", userRoles));
            
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("Authorization challenge triggered: {Scheme}", context.Scheme);
            
            return Task.CompletedTask;
        },
        OnMessageReceived = context => 
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("JWT auth message received");
            
            return Task.CompletedTask;
        }
    };
});

// Authorization policy'leri tanımla
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireMarketingRole", policy => policy.RequireRole("Marketing"));
    options.AddPolicy("RequireCompanyRole", policy => policy.RequireRole("Company"));
    
    // Admin veya Marketing rolüne sahip olan kullanıcılar için özel bir policy
    options.AddPolicy("AdminOrMarketing", policy => 
        policy.RequireRole("Admin", "Marketing"));
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Endizoom API", Version = "v1" });
    
    // Use SchemaFilter to help with form data models
    c.SchemaFilter<SwaggerSchemaFilter>();
    
    // Add security definitions
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
    
    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
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
        
        // Plesk üzerinde başlangıç hatalarını yakalamak için try-catch içine aldık
        try {
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
        catch (Exception migrationEx)
        {
            // Plesk'te migration çalışana kadar hata yakalama ve loglama
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(migrationEx, "Veritabanı migrationları uygulanırken özel bir hata oluştu: {Message}", migrationEx.Message);
            
            // İstisna stacktrace'i de logla
            logger.LogError("StackTrace: {StackTrace}", migrationEx.StackTrace);
            
            // İç istisna varsa onu da logla
            if (migrationEx.InnerException != null)
            {
                logger.LogError("InnerException: {Message}", migrationEx.InnerException.Message);
                logger.LogError("InnerException StackTrace: {StackTrace}", migrationEx.InnerException.StackTrace);
            }

            // Hata logs klasörüne de yazılsın
            File.AppendAllText(
                Path.Combine(Directory.GetCurrentDirectory(), "logs", "migration-error.log"),
                $"[{DateTime.Now}] ERROR: {migrationEx.Message}\n{migrationEx.StackTrace}\n\n"
            );
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı migrationları uygulanırken bir hata oluştu: {Message}", ex.Message);
        
        // Plesk için ek hata bilgisi
        logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
        
        // Hata logs klasörüne de yazılsın
        File.AppendAllText(
            Path.Combine(Directory.GetCurrentDirectory(), "logs", "startup-error.log"),
            $"[{DateTime.Now}] ERROR: {ex.Message}\n{ex.StackTrace}\n\n"
        );
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
        // JWT Token'ı otomatik olarak "Bearer " öneki ile kullan
        c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
        c.DisplayRequestDuration();
        // Her request için token'ı zorunlu kıl
        c.SupportedSubmitMethods(SubmitMethod.Get, SubmitMethod.Post, SubmitMethod.Put, SubmitMethod.Delete, SubmitMethod.Options, SubmitMethod.Head, SubmitMethod.Patch);
        // Authorize butonunu her zaman göster
        c.DocExpansion(DocExpansion.None);
    });
}

// HTTPS için ayarları yapılandır
app.UseHttpsRedirection();
app.UseHsts();

// Enable CORS - CORS'u Authentication'dan önce yapılandır
app.UseCors("AllowAll");

// Static files için yapılandırma - uploads klasörünü erişilebilir yap
app.UseStaticFiles(); // wwwroot klasörü için
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

// Authentication & Authorization - sıralama önemli!
app.UseAuthentication(); // Önce Authentication
app.UseAuthorization(); // Sonra Authorization

// Daha detaylı hata bilgisi için
app.Use(async (context, next) =>
{
    try
    {
        await next();
        
        // Yetkilendirme hatası sonrası daha detaylı log
        if (context.Response.StatusCode == 401)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogWarning("401 Unauthorized hatası: {Path} yolunda, {Method} metoduyla", context.Request.Path, context.Request.Method);
            
            // Header bilgilerini logla
            foreach (var header in context.Request.Headers)
            {
                logger.LogInformation("Header: {Key} = {Value}", header.Key, header.Value);
            }
        }
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "İstek işlenirken beklenmeyen hata");
        throw;
    }
});

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

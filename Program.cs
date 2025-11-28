using invoice_backend.Data;
using invoice_backend.Models;
using invoice_backend.Services.UserService;
using invoice_backend.Services.Auth;
using invoice_backend.Services.Invoice;
using invoice_backend.Services.Email;
using invoice_backend.Services.GoogleTokenValidator;
using invoice_backend.Services.BackgroundServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Text;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/invoice-backend-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting Invoice Backend API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    builder.Services.AddSwaggerGen(options =>
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            Description = "Enter your JWT token in the format: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"];
    var issuer = jwtSettings["Issuer"] ?? "InvoiceBackendAPI";
    var audience = jwtSettings["Audience"] ?? "InvoiceBackendClients";

    if (!string.IsNullOrWhiteSpace(secretKey))
    {
        var key = Encoding.UTF8.GetBytes(secretKey);
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
    }

    // Add SQLite
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseSqlite(connectionString);
        if (builder.Environment.IsDevelopment())
        {
            options.EnableSensitiveDataLogging();
        }
    });

    // Configure email
    var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
    if (emailConfig != null)
    {
        builder.Services.AddSingleton(emailConfig);
    }
    // Register services
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IInvoiceService, InvoiceService>();
    builder.Services.AddScoped<IEmailService, EmailService>();

    // Register background services
    builder.Services.AddScoped<IInvoiceDeletionPolicyService, InvoiceDeletionPolicyService>();
    builder.Services.AddHostedService<InvoiceDeletionPolicyHostedService>();

    // Add CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Invoice Backend API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Invoice Backend API failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

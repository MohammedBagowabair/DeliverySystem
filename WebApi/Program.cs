using Application;
using Application.Interfaces;
using Application.Mappings;
using Application.Services;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile(
    $"appsettings.{builder.Environment.EnvironmentName}.local.json",
    optional: true,
    reloadOnChange: true);

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (builder.Environment.IsDevelopment() || corsOrigins.Length == 0)
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins(corsOrigins).AllowAnyMethod().AllowAnyHeader();
        }
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApplicationServices();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "DeliverySystem API",
        Version = "v1",
        Description = "Delivery System Web API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
{
    if (builder.Environment.IsProduction())
        throw new InvalidOperationException("Jwt:Key must be configured via environment/secrets in Production.");
    jwtKey = "dev-only-insecure-key-change-me-32chars!!";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
builder.Services.AddScoped<IDriverService, DriverService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPdfGeneratorService, PdfGeneratorService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICompanySettingsService, CompanySettingsService>();
builder.Services.AddScoped<ICompanyFundService, CompanyFundService>();
builder.Services.AddScoped<IDayCloseService, DayCloseService>();
builder.Services.AddScoped<IBusinessDayService, BusinessDayService>();
builder.Services.AddScoped<IYearCloseService, YearCloseService>();
builder.Services.AddScoped<WeeklyDateRange>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration.GetConnectionString("MySqlConnectionStrings");
if (string.IsNullOrWhiteSpace(sqlConnectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is missing. Set it in appsettings.json / appsettings.Production.json.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(sqlConnectionString, sql => sql.EnableRetryOnFailure()));

var app = builder.Build();

app.UseMiddleware<WebApi.Middleware.GlobalExceptionMiddleware>();
app.UseCors("AppCors");

using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
try
{
    db.Database.Migrate();
}
catch (Exception ex)
{
    var logger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("Startup");
    logger?.LogError(ex,
        "Database startup failed. Check ConnectionStrings:DefaultConnection (SQL Server) in appsettings.");
    Console.Error.WriteLine("DATABASE STARTUP FAILED: " + ex);
    throw;
}

if (app.Configuration.GetValue("Database:AllowReset", false)
    && app.Configuration.GetValue("Database:ResetOnStartup", false))
{
    var resetLogger = scope.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger("DatabaseReset");
    await Infrastructure.Seed.DatabaseResetSeeder.ResetToAdminOnlyAsync(db, resetLogger);
}

await Infrastructure.Seed.UserSeeder.SeedAsync(app.Services);
await Infrastructure.Seed.CompanySettingsSeeder.SeedAsync(app.Services);
await Infrastructure.Seed.CompanyFundSeeder.SeedAsync(app.Services);

// Swagger available in Production too (MonsterASP / ops debugging)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "DeliverySystem API v1");
    c.RoutePrefix = "swagger";
});

// Avoid redirect loops behind MonsterASP HTTPS termination
if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();
app.Run();

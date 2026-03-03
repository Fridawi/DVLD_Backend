using DVLD.API.Middleware;
using DVLD.CORE.Entities;
using DVLD.CORE.Interfaces;
using DVLD.CORE.Interfaces.Licenses;
using DVLD.CORE.Interfaces.Tests;
using DVLD.CORE.Settings;
using DVLD.INFRASTRUCTURE.Data;
using DVLD.INFRASTRUCTURE.Repositories;
using DVLD.INFRASTRUCTURE.Services;
using DVLD.Services;
using DVLD.Services.Mapping;
using DVLD.SERVICES;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;

namespace DVLD.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                Log.Information("Starting DVLD Web API...");

                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((context, services, configuration) => configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext());

                // Add services to the container.
                builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
                builder.Services.AddProblemDetails();

                builder.Services.AddControllers();
                builder.Services.AddOpenApi();

                var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("DvldApiCorsPolicy", policy =>
                    {
                        if (builder.Environment.IsDevelopment())
                        {
                            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                        }
                        if (allowedOrigins != null && allowedOrigins.Any())
                        {
                            policy.WithOrigins(allowedOrigins)
                                  .AllowAnyHeader()
                                  .AllowAnyMethod();
                        }
                    });
                });

                builder.Services.AddRateLimiter(options =>
                {
                    options.AddPolicy("GeneralPolicy", context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 60, 
                                Window = TimeSpan.FromMinutes(1),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 2
                            }));

                    options.AddPolicy("LoginPolicy", context =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                            factory: partition => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 5,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0 
                            }));

                    options.OnRejected = async (context, token) =>
                    {
                        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                        context.HttpContext.Response.ContentType = "application/json";
                        await context.HttpContext.Response.WriteAsync("{\"message\": \"Too many requests. Please try again later.\"}", cancellationToken: token);
                    };
                });

                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

                var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.RequireHttpsMetadata = false; // ???????? ????????? ????? false
                    o.SaveToken = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                    };
                });

                builder.Services.AddHttpContextAccessor();

                #region Application Services
                builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
                builder.Services.AddScoped<IPersonService, PersonService>();
                builder.Services.AddScoped<IFileService, FileService>();
                builder.Services.AddScoped<ICountryService, CountryService>();
                builder.Services.AddScoped<ITestTypeService, TestTypeService>();
                builder.Services.AddScoped<ILicenseClassService, LicenseClassService>();
                builder.Services.AddScoped<IApplicationTypeService, ApplicationTypeService>();
                builder.Services.AddScoped<IUserService, UserService>();
                builder.Services.AddScoped<IJwtProvider, JwtProvider>();
                builder.Services.AddScoped<IApplicationService, ApplicationService>();
                builder.Services.AddScoped<IDriverService, DriverService>();
                builder.Services.AddScoped<ILocalDrivingLicenseApplicationService, LocalDrivingLicenseApplicationService>();
                builder.Services.AddScoped<ITestAppointmentService, TestAppointmentService>();
                builder.Services.AddScoped<ITestService, TestService>();
                builder.Services.AddScoped<ILicenseService, LicenseService>();
                builder.Services.AddScoped<IDetainedLicenseService, DetainedLicenseService>();
                builder.Services.AddScoped<IInternationalLicenseService, InternationalLicenseService>();
                #endregion

                // AutoMapper
                builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);

                var app = builder.Build();

                app.UseSerilogRequestLogging();

                app.UseExceptionHandler();

                if (app.Environment.IsDevelopment())
                {
                    app.MapOpenApi();
                    app.MapScalarApiReference();
                }

                #region File Configuration
                var contentTypeProvider = new FileExtensionContentTypeProvider();
                contentTypeProvider.Mappings[".avif"] = "image/avif";
                contentTypeProvider.Mappings[".webp"] = "image/webp";

                var configuredPath = builder.Configuration["FileStorage:UploadsPath"];
                string uploadsPath;

                if (Path.IsPathRooted(configuredPath))
                {
                    uploadsPath = configuredPath;
                }
                else
                {
                    uploadsPath = Path.Combine(builder.Environment.ContentRootPath, configuredPath ?? "Resources/Uploads");
                }

                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(uploadsPath),
                    RequestPath = "/uploads",
                    ContentTypeProvider = contentTypeProvider 
                });
                #endregion

                app.UseHttpsRedirection();
                app.UseCors("DvldApiCorsPolicy");
                app.UseRateLimiter();
                app.UseAuthentication();
                app.UseAuthorization();
                app.MapControllers();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "The application failed to start correctly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
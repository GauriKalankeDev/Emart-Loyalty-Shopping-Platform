using Microsoft.EntityFrameworkCore;
using Emart_DotNet.Models;
using Emart_DotNet.Repositories;
using Emart_DotNet.Services;
using Emart_DotNet.Configuration;
using Emart_DotNet.Utilities.Helpers;
using Emart_DotNet.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

namespace Emart_DotNet
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Configure Serilog for file and console logging
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                // Suppress noisy framework logs
                .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
                .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
                .WriteTo.Console()
                .WriteTo.File("Logs/emart-.txt", 
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            try
            {
                var builder = WebApplication.CreateBuilder(args);
                builder.Host.UseSerilog(); // Use Serilog instead of default logging
                builder.WebHost.UseUrls("http://localhost:8080");

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<Emart_DotNet.Utilities.Filters.LoggingActionFilter>();
            });

            // ===== CONFIGURATION =====
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            // ===== HELPERS & SERVICES =====
            builder.Services.AddScoped<JwtHelper>();
            builder.Services.AddScoped<PasswordHelper>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // ===== JWT AUTHENTICATION =====
            var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
            var jwtSettings = jwtSettingsSection.Get<JwtSettings>();
            var secretKey = jwtSettings?.Secret ?? "YourSuperSecretKeyForJwtSigning_MustBeAtLeast32CharsLong";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
                googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                // Verify this matches the Google Console redirect URI if set strictly
                googleOptions.CallbackPath = "/signin-google"; 
            });
           
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "E-Tour API", Version = "v1" });

                // JWT Authentication in Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' followed by your JWT token"
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
  
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql"),
                    mySqlOptions => mySqlOptions
                        .EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null)
                        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
                ));
            builder.Services.AddCors(options =>
            {
                    options.AddPolicy("AllowReact",
                        policy => policy
                            .WithOrigins("http://localhost:5173", "http://localhost:3000")
                            .AllowAnyHeader()
                            .AllowAnyMethod());
            });

            builder.Services.AddScoped<ICartRepository,CartRepository>();
            builder.Services.AddScoped<ICartItemRepository,CartItemRepository>();
            
            builder.Services.AddScoped<IProductRepository,ProductRepository>();
            
            builder.Services.AddScoped<IOrderRepository,OrderRepository>();
            builder.Services.AddScoped<IOrderItemRepository,OrderItemRepository>();
            
            builder.Services.AddScoped<IAddressRepository,AddressRepository>();
            builder.Services.AddScoped<IStoreRepository,StoreRepository>();
            builder.Services.AddScoped<ICustomerRepository,CustomerRepository>();

            builder.Services.AddScoped<IPaymentRepository,PaymentRepository>();


            builder.Services.AddScoped<ICartService,CartService>();
            builder.Services.AddScoped<IOrderService,OrderService>();
            builder.Services.AddScoped<IPaymentService,PaymentService>();
            builder.Services.AddScoped<IProductService,ProductService>();
            builder.Services.AddScoped<IStoreService, StoreService>();
            builder.Services.AddScoped<ICheckoutService, CheckoutService>();

            // Register Repositories
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<ISubCategoryRepository, SubCategoryRepository>();

            // Register Services
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISubCategoryService, SubCategoryService>();
            builder.Services.AddScoped<IAddressService, AddressService>();
            builder.Services.AddScoped<IEPointsService, EPointsService>();
            
            builder.Services.AddScoped<IEmartCardRepository, EmartCardRepository>();

            builder.Services.AddScoped<IUserService, UserService>(); // Replaced CustomerService
            builder.Services.AddScoped<IEmartCardService, EmartCardService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            
            // Add HttpClient for Analytics
            builder.Services.AddHttpClient();
            
            // Add Health Checks
            builder.Services.AddHealthChecks();
            
            // Admin Module
            builder.Services.AddScoped<IAdminRepository, AdminRepository>();
            builder.Services.AddScoped<IAdminService, AdminService>();
            builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Global Exception Handler Middleware (must be early in pipeline)
            app.UseGlobalExceptionHandler();

            // Serve static files (images) from wwwroot
            app.UseStaticFiles();

            app.UseHttpsRedirection();

            // Add CORS policy
            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            
            // Map Health Checks & Actuator-like endpoints
            app.MapHealthChecks("/actuator/health");
            app.MapGet("/actuator/info", () => new { app = new { name = "Emart .NET Backend", version = "1.0.0" } });
            app.MapGet("/actuator/metrics", () => new { message = "Metrics not fully implemented but endpoint exists" });

            Log.Information("Emart .NET Backend started successfully");
            app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

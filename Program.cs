using System.Text;
using System.Text.Json.Serialization;
using CloudinaryDotNet;
using HotelManagement.Data;
using HotelManagement.Middlewares;
using HotelManagement.Models;
using HotelManagement.Services;
using HotelManagement.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString"))
);

// Cloudinary
builder.Services.Configure<CloudinarySetting>(builder.Configuration.GetSection("CloudinarySettings"));
var cloudinarySettings = builder.Configuration
                                .GetSection("CloudinarySettings")
                                .Get<CloudinarySetting>();

var account = new Account(cloudinarySettings!.CloudName,
                          cloudinarySettings.ApiKey,
                          cloudinarySettings.ApiSecret);

var cloudinary = new Cloudinary(account);
cloudinary.Api.Secure = true;
builder.Services.AddSingleton(cloudinary);

// Password Hasher
builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

// JWT Settings & Authentication
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
var jwtSettings = builder.Configuration
                         .GetSection("JwtSettings")
                         .Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings?.SecretKey ?? "DefaultSecretKey1234567890123456")),
        ClockSkew = TimeSpan.Zero
    };
});

// Controllers
builder.Services.AddControllers();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<RoomTypeService>();
builder.Services.AddScoped<RoomService>();
builder.Services.AddScoped<RoomImageService>();
builder.Services.AddScoped<ServiceService>();
builder.Services.AddScoped<ServicePackageService>();
builder.Services.AddScoped<PromotionService>();
builder.Services.AddScoped<ApplyPromotionService>();
builder.Services.AddScoped<CancellationPolicyService>();
builder.Services.AddScoped<HotelManagement.Services.BookingService>();
builder.Services.AddScoped<BookingDetailService>();
builder.Services.AddScoped<GuestService>();
builder.Services.AddScoped<BookingServiceService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<CancellationService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<FavoriteRoomService>();
builder.Services.AddScoped<CloudinaryService>();    // Cloud Service
builder.Services.AddScoped<StatisticsService>();    // Statistics
builder.Services.AddScoped<DashboardService>();
// Auditlog Service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<AuditLogService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Middleware
app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Hotel Management API v1");
    });
}

app.UseHttpsRedirection();

app.UseRouting();

// Kích hoạt CORS cho frontend Next.js
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seeder
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    DbSeeder.Seed(context);
}

app.Run();

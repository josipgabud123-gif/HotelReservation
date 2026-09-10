using HotelReservation.API.BackgroundServices;
using HotelReservation.API.Middleware;
using HotelReservation.Application.Interfaces.Services;
using HotelReservation.Application.Services;
using HotelReservation.Domain.Interfaces;
using HotelReservation.Domain.Interfaces.Repositories;
using HotelReservation.Infrastructure.Authentication;
using HotelReservation.Infrastructure.Data;
using HotelReservation.Infrastructure.Data.Seed;
using HotelReservation.Infrastructure.Repositories;
using HotelReservation.Infrastructure.Storage;
using HotelReservation.Infrastructure.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// SERILOG
builder.Host.UseSerilog(
    (context, configuration) =>
        configuration.ReadFrom.Configuration(
            context.Configuration));


// DATABASE
builder.Services.AddDbContext<HotelReservationDbContext>(
    options =>
        options.UseSqlServer(
            builder.Configuration
                .GetConnectionString(
                    "DefaultConnection")));


// REPOSITORIES
builder.Services.AddScoped<
    IAccountRepository,
    AccountRepository>();

builder.Services.AddScoped<
    IAccommodationRepository,
    AccommodationRepository>();

builder.Services.AddScoped<
    IAccommodationUnitRepository,
    AccommodationUnitRepository>();

builder.Services.AddScoped<
    IReservationRepository,
    ReservationRepository>();

builder.Services.AddScoped<
    IReviewRepository,
    ReviewRepository>();

builder.Services.AddScoped<
    IAvailabilityBlockRepository,
    AvailabilityBlockRepository>();

builder.Services.AddScoped<
    IAmenityRepository,
    AmenityRepository>();


// UNIT OF WORK
builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();


// APPLICATION SERVICES
builder.Services.AddScoped<
    IAccommodationService,
    AccommodationService>();

builder.Services.AddScoped<
    IAccommodationUnitService,
    AccommodationUnitService>();

builder.Services.AddScoped<
    IAmenityService,
    AmenityService>();

builder.Services.AddScoped<
    IAvailabilityBlockService,
    AvailabilityBlockService>();

builder.Services.AddScoped<
    IAccommodationSearchService,
    AccommodationSearchService>();

builder.Services.AddScoped<
    IReservationService,
    ReservationService>();

builder.Services.AddScoped<
    IReservationCompletionService,
    ReservationCompletionService>();

builder.Services.AddScoped<
    IReviewService,
    ReviewService>();

builder.Services.AddScoped<
    IAccountManagementService,
    AccountManagementService>();


// BACKGROUND SERVICES
builder.Services.AddHostedService<
    ReservationCompletionBackgroundService>();


// JWT SETTINGS
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

var jwtSettings =
    builder.Configuration
        .GetSection("Jwt")
        .Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are missing.");


// AUTH SERVICES
builder.Services.AddScoped<
    IPasswordHasher,
    PasswordHasher>();

builder.Services.AddScoped<
    IJwtService,
    JwtService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();


// AUTHENTICATION
builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    jwtSettings.Issuer,

                ValidAudience =
                    jwtSettings.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSettings.Key))
            };
    });

builder.Services.AddAuthorization();


// FILE STORAGE
builder.Services.AddScoped<
    IFileStorageService>(
    _ => new LocalFileStorageService(
        Path.Combine(
            builder.Environment.ContentRootPath,
            "wwwroot")));


// CONTROLLERS
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            string message = string.Join(
                " ",
                context.ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e =>
                        string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? "Invalid request data."
                            : e.ErrorMessage)
                    .Distinct());

            if (string.IsNullOrWhiteSpace(message))
            {
                message = "Invalid request data.";
            }

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                new
                {
                    statusCode = 400,
                    message
                });
        };
    });


// SWAGGER
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new Microsoft.OpenApi.Models
            .OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type =
                Microsoft.OpenApi.Models
                    .SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In =
                Microsoft.OpenApi.Models
                    .ParameterLocation.Header,

            Description =
                "Enter your JWT token."
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models
            .OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models
                    .OpenApiSecurityScheme
                {
                    Reference =
                        new Microsoft.OpenApi.Models
                            .OpenApiReference
                        {
                            Type =
                                Microsoft.OpenApi.Models
                                    .ReferenceType
                                    .SecurityScheme,

                            Id = "Bearer"
                        }
                },

                Array.Empty<string>()
            }
        });
});


var app = builder.Build();


// ADMIN SEED
using (var scope =
       app.Services.CreateScope())
{
    var context =
        scope.ServiceProvider
            .GetRequiredService<
                HotelReservationDbContext>();

    var passwordHasher =
        scope.ServiceProvider
            .GetRequiredService<
                IPasswordHasher>();

    string adminEmail =
        builder.Configuration[
            "InitialAdmin:Email"]
        ?? throw new InvalidOperationException(
            "Initial admin email is missing.");

    string adminPassword =
        builder.Configuration[
            "InitialAdmin:Password"]
        ?? throw new InvalidOperationException(
            "Initial admin password is missing.");

    await AdminSeeder.SeedAsync(
        context,
        passwordHasher,
        adminEmail,
        adminPassword);
}


// MIDDLEWARE

app.UseMiddleware<
    GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();


// JWT authentication
app.UseAuthentication();


// Check if authenticated account is active
app.UseMiddleware<
    ActiveAccountMiddleware>();


// Role / endpoint authorization
app.UseAuthorization();


app.MapControllers();

app.Run();
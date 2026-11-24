using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using API.Authentication;
using API.Email;
using Application.Services.Implementations;
using Infrastructure.Extensions;
using Application.Extensions;
using API.Middleware;
using Shared.DTOs;
using Application.Services.Interfaces;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(value => value.Errors)
            .Select(error => error.ErrorMessage)
            .Where(message => !string.IsNullOrWhiteSpace(message));
        return new BadRequestObjectResult(ApiErrorResponse.Create("Validatie mislukt.", errors));
    };
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        }] = Array.Empty<string>()
    });
});

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddOptions<JwtOptions>()
    .Bind(jwtSection)
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var userIdValue = context.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
                var role = context.Principal?.FindFirstValue(ClaimTypes.Role);
                var validator = context.HttpContext.RequestServices.GetRequiredService<IAccessTokenValidator>();
                if (!Guid.TryParse(userIdValue, out var userId) || role is null ||
                    !await validator.IsCurrentAsync(userId, role))
                {
                    context.Fail("Het toegangstoken is niet meer actueel.");
                }
            },
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ApiErrorResponse.Create("Authenticatie is vereist."));
            },
            OnForbidden = async context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ApiErrorResponse.Create("Je hebt onvoldoende rechten."));
            }
        };
    });
builder.Services.AddAuthorization();

// API global exception handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Infrastructure (EF Core DbContext)
builder.Services.AddInfrastructure(builder.Configuration);

// Application services
builder.Services.AddApplicationServices();

// E-mails to visitors without an account. LoggingVisitorEmailSender only logs; replace it with a class that really
// sends the mail. The link to the cancellation page comes from the setting "VisitorEmails:CancellationPageUrl".
builder.Services.Configure<VisitorEmailOptions>(builder.Configuration.GetSection(VisitorEmailOptions.SectionName));
builder.Services.AddScoped<IVisitorEmailSender, LoggingVisitorEmailSender>();

var app = builder.Build();

var bootstrapArgument = args.FirstOrDefault(argument =>
    argument.StartsWith("--bootstrap-admin=", StringComparison.OrdinalIgnoreCase));
var shouldMigrate = args.Contains("--migrate", StringComparer.OrdinalIgnoreCase) ||
    args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase) ||
    bootstrapArgument is not null;

if (shouldMigrate)
{
    await using var scope = app.Services.CreateAsyncScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    if (bootstrapArgument is not null)
    {
        var email = bootstrapArgument[(bootstrapArgument.IndexOf('=') + 1)..];
        var bootstrapper = scope.ServiceProvider.GetRequiredService<IAdministratorBootstrapService>();
        await bootstrapper.BootstrapAsync(email);
        return;
    }

    if (args.Contains("--migrate-only", StringComparer.OrdinalIgnoreCase))
        return;
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
// The development UI calls the API's HTTP endpoint in both launch profiles.
// Redirecting these requests to HTTPS strips HttpClient's Bearer header.
if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

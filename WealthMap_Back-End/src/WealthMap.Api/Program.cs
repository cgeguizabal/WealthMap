using WealthMap.Infrastructure;
using WealthMap.Application;
using WealthMap.Api.Middleware;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Pays salary into deposit accounts on each job's payment days. Catches up on
// startup, so paydays that passed while the app was stopped are not skipped.
builder.Services.AddHostedService<WealthMap.Api.BackgroundServices.SalaryPostingRunner>();







// In development the frontend reaches the API through Vite's proxy, which makes
// the call same-origin — nothing here applies. A deployed frontend is a genuinely
// different origin, so its exact URL has to be listed in Cors:AllowedOrigins.
//
// Origins are configuration rather than constants because they differ per
// environment, and the list is deliberately explicit: AllowAnyOrigin would let
// any site on the internet call the API with a token it had somehow obtained.
const string CorsPolicy = "WealthMapFrontend";

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
    {
        // AllowCredentials is required for the refresh cookie to travel at all on a
        // cross-origin call. It is also why the origin list must stay explicit:
        // browsers reject credentials combined with a wildcard origin outright, so
        // there is no way to loosen this by accident without CORS breaking loudly.
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]
                    ?? throw new InvalidOperationException("Jwt:Secret is not configured."))),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<WealthMap.Api.Auth.CookieSettings>(
    builder.Configuration.GetSection(WealthMap.Api.Auth.CookieSettings.SectionName));
builder.Services.AddScoped<WealthMap.Api.Auth.RefreshTokenCookie>();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

// Before authentication: a rejected pre-flight must come back as a CORS failure
// the browser can explain, not as a 401 the frontend would misread as a bad token.
app.UseCors(CorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

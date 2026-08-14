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
        // No AllowCredentials: the JWT travels in the Authorization header, not a
        // cookie, so the browser never needs to send credentials cross-origin.
        // Adding it would also make the wildcard-origin mistake impossible to fix
        // quietly, since browsers reject that pair outright.
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
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

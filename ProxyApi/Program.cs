using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var jwtConfig = builder.Configuration.GetSection("Authentication:JwtBearer");
var proxyConfig = builder.Configuration.GetSection("ReverseProxy");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = jwtConfig["Authority"];
        options.Audience = jwtConfig["Audience"];
        options.MetadataAddress=jwtConfig["MetadataAddress"]!;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = jwtConfig.GetValue<bool>("ValidateIssuer"),
            ValidateAudience = jwtConfig.GetValue<bool>("ValidateAudience"),
            ValidAudience = jwtConfig["Audience"],
            ValidIssuer = jwtConfig["Authority"]
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("proxyApiPolicy", policy =>
        policy.RequireAuthenticatedUser()
            .RequireClaim("aud", jwtConfig["Audience"])); // Ensure aud matches
});

builder.Services.AddReverseProxy().LoadFromConfig(proxyConfig);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapReverseProxy();

app.Run();
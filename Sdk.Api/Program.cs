using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Xml;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Sdk.Core;
using Sdk.Core.Domain.Interfaces;
using Sdk.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// add core service from SDK, make use of dependency injection
builder.Services.AddSdkCoreServices();

// add default storage implementation
var storageType = builder.Configuration.GetValue<string>("Storage:Type");
if (storageType != null && storageType.Equals("postgres", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddPostgresStorage(options =>
    {
        options.ConnectionString = builder.Configuration.GetConnectionString("Postgres");
        options.Schema = builder.Configuration.GetValue<string>("Storage:Schema") ?? "public";
    });
}
else
{
    builder.Services.AddDefaultStorage();
}

// add authentication handler
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Disable built-in validation via fake parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = builder.Configuration.GetValue<bool>("Token:ValidateIssuer"),
            ValidateAudience = builder.Configuration.GetValue<bool>("Token:ValidateAudience"),
            ValidateIssuerSigningKey = builder.Configuration.GetValue<bool>("Token:ValidateIssuerSigningKey"),
            ValidateLifetime = builder.Configuration.GetValue<bool>("Token:ValidateLifetime"),
            ValidateActor = builder.Configuration.GetValue<bool>("Token:ValidateActor"),
            ValidateTokenReplay = builder.Configuration.GetValue<bool>("Token:ValidateTokenReplay"),
            SignatureValidator = (token, asdf) => new JsonWebTokenHandler().ReadJsonWebToken(token)
        };
        // Custom logic example: additional validation
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = _ => Task.CompletedTask,
            OnMessageReceived = _ => Task.CompletedTask
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CustomPolicy", policy =>
    {
        policy.RequireAssertion(context =>
        {
            // Custom authorization logic here
            // Return true if authorized, false otherwise
            return context.User.HasClaim(c => c.Type == "custom-claim");
        });
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
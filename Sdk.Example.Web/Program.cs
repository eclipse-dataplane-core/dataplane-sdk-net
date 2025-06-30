
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Sdk.Core;
using Sdk.Core.Domain;
using Sdk.Core.Domain.Messages;
using Sdk.Core.Extension;
using Sdk.Example.Web;
using Void = Sdk.Core.Domain.Void;

var builder = WebApplication.CreateBuilder(args);

// Add controllers to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDataPlaneSdk();


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


//automatically migrate the database schema - can be replaced by invoking `dotnet ef database update` command
await using (var dbContext = new DataFlowContext())
{
    await dbContext.EnsureMigrated();
}

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
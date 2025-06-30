using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Sdk.Core.Authorization;
using Sdk.Example.Web;

var builder = WebApplication.CreateBuilder(args);

// Add controllers to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDataPlaneSdk(builder.Configuration);

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
            SignatureValidator = (token, _) => new JsonWebTokenHandler().ReadJsonWebToken(token)
        };
        // Custom logic example: additional validation
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = c => { return Task.CompletedTask; },
            OnMessageReceived = _ => Task.CompletedTask
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DataFlowAccess", policy =>
        policy.Requirements.Add(new DataFlowRequirement()));
    
    options.AddPolicy("FooAccess", policy => 
        policy.Requirements.Add(new FooRequirement()));
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
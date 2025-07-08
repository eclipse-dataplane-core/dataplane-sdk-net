using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sdk.Api;
using Sdk.Api.Controllers;
using Sdk.Api.Test;

const string testAuthScheme = "Test";

var builder = WebApplication.CreateBuilder(args);

var services = builder.Services;
services.AddAuthentication(testAuthScheme)
    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(testAuthScheme, _ => { });

services.Configure<AuthenticationOptions>(options =>
{
    options.DefaultAuthenticateScheme = testAuthScheme;
    options.DefaultChallengeScheme = testAuthScheme;
});
services.AddControllers()
    .AddApplicationPart(typeof(DataPlaneSignalingApiController).Assembly);

services.AddSdkAuthorization();

var app = builder.Build();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
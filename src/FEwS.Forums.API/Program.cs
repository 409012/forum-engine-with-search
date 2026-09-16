using System.Reflection;
using Microsoft.AspNetCore.DataProtection;
using FEwS.Forums.API.Authentication;
using FEwS.Forums.API.Middlewares;
using FEwS.Forums.API.Monitoring;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.DependencyInjection;
using FEwS.Forums.Storage.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApiLogging(builder.Configuration, builder.Environment)
    .AddApiMetrics(builder.Configuration);
IDataProtectionBuilder dataProtection = builder.Services.AddDataProtection()
    .SetApplicationName("FEwS.Forums");
if (builder.Configuration["DataProtection:KeysPath"] is { Length: > 0 } keysPath)
{
    dataProtection.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
}
builder.Services.AddScoped<ISymmetricEncryptor, DataProtectionTokenProtector>();
builder.Services.AddScoped<ISymmetricDecryptor, DataProtectionTokenProtector>();
builder.Services.AddScoped<IAuthTokenStorage, AuthTokenStorage>();

builder.Services
    .AddForumDomain()
    .AddForumStorage(builder.Configuration.GetConnectionString("Postgres") ?? throw new InvalidOperationException());
builder.Services.AddAutoMapper(config => config.AddMaps(Assembly.GetExecutingAssembly()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

WebApplication app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapControllers();
app.MapPrometheusScrapingEndpoint();

app
    .UseMiddleware<ErrorHandlingMiddleware>()
    .UseMiddleware<AuthenticationMiddleware>();

app.Run();

public partial class Program;

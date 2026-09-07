using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ratbags.Accounts.Client;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.Models.DB;
using Ratbags.Articles.API.ServiceExtensions;
using Ratbags.Core.Messaging.ASB;

var builder = WebApplication.CreateBuilder(args);

// secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<AppSettings>(builder.Configuration);
var appSettings = builder.Configuration.Get<AppSettings>() ?? throw new Exception("Appsettings.json missing");

var certificatePath = string.Empty;
var certificateKeyPath = string.Empty;

// are we in docker?
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

certificatePath = isDocker
    ? Path.Combine("/https", appSettings.Certificate.Name)
    : Path.Combine(appSettings.Certificate.Path, appSettings.Certificate.Name);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(Convert.ToInt32(appSettings.Ports.Http));
    serverOptions.ListenAnyIP(Convert.ToInt32(appSettings.Ports.Https), listenOptions =>
    {
        listenOptions.UseHttps(
            certificatePath,
            appSettings.Certificate.Password);
    });
});

// add services
// cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            .WithOrigins("https://localhost:5001") // ocelot
            .WithOrigins("https://localhost:5000") // ocelot http
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

builder.Services.AddRatbagsServiceBus(appSettings);

// direct http call to accounts-api - replaces the old asb-based username lookup.
// cert bypass is dev-only - see AddAccountsClient for why it's needed in docker.
builder.Services.AddAccountsClient(appSettings.Services.AccountsApi, builder.Environment.IsDevelopment());

builder.Services.Configure<JsonOptions>(x =>
{
    x.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

// add service extensions
builder.Services.AddDIServiceExtension();
builder.Services.AddAuthenticationServiceExtension(appSettings);

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// errors
if (!app.Environment.IsDevelopment())
{
    // production errors
    app.UseExceptionHandler("/error");  // TODO needs endpoint
}
else
{
    app.UseDeveloperExceptionPage();
}

//app.UseHttpsRedirection();
if (!app.Environment.IsEnvironment("Docker")) app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();

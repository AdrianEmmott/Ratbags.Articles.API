using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.IOC;
using Ratbags.Articles.API.Models;
using Ratbags.Articles.API.ServiceExtensions;
using Ratbags.Shared.DTOs.Events.AppSettingsBase;

var builder = WebApplication.CreateBuilder(args);

// secrets
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.Configure<AppSettingsBase>(builder.Configuration);
var appSettings = builder.Configuration.Get<AppSettingsBase>() ?? throw new Exception("Appsettings missing");

// config kestrel for https on 5001
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5078); // HTTP
    serverOptions.ListenAnyIP(7159, listenOptions =>
    {
        listenOptions.UseHttps(
            appSettings.Certificate.Name,
            appSettings.Certificate.Password);
    });
});

// config cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
            //.WithOrigins("https://localhost:7117")    // ocelot - vs
            .WithOrigins("https://localhost:5001")      // ocelot - docker
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDIServiceExtension();
builder.Services.AddMassTransitWithRabbitMqServiceExtension(appSettings);

var app = builder.Build();

app.UseCors("AllowSpecificOrigin");

// config http request pipeline
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
    app.UseExceptionHandler("/error");  // needs endpoint
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
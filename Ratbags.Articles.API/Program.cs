using MassTransit;
using Microsoft.EntityFrameworkCore;
using Ratbags.Articles.API.IOC;
using Ratbags.Articles.API.Models;
using Ratbags.Shared.DTOs.Events.AppSettingsBase;
using Ratbags.Shared.DTOs.Events.Events.CommentsRequest;

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

builder.Services.AddCustomServices();

// mass transit
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host($"rabbitmq://{appSettings.Messaging.Hostname}/{appSettings.Messaging.VirtualHost}", h =>
        {
            h.Username(appSettings.Messaging.Username);
            h.Password(appSettings.Messaging.Password);
        });

        // ensure messages sent to correct exchange and routing key
        cfg.Message<CommentsForArticleRequest>(c =>
        {
            c.SetEntityName("articles.comments.exchange"); // set exchange name for this message type
        });

        cfg.Send<CommentsForArticleRequest>(x =>
        {
            x.UseRoutingKeyFormatter(context => "request");
        });
    });
});

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
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

// Configure Kestrel to use HTTPS on port 5001
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

// Configure CORS
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

// Add services to the container.
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

        // Ensure you're sending messages to the correct exchange and routing key
        cfg.Message<CommentsForArticleRequest>(c =>
        {
            c.SetEntityName("articles.comments.exchange"); // Sets the exchange name for this message type
        });

        cfg.Send<CommentsForArticleRequest>(x =>
        {
            x.UseRoutingKeyFormatter(context => "request");
        });
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
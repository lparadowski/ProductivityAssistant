using Anthropic.SDK;
using Application;
using Application.Settings;
using Infrastructure;

var builder = WebApplication.CreateBuilder(args);

//Todo Configure logging.

var applicationSettings = builder.Configuration.GetSection("ApplicationSettings").Get<ApplicationSettings>();
builder.Services.AddApplicationServices(applicationSettings!);
builder.Services.AddInfrastructureServices();

builder.Services.AddCors(corsOptions =>
    corsOptions.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    }));

//Add when necessary.
//builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning();
builder.Services.AddCors(corsOptions =>
corsOptions.AddPolicy("AllowAll", builder =>
{
    builder.AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
}));

builder.Services.AddSingleton(sp =>
    new AnthropicClient(applicationSettings!.AnthropicApiKey));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseCors("AllowAll");

app.MapControllers();

app.Run();
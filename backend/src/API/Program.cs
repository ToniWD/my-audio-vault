using MusicStream.API.Extensions;
using MusicStream.Application.Configuration;
using MusicStream.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<AuthenticationSettings>(builder.Configuration.GetSection("Authentication"));
// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);

// Endpoints auto validation
builder.Services.AddRequestFluentValidation();
// Global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "API v1");
    });
}

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.MapControllers();

app.Run();


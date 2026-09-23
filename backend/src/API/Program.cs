using MusicStream.API.Extensions;
using MusicStream.Application.Configuration;
using MusicStream.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var allowExpoOrigin = "_allowExpoOrigin";

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: allowExpoOrigin, policy =>
    {
        policy.WithOrigins("http://localhost:8081", "http://localhost:19006")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddInfrastructureServices(builder.Configuration);

// Endpoints auto validation
builder.Services.AddRequestFluentValidation();
// Global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

var app = builder.Build();

await app.EnsureS3BucketCreatedAsync();
await app.SeedDatabaseAsync();

app.UseCors(allowExpoOrigin);
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


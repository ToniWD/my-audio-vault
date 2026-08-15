using FluentValidation;
using FluentValidation.AspNetCore;
using MusicStream.Application.Validators;

namespace MusicStream.API.Extensions;

public static class RequestsValidationExtensions
{
    public static void AddRequestFluentValidation(this IServiceCollection services)
    {
        // Add all request validators
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

        // Enable auto validation for endpoints requests 
        services.AddFluentValidationAutoValidation();
    }
}
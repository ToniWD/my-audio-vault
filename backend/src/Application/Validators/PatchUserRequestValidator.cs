using FluentValidation;
using MusicStream.Application.DTOs;

namespace MusicStream.Application.Validators;

using FluentValidation;

public class PatchUserRequestValidator : AbstractValidator<PatchUserRequest>
{
    public PatchUserRequestValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("At least one field must be provided for update.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username cannot be empty.")
            .Length(3, 20).WithMessage("Username must be between 3 and 20 characters.")
            .Matches("^[a-zA-Z0-9_.]+$").WithMessage("Username can only contain letters, numbers, underscores, and dots.")
            .When(x => x.Username != null);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name cannot be empty.")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.")
            .When(x => x.FirstName != null);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name cannot be empty.")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.")
            .When(x => x.LastName != null);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address cannot be empty.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .When(x => x.Email != null);
    }

    private bool HaveAtLeastOneField(PatchUserRequest request)
    {
        return request.Username != null
            || request.FirstName != null
            || request.LastName != null
            || request.Email != null;
    }
}
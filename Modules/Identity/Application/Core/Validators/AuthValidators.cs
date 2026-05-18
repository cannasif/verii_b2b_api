using FluentValidation;
using Wms.Application.Common;
using Wms.Application.Identity.Dtos;

namespace Wms.Application.Identity.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.LoginEmailRequired"))
            .MaximumLength(100).WithMessage(loc.GetLocalizedString("ValidatorAuth.LoginEmailTooLong"));
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.LoginPasswordRequired"))
            .MaximumLength(100).WithMessage(loc.GetLocalizedString("ValidatorAuth.LoginPasswordTooLong"));
    }
}

public sealed class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.UsernameRequired"))
            .MaximumLength(50).WithMessage(loc.GetLocalizedString("ValidatorAuth.UsernameTooLong"));
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.EmailRequired"))
            .EmailAddress().WithMessage(loc.GetLocalizedString("ValidatorAuth.EmailInvalid"))
            .MaximumLength(100).WithMessage(loc.GetLocalizedString("ValidatorAuth.EmailTooLong"));
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordRequired"))
            .MinimumLength(6).WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordMinLength"))
            .MaximumLength(100).WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordTooLong"));
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordsMustMatch"));
        RuleFor(x => x.FirstName).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.FirstName));
        RuleFor(x => x.LastName).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.LastName));
        RuleFor(x => x.PhoneNumber).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));
    }
}

public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.EmailRequired"))
            .EmailAddress().WithMessage(loc.GetLocalizedString("ValidatorAuth.EmailInvalid"));
    }
}

public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Token).NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.ResetTokenRequired"));
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordRequired"))
            .MinimumLength(6).WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordMinLength"));
    }
}

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(ILocalizationService loc)
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.CurrentPasswordRequired"));
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordRequired"))
            .MinimumLength(6).WithMessage(loc.GetLocalizedString("ValidatorAuth.PasswordMinLength"));
    }
}

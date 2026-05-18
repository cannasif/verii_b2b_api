using FluentValidation;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;

namespace Wms.Application.Communications.Validators;

public sealed class UpdateSmtpSettingsDtoValidator : AbstractValidator<UpdateSmtpSettingsDto>
{
    public UpdateSmtpSettingsDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Host)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorSmtp.HostRequired"))
            .MaximumLength(200).WithMessage(loc.GetLocalizedString("ValidatorSmtp.HostTooLong"));
        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage(loc.GetLocalizedString("ValidatorSmtp.PortRangeInvalid"));
        RuleFor(x => x.Username).MaximumLength(200);
        RuleFor(x => x.Password).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Password));
        RuleFor(x => x.FromEmail)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorSmtp.FromEmailRequired"))
            .MaximumLength(200).WithMessage(loc.GetLocalizedString("ValidatorSmtp.FromEmailTooLong"))
            .EmailAddress().WithMessage(loc.GetLocalizedString("ValidatorSmtp.FromEmailInvalid"));
        RuleFor(x => x.FromName)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorSmtp.FromNameRequired"))
            .MaximumLength(200).WithMessage(loc.GetLocalizedString("ValidatorSmtp.FromNameTooLong"));
        RuleFor(x => x.Timeout)
            .InclusiveBetween(1, 300).WithMessage(loc.GetLocalizedString("ValidatorSmtp.TimeoutRangeInvalid"));
    }
}

public sealed class SendTestMailDtoValidator : AbstractValidator<SendTestMailDto>
{
    public SendTestMailDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.To)
            .EmailAddress().WithMessage(loc.GetLocalizedString("ValidatorSmtp.TestMailToEmailInvalid"))
            .When(x => !string.IsNullOrWhiteSpace(x.To));
    }
}

public sealed class CreateNotificationDtoValidator : AbstractValidator<CreateNotificationDto>
{
    public CreateNotificationDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorNotification.TitleRequired"))
            .MaximumLength(200).WithMessage(loc.GetLocalizedString("ValidatorNotification.TitleTooLong"));
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorNotification.BodyRequired"))
            .MaximumLength(2000).WithMessage(loc.GetLocalizedString("ValidatorNotification.BodyTooLong"));
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.RelatedEntityType).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.RelatedEntityType));
        RuleFor(x => x.ActionUrl).MaximumLength(250).When(x => !string.IsNullOrWhiteSpace(x.ActionUrl));
        RuleFor(x => x.TerminalActionCode).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.TerminalActionCode));
    }
}

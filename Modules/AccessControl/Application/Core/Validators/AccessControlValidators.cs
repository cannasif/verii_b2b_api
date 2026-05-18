using FluentValidation;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;

namespace Wms.Application.AccessControl.Validators;

public sealed class CreatePermissionDefinitionDtoValidator : AbstractValidator<CreatePermissionDefinitionDto>
{
    public CreatePermissionDefinitionDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionCodeRequired"))
            .MaximumLength(120).WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionCodeTooLong"));
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionNameRequired"))
            .MaximumLength(150).WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionNameTooLong"));
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class UpdatePermissionDefinitionDtoValidator : AbstractValidator<UpdatePermissionDefinitionDto>
{
    public UpdatePermissionDefinitionDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Code).MaximumLength(120).When(x => !string.IsNullOrWhiteSpace(x.Code));
        RuleFor(x => x.Name).MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Name));
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}

public sealed class SyncPermissionDefinitionsDtoValidator : AbstractValidator<SyncPermissionDefinitionsDto>
{
    public SyncPermissionDefinitionsDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Items)
            .NotNull().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.SyncItemsRequired"))
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.SyncItemsRequireAtLeastOne"));
        RuleForEach(x => x.Items).SetValidator(new SyncPermissionDefinitionItemDtoValidator(loc));
    }
}

public sealed class SyncPermissionDefinitionItemDtoValidator : AbstractValidator<SyncPermissionDefinitionItemDto>
{
    public SyncPermissionDefinitionItemDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionCodeRequired"))
            .MaximumLength(120).WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionCodeTooLong"));
        RuleFor(x => x.Name).MaximumLength(150).When(x => !string.IsNullOrWhiteSpace(x.Name));
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}

public sealed class CreatePermissionGroupDtoValidator : AbstractValidator<CreatePermissionGroupDto>
{
    public CreatePermissionGroupDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionGroupNameRequired"))
            .MaximumLength(100).WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionGroupNameTooLong"));
        RuleFor(x => x.Description).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Description));
    }
}

public sealed class UpdatePermissionGroupDtoValidator : AbstractValidator<UpdatePermissionGroupDto>
{
    public UpdatePermissionGroupDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.Name).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Name));
        RuleFor(x => x.Description).MaximumLength(500).When(x => x.Description != null);
    }
}

public sealed class SetPermissionGroupPermissionsDtoValidator : AbstractValidator<SetPermissionGroupPermissionsDto>
{
    public SetPermissionGroupPermissionsDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.PermissionDefinitionIds)
            .NotNull().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionDefinitionIdsRequired"));
    }
}

public sealed class SetUserPermissionGroupsDtoValidator : AbstractValidator<SetUserPermissionGroupsDto>
{
    public SetUserPermissionGroupsDtoValidator(ILocalizationService loc)
    {
        RuleFor(x => x.PermissionGroupIds)
            .NotNull().WithMessage(loc.GetLocalizedString("ValidatorAccessControl.PermissionGroupIdsRequired"));
    }
}

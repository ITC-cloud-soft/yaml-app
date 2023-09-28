using FluentValidation;

namespace Yaml.Application;

public class SaveYalAppInfoCommandValidator : AbstractValidator<SaveYamlAppCommand>
{
    public SaveYalAppInfoCommandValidator()
    {
        // RuleFor(v => v.name)
        //     .MaximumLength(4)
        //     .NotEmpty();
    }
}
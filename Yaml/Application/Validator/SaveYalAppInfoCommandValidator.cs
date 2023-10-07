using FluentValidation;
using Yaml.Application.Command;

namespace Yaml.Application.Validator;

public class SaveYalAppInfoCommandValidator : AbstractValidator<SaveYamlAppCommand>
{
    public SaveYalAppInfoCommandValidator()
    {
        // RuleFor(v => v.name)
        //     .MaximumLength(4)
        //     .NotEmpty();
    }
}
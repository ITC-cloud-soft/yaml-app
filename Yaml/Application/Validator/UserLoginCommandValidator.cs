using FluentValidation;
using Yaml.Application.Command;

namespace Yaml.Application.Validator;

public class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
{
    public UserLoginCommandValidator()
    {
        RuleFor(user => user.Name)
             .MaximumLength(4)
            .NotEmpty();
    }
}
using FluentValidation;

namespace DevHabit.Api.DTOs.Tags;

public sealed class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
{
    public CreateTagDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tag name is required.")
            .MinimumLength(3).WithMessage("Tag name must be at least 3 characters.")
            .MaximumLength(50).WithMessage("Tag name must not exceed 50 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(200).WithMessage("Tag description must not exceed 200 characters.");
    }
}

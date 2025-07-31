using FEwS.Forums.Domain.Exceptions;
using FluentValidation;

namespace FEwS.Forums.Domain.UseCases.CreateComment;

internal class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(c => c.TopicId)
            .NotEmpty().WithErrorCode(ValidationErrorCode.Empty);
        RuleFor(c => c.Text)
            .NotEmpty().WithErrorCode(ValidationErrorCode.Empty);
    }
}
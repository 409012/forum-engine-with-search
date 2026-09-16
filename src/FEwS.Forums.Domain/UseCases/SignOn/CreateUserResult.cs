namespace FEwS.Forums.Domain.UseCases.SignOn;

public abstract record CreateUserResult
{
    public sealed record Created(Guid UserId) : CreateUserResult;
    public sealed record DuplicateUserName : CreateUserResult;
}

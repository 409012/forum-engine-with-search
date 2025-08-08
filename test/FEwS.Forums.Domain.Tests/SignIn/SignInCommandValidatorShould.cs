using FluentAssertions;
using FEwS.Forums.Domain.UseCases.SignIn;
using Xunit;

namespace FEwS.Forums.Domain.Tests.SignIn;

public class SignInCommandValidatorShould
{
    private readonly SignInCommandValidator sut = new();

    [Fact]
    public void ReturnSuccessWhenCommandValid()
    {
        var command = new SignInCommand("Test", "qwerty");
        sut.Validate(command).IsValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetInvalidCommands()
    {
        var command = new SignInCommand("Test", "qwerty");
        yield return new object[] { command with { UserName = string.Empty } };
        yield return new object[] { command with { UserName = "  " } };
        yield return new object[] { command with { UserName = "123456789012345678901" } };
        yield return new object[] { command with { Password = "      " } };
        yield return new object[] { command with { Password = string.Empty } };
    }

    [Theory]
    [MemberData(nameof(GetInvalidCommands))]
    public void ReturnFailureWhenCommandInvalid(SignInCommand command)
    {
        sut.Validate(command).IsValid.Should().BeFalse();
    }
}
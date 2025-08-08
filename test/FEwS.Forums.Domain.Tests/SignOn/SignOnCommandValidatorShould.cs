using FluentAssertions;
using FEwS.Forums.Domain.UseCases.SignOn;
using Xunit;

namespace FEwS.Forums.Domain.Tests.SignOn;

public class SignOnCommandValidatorShould
{
    private readonly SignOnCommandValidator sut = new();

    [Fact]
    public void ReturnSuccessWhenCommandValid()
    {
        var validCommand = new SignOnCommand("Test", "qwerty");
        sut.Validate(validCommand).IsValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetInvalidCommands()
    {
        var validCommand = new SignOnCommand("Test", "qwerty");
        yield return new object[] { validCommand with { UserName = string.Empty } };
        yield return new object[] { validCommand with { UserName = "123456789012345678901" } };
        yield return new object[] { validCommand with { UserName = "   " } };
        yield return new object[] { validCommand with { Password = string.Empty } };
        yield return new object[] { validCommand with { Password = "     " } };
    }

    [Theory]
    [MemberData(nameof(GetInvalidCommands))]
    public void ReturnFailureWhenCommandInvalid(SignOnCommand command)
    {
        sut.Validate(command).IsValid.Should().BeFalse();
    }
}
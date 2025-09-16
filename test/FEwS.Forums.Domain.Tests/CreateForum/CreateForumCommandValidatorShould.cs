using FluentAssertions;
using FEwS.Forums.Domain.UseCases.CreateForum;
using Xunit;

namespace FEwS.Forums.Domain.Tests.CreateForum;

public class CreateForumCommandValidatorShould
{
    private readonly CreateForumCommandValidator sut = new();

    [Fact]
    public void ReturnSuccessWhenCommandValid()
    {
        var validCommand = new CreateForumCommand("Title");
        sut.Validate(validCommand).IsValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetInvalidCommands()
    {
        yield return [new CreateForumCommand(Title: string.Empty)];
        yield return
        [
            new CreateForumCommand(Title: "123456789012345678901234567890123456789012345678901")
        ];
    }
    
    [Theory]
    [MemberData(nameof(GetInvalidCommands))]
    public void ReturnFailureWhenCommandInvalid(CreateForumCommand command)
    {
        sut.Validate(command).IsValid.Should().BeFalse();
    }
}
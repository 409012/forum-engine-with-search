using FluentAssertions;
using Moq;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization;
using Xunit;

namespace FEwS.Forums.Domain.Tests.Authorization;

public class AccountIntentionResolverShould
{
    private readonly AccountIntentionResolver sut = new();
    
    [Fact]
    public void ReturnFalseWhenIntentionNotInEnum()
    {
        var intention = (AccountIntention) (-1);
        sut.IsAllowed(new Mock<IIdentity>().Object, intention).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseWhenCheckingForumCreateIntentionAndUserIsGuest()
    {
        sut.IsAllowed(User.Guest, AccountIntention.SignOut).Should().BeFalse();
    }

    [Fact]
    public void ReturnTrueWhenCheckingForumCreateIntentionAndUserIsAuthenticated()
    {
        sut.IsAllowed(new User(Guid.Parse("6F5C56BD-25EB-4BDC-9604-F19DAE2963A4"), Guid.Empty), AccountIntention.SignOut)
            .Should().BeTrue();
    }
}
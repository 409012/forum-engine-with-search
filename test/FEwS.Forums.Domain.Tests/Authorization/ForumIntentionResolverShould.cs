using FluentAssertions;
using Moq;
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization;
using Xunit;

namespace FEwS.Forums.Domain.Tests.Authorization;

public class ForumIntentionResolverShould
{
    private readonly ForumIntentionResolver sut = new();
    
    [Fact]
    public void ReturnFalseWhenIntentionNotInEnum()
    {
        var intention = (ForumIntention) (-1);
        sut.IsAllowed(new Mock<IIdentity>().Object, intention).Should().BeFalse();
    }

    [Fact]
    public void ReturnFalseWhenCheckingForumCreateIntentionAndUserIsGuest()
    {
        sut.IsAllowed(User.Guest, ForumIntention.Create).Should().BeFalse();
    }

    [Fact]
    public void ReturnTrueWhenCheckingForumCreateIntentionAndUserIsAuthenticated()
    {
        sut.IsAllowed(new User(Guid.Parse("6F5C56BD-25EB-4BDC-9604-F19DAE2963A4"), Guid.Empty), ForumIntention.Create)
            .Should().BeTrue();
    }
}
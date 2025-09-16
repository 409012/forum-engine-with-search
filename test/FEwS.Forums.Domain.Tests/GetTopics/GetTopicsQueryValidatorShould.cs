using FluentAssertions;
using FEwS.Forums.Domain.UseCases.GetTopics;
using Xunit;

namespace FEwS.Forums.Domain.Tests.GetTopics;

public class GetTopicsQueryValidatorShould
{
    private readonly GetTopicsQueryValidator sut = new();

    [Fact]
    public void ReturnSuccessWhenQueryIsValid()
    {
        var query = new GetTopicsQuery(
            Guid.Parse("DA60E33E-7F32-4BFC-A4FF-E19F9BFE934B"),
            10,
            5);
        sut.Validate(query).IsValid.Should().BeTrue();
    }

    public static IEnumerable<object[]> GetInvalidQuery()
    {
        var query = new GetTopicsQuery(Guid.Parse("DA60E33E-7F32-4BFC-A4FF-E19F9BFE934B"), 10, 5);
        yield return [query with { ForumId = Guid.Empty }];
        yield return [query with { Skip = -40 }];
        yield return [query with { Take = -1 }];
    }

    [Theory]
    [MemberData(nameof(GetInvalidQuery))]
    public void ReturnFailureWhenQueryIsInvalid(GetTopicsQuery query)
    {
        sut.Validate(query).IsValid.Should().BeFalse();
    }
}
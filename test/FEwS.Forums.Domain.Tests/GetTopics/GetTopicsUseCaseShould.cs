using FluentAssertions;
using Moq;
using Moq.Language.Flow;
using FEwS.Forums.Domain.Exceptions;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;
using FEwS.Forums.Domain.UseCases.GetTopics;
using Xunit;

namespace FEwS.Forums.Domain.Tests.GetTopics;

public class GetTopicsUseCaseShould
{
    private readonly GetTopicsUseCase sut;
    private readonly Mock<IGetTopicsStorage> storage;
    private readonly ISetup<IGetForumsStorage,Task<IEnumerable<Forum>>> getForumsSetup;
    private readonly ISetup<IGetTopicsStorage, Task<TopicsPagedResult>> getTopicsSetup;

    public GetTopicsUseCaseShould()
    {
        var getForumsStorage = new Mock<IGetForumsStorage>();
        getForumsSetup = getForumsStorage.Setup(s => s.GetForumsAsync(It.IsAny<CancellationToken>()));

        storage = new Mock<IGetTopicsStorage>();
        getTopicsSetup = storage.Setup(s =>
            s.GetTopicsAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()));

        sut = new GetTopicsUseCase(getForumsStorage.Object, storage.Object);
    }

    [Fact]
    public async Task ThrowForumNotFoundExceptionWhenNoForum()
    {
        var forumId = Guid.Parse("64C3B227-8D4A-4A0E-A161-04F19C2ABBC4");

        getForumsSetup.ReturnsAsync([
            new()
            {
                Id = Guid.Parse("01B1C554-184B-4B32-913E-F7031AAD3BAC"),
                Title = "A Forum"
            }
        ]);

        var query = new GetTopicsQuery(forumId, 0, 1);
        await sut.Invoking(s => s.Handle(query, CancellationToken.None))
            .Should().ThrowAsync<ForumNotFoundException>();
    }

    [Fact]
    public async Task ReturnTopicsExtractedFromStorageWhenForumExists()
    {
        var forumId = Guid.Parse("845D0972-0E11-4BD2-A102-299E99590267");

        getForumsSetup.ReturnsAsync([
            new()
            {
                Id = Guid.Parse("845D0972-0E11-4BD2-A102-299E99590267"),
                Title = "A Forum"
            }
        ]);
        var expectedResources = new TopicReadModel[] { new()
            {
                Title = "A Topic",
                LastComment = null,
                TotalCommentsCount = 0
            }
        };
        var expectedTotalCount = 6;
        getTopicsSetup.ReturnsAsync(new TopicsPagedResult(expectedResources, expectedTotalCount));

        var (actualResources, actualTotalCount) = await sut.Handle(
            new GetTopicsQuery(forumId, 5, 10), CancellationToken.None);

        actualResources.Should().BeEquivalentTo(expectedResources);
        actualTotalCount.Should().Be(expectedTotalCount);
        storage.Verify(s => s.GetTopicsAsync(forumId, 5, 10, It.IsAny<CancellationToken>()), Times.Once);
    }
}
using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization;
using FEwS.Forums.Domain.Authorization.AccessManagement;
using FEwS.Forums.Domain.DomainEvents;
using FEwS.Forums.Domain.Models;
using FEwS.Forums.Domain.UseCases.GetForums;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.CreateTopic;

internal class CreateTopicUseCase(
    IIntentionManager intentionManager,
    IIdentityProvider identityProvider,
    IGetForumsStorage getForumsStorage,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateTopicCommand, Topic>
{
    public async Task<Topic> Handle(CreateTopicCommand command, CancellationToken cancellationToken)
    {
        (Guid forumId, string title) = command;
        intentionManager.ThrowIfForbidden(TopicIntention.Create);

        await getForumsStorage.ThrowIfForumNotFound(forumId, cancellationToken);

        await using IUnitOfWorkScope scope = await unitOfWork.StartScope(cancellationToken);
        Topic topic = await scope.GetStorage<ICreateTopicStorage>()
            .CreateTopicAsync(forumId, identityProvider.Current.UserId, title, cancellationToken);
        await scope.GetStorage<IDomainEventStorage>()
            .AddEventAsync(ForumDomainEvent.TopicCreated(topic), cancellationToken);
        await scope.CommitAsync(cancellationToken);

        return topic;
    }
}
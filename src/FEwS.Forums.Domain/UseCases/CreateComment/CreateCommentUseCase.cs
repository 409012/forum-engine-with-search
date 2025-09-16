using FEwS.Forums.Domain.Authentication;
using FEwS.Forums.Domain.Authorization;
using FEwS.Forums.Domain.Authorization.AccessManagement;
using FEwS.Forums.Domain.DomainEvents;
using FEwS.Forums.Domain.Exceptions;
using FEwS.Forums.Domain.Models;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.CreateComment;

internal class CreateCommentUseCase(
    IIntentionManager intentionManager,
    IIdentityProvider identityProvider,
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCommentCommand, Comment>
{
    public async Task<Comment> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        await using IUnitOfWorkScope scope = await unitOfWork.StartScope(cancellationToken);
        ICreateCommentStorage storage = scope.GetStorage<ICreateCommentStorage>();

        Topic? topic = await storage.FindTopicAsync(request.TopicId, cancellationToken);
        if (topic is null)
        {
            throw new TopicNotFoundException(request.TopicId);
        }

        intentionManager.ThrowIfForbidden(TopicIntention.CreateComment, topic);

        IDomainEventStorage domainEventsStorage = scope.GetStorage<IDomainEventStorage>();
        Comment comment = await storage.CreateCommentAsync(
            request.TopicId, identityProvider.Current.UserId, request.Text, cancellationToken);
        await domainEventsStorage.AddEventAsync(ForumDomainEvent.CommentCreated(topic, comment), cancellationToken);

        await scope.CommitAsync(cancellationToken);

        return comment;
    }
}
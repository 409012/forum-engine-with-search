using FEwS.Forums.Domain.Authorization;
using FEwS.Forums.Domain.Authorization.AccessManagement;
using FEwS.Forums.Domain.Models;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.CreateForum;

internal class CreateForumUseCase(
    IIntentionManager intentionManager,
    ICreateForumStorage storage)
    : IRequestHandler<CreateForumCommand, Forum>
{
    public async Task<Forum> Handle(CreateForumCommand command, CancellationToken cancellationToken)
    {
        intentionManager.ThrowIfForbidden(ForumIntention.Create);
        return await storage.CreateForumAsync(command.Title, cancellationToken);
    }
}
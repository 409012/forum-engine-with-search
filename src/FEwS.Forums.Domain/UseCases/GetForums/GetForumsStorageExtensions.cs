using FEwS.Forums.Domain.Exceptions;

namespace FEwS.Forums.Domain.UseCases.GetForums;

internal static class GetForumsStorageExtensions
{
    private static async Task<bool> ForumExists(this IGetForumsStorage storage, Guid forumId,
        CancellationToken cancellationToken)
    {
        IEnumerable<Models.Forum> forums = await storage.GetForumsAsync(cancellationToken);
        return forums.Any(f => f.Id == forumId);
    }

    public static async Task ThrowIfForumNotFound(this IGetForumsStorage storage, Guid forumId,
        CancellationToken cancellationToken)
    {
        if (!await ForumExists(storage, forumId, cancellationToken))
        {
            throw new ForumNotFoundException(forumId);
        }
    }
}
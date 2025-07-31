using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.CreateForum;

public interface ICreateForumStorage
{
    public Task<Forum> CreateForumAsync(string title, CancellationToken cancellationToken);
}
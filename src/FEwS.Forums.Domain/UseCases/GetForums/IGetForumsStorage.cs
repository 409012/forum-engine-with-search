using FEwS.Forums.Domain.Models;

namespace FEwS.Forums.Domain.UseCases.GetForums;

public interface IGetForumsStorage
{
    Task<IEnumerable<Forum>> GetForumsAsync(CancellationToken cancellationToken);
}
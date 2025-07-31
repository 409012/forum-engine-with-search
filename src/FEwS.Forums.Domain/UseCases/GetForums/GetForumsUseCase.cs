using FEwS.Forums.Domain.Models;
using MediatR;

namespace FEwS.Forums.Domain.UseCases.GetForums;

internal class GetForumsUseCase(
    IGetForumsStorage storage) : IRequestHandler<GetForumsQuery, IEnumerable<Forum>>
{
    public Task<IEnumerable<Forum>> Handle(GetForumsQuery query, CancellationToken cancellationToken) => 
        storage.GetForumsAsync(cancellationToken);
}
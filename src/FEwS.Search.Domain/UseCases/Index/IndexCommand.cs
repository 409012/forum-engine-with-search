using FEwS.Search.Domain.Models;
using MediatR;

namespace FEwS.Search.Domain.UseCases.Index;

public record IndexCommand(Guid EntityId, SearchEntityType EntityType, string? Title, string? Text) : IRequest;
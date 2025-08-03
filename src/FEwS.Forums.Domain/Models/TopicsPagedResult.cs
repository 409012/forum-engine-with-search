namespace FEwS.Forums.Domain.Models;

public record TopicsPagedResult(IEnumerable<TopicReadModel> Resources, int TotalCount);
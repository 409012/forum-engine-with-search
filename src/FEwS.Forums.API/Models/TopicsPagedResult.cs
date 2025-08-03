namespace FEwS.Forums.API.Models;

public record TopicsPagedResult(IEnumerable<TopicReadModel> Resources, int TotalCount);
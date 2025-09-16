using System.Diagnostics;

namespace FEwS.Search.ForumConsumer.Monitoring;

public static class ForumConsumerMetrics
{
    public static string ApplicationName { get; set; } = "FEwS.Search.ForumConsumer";

    internal static readonly ActivitySource ActivitySource = new(ApplicationName);
}
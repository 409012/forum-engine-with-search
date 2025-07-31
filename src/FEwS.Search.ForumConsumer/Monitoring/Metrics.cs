using System.Diagnostics;

namespace FEwS.Search.ForumConsumer.Monitoring;

public class Metrics
{
    public const string ApplicationName = "FEwS.Search.ForumConsumer";
    internal static readonly ActivitySource ActivitySource = new(ApplicationName);
}
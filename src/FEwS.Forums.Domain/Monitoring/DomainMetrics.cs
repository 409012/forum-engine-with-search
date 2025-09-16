using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FEwS.Forums.Domain.Monitoring;

public class DomainMetrics(IMeterFactory meterFactory)
{
    public static string ApplicationName { get; set; } = "FEwS.Forums.Domain";

    private readonly Meter meter = meterFactory.Create(ApplicationName);
    private readonly ConcurrentDictionary<string, Counter<int>> counters = new();

    internal static readonly ActivitySource ActivitySource = new(ApplicationName);

    public void IncrementCount(string name, int value, IDictionary<string, object?>? additionalTags = null)
    {
        Counter<int> counter = counters.GetOrAdd(name, _ => meter.CreateCounter<int>(name));
        counter.Add(value, additionalTags?.ToArray() ?? ReadOnlySpan<KeyValuePair<string, object?>>.Empty);
    }

    public static IDictionary<string, object?> ResultTags(bool success)
    {
        return new Dictionary<string, object?>
        {
            ["success"] = success
        };
    }
}
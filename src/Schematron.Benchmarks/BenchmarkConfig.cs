using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;

namespace Schematron.Benchmarks;

/// <summary>
/// Shared BenchmarkDotNet configuration: short-run job + GitHub-markdown and HTML reports.
/// </summary>
class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddJob(Job.Default
            .WithWarmupCount(2)
            .WithIterationCount(10));

        AddLogger(ConsoleLogger.Default);
        AddColumnProvider(DefaultColumnProviders.Instance);
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(HtmlExporter.Default);
    }
}

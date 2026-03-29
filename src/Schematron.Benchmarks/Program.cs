using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using Schematron.Benchmarks;

// Run in Release; BenchmarkDotNet will warn and refuse to produce reliable
// numbers in Debug mode.
BenchmarkRunner.Run(
[
    BenchmarkConverter.TypeToBenchmarks(typeof(PurchaseOrderBenchmark)),
    BenchmarkConverter.TypeToBenchmarks(typeof(BookCatalogBenchmark)),
]);


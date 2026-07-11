using Microsoft.Extensions.Hosting;

namespace GapMiner.ServiceDefaults;

/// <summary>
/// Extension methods to register shared Aspire service defaults
/// (OpenTelemetry, health checks, resilience, service discovery) on an application host.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds the shared service defaults to the specified builder.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The builder, for chaining.</returns>
    /// <remarks>
    /// TODO(T-0.3): wire up OpenTelemetry tracing/metrics/logging,
    /// health-check endpoints (<c>/health</c>, <c>/alive</c>),
    /// service discovery, and HTTP client resilience once the relevant
    /// Aspire/Telemetry packages are added to central package management.
    /// </remarks>
    public static IHostApplicationBuilder AddServiceDefaults(this IHostApplicationBuilder builder)
    {
        return builder;
    }
}

using ShopSphere.IntegrationTests.Common;
using ShopSphere.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ShopSphere.IntegrationTests;

[Collection("Containers")]
public sealed class HealthTests(ContainerFixture containers)
    : IntegrationTestBase(containers)
{
    [Fact]
    public async Task Ready_should_return_success()
    {
        HttpResponseMessage response = await Client.GetAsync("/health");
        for (var attempt = 1; attempt < 20 && !response.IsSuccessStatusCode; attempt++)
        {
            response.Dispose();
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            response = await Client.GetAsync("/health");
        }

        var body = await response.Content.ReadAsStringAsync();
        var report = await Factory.Services
            .GetRequiredService<HealthCheckService>()
            .CheckHealthAsync();
        var checks = string.Join(
            "; ",
            report.Entries.Select(entry =>
                $"{entry.Key}={entry.Value.Status}: {entry.Value.Exception?.Message}"));
        
        response.IsSuccessStatusCode.Should().BeTrue(
            "Readiness response: {0}; Checks: {1}", body, checks);
    }
}
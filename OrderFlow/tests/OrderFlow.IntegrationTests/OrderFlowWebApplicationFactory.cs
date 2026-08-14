using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace OrderFlow.IntegrationTests;

/// <summary>Boots the real Api pipeline against a unique, disposable SQLite file per test class.</summary>
public class OrderFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"orderflow-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:OrderFlow"] = $"Data Source={_databasePath}"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}

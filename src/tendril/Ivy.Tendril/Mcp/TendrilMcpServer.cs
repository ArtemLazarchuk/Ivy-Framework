using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Ivy.Tendril.Mcp;

public class TendrilMcpServer
{
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            var builder = Host.CreateEmptyApplicationBuilder(settings: null);

            builder.Services
                .AddMcpServer(options =>
                {
                    options.ServerInfo = new()
                    {
                        Name = "tendril",
                        Version = typeof(TendrilMcpServer).Assembly.GetName().Version?.ToString() ?? "1.0.0"
                    };
                })
                .WithStdioServerTransport()
                .WithToolsFromAssembly();

            var host = builder.Build();
            await host.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"MCP server error: {ex.Message}");
            return 1;
        }
    }
}

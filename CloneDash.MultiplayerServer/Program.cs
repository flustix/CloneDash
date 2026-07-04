using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Midori.Logging;
using System.Net;
using System.Threading.Tasks;
using Midori.Networking;
using Midori.Utils.Extensions;

namespace CloneDash.MultiplayerServer;

public static class Program
{
	public static async Task Main() {
		HostApplicationBuilder builder = new();
		builder.Logging.ClearProviders();
		builder.Logging.AddProvider(new MidoriLoggerProvider());
		builder.Services.AddHttpServer(c => {
			c.Address = IPAddress.Any;
			c.Port = 6942;
		});

		IHost host = builder.Build();
		HttpRouter router = host.Services.GetRequiredService<HttpRouter>();
		MultiplayerSocket.Sockets = router.MapModule<MultiplayerSocket>("/", manager: true)!;

		await host.RunAsync();
	}
}
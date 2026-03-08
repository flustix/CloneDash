using System.Net;
using System.Threading.Tasks;
using Midori.Networking;

namespace CloneDash.MultiplayerServer;

public static class Program
{
    public static async Task Main()
    {
        var http = new HttpServer();
        MultiplayerSocket.Sockets = http.MapModule<MultiplayerSocket>("/");
        http.Start(IPAddress.Any, 6942);

        await Task.Delay(-1);
    }
}
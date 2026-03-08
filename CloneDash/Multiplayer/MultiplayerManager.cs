using JetBrains.Annotations;
using Nucleus;
using Nucleus.Commands;
using Nucleus.Common.Commands;

namespace CloneDash.Multiplayer;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public static class MultiplayerManager
{
    public static Action? OnConnect;
    public static Action? OnDisconnect;

    public static bool Connected => Client != null;
    public static MultiplayerClient? Client { get; private set; }

    [ConCommand(Help: "Connect to a multiplayer server.")]
    public static void mp_connect(ConCommand cmd, in TokenizedCommand args)
    {
        if (Client is not null)
        {
            Logs.Warn("Already connected.");
            return;
        }

        var name = new string(args.Arg(1));
        var ip = new string(args.Arg(2));

        if (string.IsNullOrEmpty(name))
        {
            Logs.Warn("Usage: mp_connect <name> (ip)");
            return;
        }

        if (string.IsNullOrEmpty(ip)) ip = "ws://localhost:6942";

        Client = new MultiplayerClient(ip);
        Client.OnClose += () =>
        {
            Client = null;
            Logs.Warn("Disconnected from MP.");
            OnDisconnect?.Invoke();
        };

        try
        {
            Client.Connect(name);
            Logs.Info($"Connected to {ip}.");
            OnConnect?.Invoke();
        }
        catch (Exception ex)
        {
            Logs.Error($"Failed to connect: {ex.GetType().Name} - {ex.Message}");
        }
    }

    [ConCommand(Help: "Lists the players in the current lobby.")]
    public static void mp_players(ConCommand cmd, in TokenizedCommand args)
    {
        if (Client is null)
        {
            Logs.Warn("Not connected.");
            return;
        }

        foreach (var player in Client.Players)
        {
            Logs.Info($"{player.Name} ({player.ID}) - ch:{player.Character}");
        }
    }

    [ConCommand(Help: "Leaves the current lobby.")]
    public static void mp_leave(ConCommand cmd, in TokenizedCommand args)
    {
        if (Client is null)
        {
            Logs.Warn("Not connected.");
            return;
        }

        Client.Close();
    }
}
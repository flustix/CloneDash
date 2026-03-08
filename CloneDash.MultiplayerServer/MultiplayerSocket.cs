using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CloneDash.Multiplayer;
using CloneDash.Multiplayer.Exceptions;
using Midori.Logging;
using Midori.Networking;
using Midori.Networking.WebSockets.Typed;
using OdinSerializer.Utilities;

namespace CloneDash.MultiplayerServer;

public class MultiplayerSocket : TypedWebSocketSession<IMultiplayerServer, IMultiplayerClient>, IMultiplayerServer
{
    public static HttpConnectionManager<MultiplayerSocket> Sockets { get; set; } = null!;
    public List<MultiplayerSocket> Connected => Sockets.Where(x => x.Status > PlayerStatus.Uninitialized).ToList();

    public PlayerStatus Status { get; set; } = PlayerStatus.Uninitialized;
    public string Name { get; set; } = string.Empty;
    public string Character { get; set; } = string.Empty;
    public bool Host { get; set; }

    public Task<string> JoinLobby(string name, string character)
    {
        Name = name;
        Character = character;
        Connected.ForEach(x => x.Client.PlayerJoined(CreateJson()));
        Status = PlayerStatus.Idle;

        if (!Sockets.Any(x => x.Host))
            Host = true;

        Connected.ForEach(x =>
        {
            Client.PlayerJoined(x.CreateJson());
            if (x.Host) Client.UpdateHost(x.ID);
        });

        Logger.Log($"{ID} joined. c:{Character} h:{Host}");
        return Task.FromResult(ID);
    }

    public Task UpdateCharacter(string id)
    {
        Character = id;
        Logger.Log($"{ID} switched character to {Character}.");

        Connected.ForEach(x => x.Client.PlayerCharacterChange(ID, id));
        return Task.CompletedTask;
    }

    public Task StartMap(string id, int diff)
    {
        if (!Host) throw new NotHostException();
        
        Logger.Log($"Starting {id}:{diff}.");
        Connected.ForEach(x => x.Client.LoadMap(id, diff));
        return Task.CompletedTask;
    }

    private MultiplayerClient.Player CreateJson() => new()
    {
        ID = ID,
        Name = Name,
        Character = Character
    };
}
using CloneDash.Characters;
using CloneDash.Common;
using CloneDash.Common.Gamemodes;
using CloneDash.Compatibility.MuseDash;
using CloneDash.Game;
using Midori.Networking.WebSockets;
using Midori.Networking.WebSockets.Frame;
using Midori.Networking.WebSockets.Typed;
using Newtonsoft.Json;
using Nucleus;

namespace CloneDash.Multiplayer;

public class MultiplayerClient : IMultiplayerClient
{
	public event Action<Player>? OnPlayerJoined;
	public event Action<Player>? OnPlayerUpdate;
	public event Action<Player>? OnPlayerLeft;
	public event Action? OnClose;

	public bool IsHost => _roomHost == PlayerId;
	public List<Player> Players { get; } = new();

	private readonly string _ip;
	private readonly TypedWebSocketClient<IMultiplayerServer, IMultiplayerClient> _connection;

	public string PlayerId { get; private set; } = string.Empty;
	private string _roomHost = string.Empty;

	public MultiplayerClient(string ip) {
		_ip = ip;

		_connection = new TypedWebSocketClient<IMultiplayerServer, IMultiplayerClient>(this) { PingInterval = 30000 };
		_connection.OnClose += Close;
	}

	public void Connect(string name) {
		try {
			_connection.Connect(_ip);

			CharacterMod.CharacterUpdated += UpdateCharacter;

			PlayerId = _connection.Server.JoinLobby(name, new string(CharacterMod.character.GetString())).Result;
		}
		catch (Exception ex) {
			throw new AggregateException(_connection.CloseReason, ex);
		}
	}

	public void Close() => MainThread.RunASAP(() => {
		if (_connection.State == WebSocketState.Open) {
			_connection.Close(WebSocketCloseCode.NormalClosure, "user left.");
			return;
		}

		CharacterMod.CharacterUpdated -= UpdateCharacter;
		OnClose?.Invoke();
	});

	private void UpdateCharacter(ICharacterDescriptor? desc)
		=> _connection.Server.UpdateCharacter(new string(desc!.GetUUID()) ?? "");

	public Task StartMap(string id, int diff) => _connection.Server.StartMap(id, diff);

	#region IMultiplayerClient Implementation

	Task IMultiplayerClient.PlayerJoined(Player player) {
		MainThread.RunASAP(() => {
			Players.Add(player);
			Logs.Info($"Player {player.Name} ({player.ID}) has joined. [{player.Character}]");
			OnPlayerJoined?.Invoke(player);
		});

		return Task.CompletedTask;
	}

	public Task PlayerCharacterChange(string id, string character) {
		MainThread.RunASAP(() => {
			var player = Players.FirstOrDefault(x => x.ID == id);
			if (player is null) return;

			Logs.Info($"{player.Name} changed their character to {character}.");
			player.Character = character;
			OnPlayerUpdate?.Invoke(player);
		});

		return Task.CompletedTask;
	}

	Task IMultiplayerClient.UpdateHost(string id) {
		MainThread.RunASAP(() => {
			var player = Players.FirstOrDefault(x => x.ID == id);
			if (player is null) return;

			Logs.Info($"Host has been transferred to {player.Name} ({player.ID}).");
		});

		return Task.FromResult(_roomHost = id);
	}

	Task IMultiplayerClient.PlayerLeft(string id) {
		MainThread.RunASAP(() => {
			var player = Players.FirstOrDefault(x => x.ID == id);
			Players.RemoveAll(x => x.ID == id);

			if (player is null)
				return;

			Logs.Info($"Player {player.Name} ({player.ID}) has left.");
			OnPlayerLeft?.Invoke(player);
		});

		return Task.CompletedTask;
	}

	Task IMultiplayerClient.LoadMap(string id, int diff) {
		MainThread.RunASAP(() => {
			MD1_Song? song = MuseDash1Compatibility.Songs.FirstOrDefault(x => x.GetUUID().SequenceEqual(id.AsSpan()));
			if (song is null) {
				Logs.Error($"Could not find any song with ID {id}.");
				return;
			}

			LevelTransitions.LoadSongChart(
				$"Loading '{song.FetchMetadata(HumanLanguage.GetCurrentLanguage()).Name}'...",
				song.GetSheet(diff),
				new GameLoadGenericParameters()
			);
		});

		return Task.CompletedTask;
	}

	#endregion

	public class Player
	{
		[JsonProperty("id")] public string ID { get; init; } = string.Empty;
		[JsonProperty("name")] public string Name { get; set; } = string.Empty;
		[JsonProperty("character")] public string Character { get; set; } = string.Empty;
	}
}
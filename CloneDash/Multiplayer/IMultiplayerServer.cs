namespace CloneDash.Multiplayer;

public interface IMultiplayerServer
{
	Task<string> JoinLobby(string name, string character);
	Task UpdateCharacter(string id);
	Task StartMap(string id, int diff);
}
namespace CloneDash.Multiplayer;

public interface IMultiplayerClient
{
    Task PlayerJoined(MultiplayerClient.Player player);
    Task PlayerCharacterChange(string id, string character);
    Task UpdateHost(string id);
    Task PlayerLeft(string id);
    
    Task LoadMap(string id, int diff);
}
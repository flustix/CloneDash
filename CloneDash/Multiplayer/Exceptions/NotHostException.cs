using Midori.Networking.WebSockets.Typed;

namespace CloneDash.Multiplayer.Exceptions;

public class NotHostException : TypedWebSocketException
{
    public NotHostException()
        : base("You are not the host of this lobby.")
    {
    }
}
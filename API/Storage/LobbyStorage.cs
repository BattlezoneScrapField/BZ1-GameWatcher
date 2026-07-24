using BZAPI.Models;

namespace BZAPI.Storage
{
    public static class LobbyStorage
    {
        public static ICollection<BZ98Lobby>? Lobbies { get; set; }
    }
}

using BZAPI.Models;

namespace BZAPI.Storage
{
    /// <summary>
    /// Immutable point-in-time view of the lobby list.
    /// </summary>
    /// <param name="Lobbies">The lobbies, safe to enumerate without locking.</param>
    /// <param name="LastUpdatedUtc">When the lobby list was last changed, or null if never.</param>
    public sealed record LobbySnapshot(IReadOnlyList<BZ98Lobby> Lobbies, DateTimeOffset? LastUpdatedUtc)
    {
        public static readonly LobbySnapshot Empty = new([], null);
    }

    public interface ILobbyStore
    {
        /// <summary>
        /// The current snapshot. Never null; safe to read from any thread.
        /// </summary>
        LobbySnapshot Current { get; }

        void Replace(IEnumerable<BZ98Lobby> lobbies);

        void AddOrUpdate(BZ98Lobby lobby);

        void Remove(int lobbyId);
    }

    /// <summary>
    /// Holds the lobby list produced by the websocket watcher and read by HTTP requests.
    /// </summary>
    /// <remarks>
    /// Writes take a lock and publish a brand new list; readers take the current snapshot with a
    /// single atomic reference read. This means a reader can never observe a half-applied update,
    /// which previously caused intermittent "Collection was modified" failures during JSON
    /// serialisation.
    ///
    /// Lobby objects must be fully populated *before* being handed to this store — once published
    /// they are treated as immutable, because readers may be serialising them at any moment.
    /// </remarks>
    public sealed class LobbyStore : ILobbyStore
    {
        private readonly object _writeLock = new();
        private LobbySnapshot _current = LobbySnapshot.Empty;

        public LobbySnapshot Current => Volatile.Read(ref _current);

        public void Replace(IEnumerable<BZ98Lobby> lobbies)
        {
            ArgumentNullException.ThrowIfNull(lobbies);

            lock (_writeLock)
            {
                Publish(lobbies.ToList());
            }
        }

        public void AddOrUpdate(BZ98Lobby lobby)
        {
            ArgumentNullException.ThrowIfNull(lobby);

            lock (_writeLock)
            {
                var updated = _current.Lobbies.ToList();
                var index = updated.FindIndex(l => l.Id == lobby.Id);

                if (index >= 0)
                {
                    updated[index] = lobby;
                }
                else
                {
                    updated.Add(lobby);
                }

                Publish(updated);
            }
        }

        public void Remove(int lobbyId)
        {
            lock (_writeLock)
            {
                var updated = _current.Lobbies.Where(l => l.Id != lobbyId).ToList();

                if (updated.Count == _current.Lobbies.Count)
                {
                    return;
                }

                Publish(updated);
            }
        }

        private void Publish(List<BZ98Lobby> lobbies) =>
            Volatile.Write(ref _current, new LobbySnapshot(lobbies, DateTimeOffset.UtcNow));
    }
}

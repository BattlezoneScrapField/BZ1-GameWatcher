using BZAPI.Models;
using Newtonsoft.Json;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using Websocket.Client;

namespace BZAPI.Websocket
{
    public class BZ98WebsocketClient
    {
        private readonly Uri url = new("ws://battlezone98mp.webdev.rebellion.co.uk:1337/");
        private readonly ulong ChaosSteamId = 76561198104781489;

        public async Task StartWebsocketClientAsync()
        {
            // Create our Steam stuff as well for queries.
            var steamWebInterfaceFactory = new SteamWebInterfaceFactory("YOUR_STEAM_KEY");
            var steamUserInterface = steamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
            var exitEvent = new ManualResetEvent(false);

            try
            {
                using (var client = new WebsocketClient(url))
                {
                    client.MessageReceived.Subscribe(async msg =>
                    {
                        // Log that we have processed a message so we know from face value in the console on the server.
                        Console.WriteLine($"Processing message: {msg}");

                        var websocketMessage = JsonConvert.DeserializeObject<WebsocketGenericMessage>(msg.Text);

                        // Handle each message depending on the type.
                        switch (websocketMessage?.Type)
                        {
                            case nameof(WebsocketMessageType.OnAuthorization):
                                // For authorisation, we need to ask for the lobby data.
                                WebsocketBoolMessage websocketBoolMessage = new()
                                {
                                    Type = "DoEnterLounge",
                                    Content = true
                                };

                                string loungeMsg = JsonConvert.SerializeObject(websocketBoolMessage);

                                await Task.Run(() => client.Send(loungeMsg));
                                break;
                            case nameof(WebsocketMessageType.OnLobbyListChanged):
                            case nameof(WebsocketMessageType.OnLobbyChanged):
                                var lobbyDataMsg = JsonConvert.DeserializeObject<WebsocketLobbyMessage>(msg.Text);

                                if (lobbyDataMsg == null)
                                {
                                    break;
                                }

                                if (lobbyDataMsg.Data == null)
                                {
                                    break;
                                }

                                if (lobbyDataMsg.Data.BZ98Lobbies == null)
                                {
                                    break;
                                }

                                var lobbies = lobbyDataMsg.Data.BZ98Lobbies.Select(l => l.Value);

                                switch (websocketMessage.Type)
                                {
                                    case nameof(WebsocketMessageType.OnLobbyListChanged):
                                        Storage.LobbyStorage.Lobbies = lobbies.ToList();
                                        break;
                                    case nameof(WebsocketMessageType.OnLobbyChanged):
                                        var lobbyToChange = lobbies.FirstOrDefault();
                                        UpdateLobby(lobbyToChange);
                                        break;
                                }

                                // Process each lobby.
                                foreach (var lobby in lobbies)
                                {
                                    if (lobby.MetaData?.Name != null)
                                    {
                                        // Check to see what mod is being played.
                                        lobby.MetaData.Name = lobby.MetaData.Name[(lobby.MetaData.Name.IndexOf("~~") + 2)..];
                                    }

                                    if (lobby.Users == null || lobby.Users.Count == 0)
                                    {
                                        continue;
                                    }

                                    foreach (var user in lobby.Users)
                                    {
                                        if (user.Value?.IPAddress == "::ffff:54.200.83.68")
                                        {
                                            lobby.Users.Remove(user.Key);
                                        }

                                        if (user.Value == null)
                                        {
                                            continue;
                                        }

                                        if (user.Key[0] != 'S') 
                                        {
                                            user.Value.IsGOG = true;
                                            continue; 
                                        }

                                        // Mark the user as a Steam user.
                                        user.Value.IsSteam = true;

                                        var trimmedId = user.Key.TrimStart('S');

                                        // Steam ID should match so we need to parse it.
                                        ulong steamId = ulong.Parse(trimmedId);

                                        // Store the clean Steam ID in the player object.
                                        user.Value.SteamCleanId = trimmedId;

                                        // Check to see if this is the "dangerous" user.
                                        user.Value.IsDangerous = steamId == ChaosSteamId;

                                        // Query steam for their profile pictures.
                                        var playerSummaryResponse = await steamUserInterface.GetPlayerSummaryAsync(steamId);

                                        // Update the user with their new Steam Image.
                                        user.Value.SteamImgUri = playerSummaryResponse.Data.AvatarFullUrl;

                                        // Check if the user is the host. If they are, we can store that in the lobby data.
                                        if (user.Value.Id == lobby.Owner)
                                        {
                                            lobby.Host = user.Value;
                                        }
                                    }
                                }

                                break;
                            case nameof(WebsocketMessageType.OnLobbyRemoved):
                                var lobbyToDelete = JsonConvert.DeserializeObject<WebsocketIntMessage>(msg.Text);

                                if (lobbyToDelete == null)
                                {
                                    break;
                                }

                                if (lobbyToDelete.Data == null)
                                {
                                    break;
                                }

                                DeleteLobby(lobbyToDelete.Data.Id);
                                break;
                            default:
                                break;
                        }
                    });

                    WebsocketAuthMessage message = new()
                    {
                        Type = "Authorization",
                        Content = new()
                        {
                            AuthType = "web",
                            Key = string.Empty,
                            Id = "0",
                            apiVer = "0.0"
                        }
                    };

                    client.ReconnectTimeout = null;
                    client.DisconnectionHappened.Subscribe(d => Task.Run(() => OnWebsocketDisconnect(d)));
                    await client.Start();

                    await Task.Run(() => client.Send(JsonConvert.SerializeObject(message)));

                    exitEvent.WaitOne();
                }
            }
            catch (Exception ex)
            {
                WriteConsoleError(ex);
            }
        }

        private static void OnWebsocketDisconnect(DisconnectionInfo d)
        {
            WriteConsoleError(d.Exception);
        }

        private static void AddLobby(BZ98Lobby? lobbyData)
        {
            if (Storage.LobbyStorage.Lobbies == null || Storage.LobbyStorage.Lobbies.Count <= 0 || lobbyData == null)
            {
                return;
            }

            Storage.LobbyStorage.Lobbies.Add(lobbyData);
        }

        private static void UpdateLobby(BZ98Lobby? lobbyData)
        {
            if (Storage.LobbyStorage.Lobbies == null || Storage.LobbyStorage.Lobbies.Count <= 0 || lobbyData == null)
            {
                return;
            }

            var lobby = Storage.LobbyStorage.Lobbies.FirstOrDefault(l => l.Id == lobbyData.Id);

            if (lobby != null)
            {
                Storage.LobbyStorage.Lobbies.Remove(lobby);
            }

            AddLobby(lobbyData);
        }

        private static void DeleteLobby(int lobbyId)
        {
            if (Storage.LobbyStorage.Lobbies == null || Storage.LobbyStorage.Lobbies.Count <= 0)
            {
                return;
            }

            var lobby = Storage.LobbyStorage.Lobbies.FirstOrDefault(l => l.Id == lobbyId);

            if (lobby == null)
            {
                return;
            }

            Storage.LobbyStorage.Lobbies.Remove(lobby);
        }

        private static void WriteConsoleError(Exception message)
        {
            Console.WriteLine(message);
        }
    }
}

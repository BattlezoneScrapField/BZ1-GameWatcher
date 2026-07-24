using BZAPI.Models;
using Newtonsoft.Json;

namespace BZAPI.Websocket
{
    public enum WebsocketMessageType
    {
        OnAuthorization = 0,
        OnLobbyListChanged = 1,
        OnLobbyChanged  = 2,
        OnLobbyRemoved = 3
    }

    public class WebsocketGenericMessage
    {
        [JsonProperty("type")]
        public string? Type { get; set; }
    }

    public class WebsocketBoolMessage : WebsocketGenericMessage
    {
        [JsonProperty("content")]
        public bool Content { get; set; }
    }

    public class WebsocketIntMessage : WebsocketGenericMessage
    {
        [JsonProperty("data")]
        public WebSocketIntData? Data { get; set; }
    }

    public class WebSocketIntData
    {
        [JsonProperty("id")]
        public int Id { get; set; }
    }

    public class WebsocketAuthMessage : WebsocketGenericMessage
    {
        [JsonProperty("content")]
        public WebsocketAuthMessageContent? Content { get; set; }
    }

    public class WebsocketLobbyMessage : WebsocketGenericMessage
    {
        [JsonProperty("data")]
        public WebsocketLobbyData? Data { get; set; }
    }

    public class WebsocketLobbyData
    {
        [JsonProperty("lobbies")]
        public Dictionary<string, BZ98Lobby>? BZ98Lobbies { get; set; }
    }

    public class WebsocketAuthMessageContent
    {
        [JsonProperty("authtype")]
        public string? AuthType { get; set; }

        [JsonProperty("key")]
        public string? Key { get; set; }

        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("apiVer")]
        public string? ApiVer { get; set; }
    }
}

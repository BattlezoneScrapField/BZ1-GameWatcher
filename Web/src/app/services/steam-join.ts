import { environment } from '../../environments/environment';

/**
 * Builds the steam:// URL that launches Battlezone 98 Redux straight into a lobby.
 *
 * Shared by the games list and the /join/:lobbyId deep link so the two cannot drift apart.
 */
export function buildSteamJoinUrl(lobbyId: string | number): string {
    return `steam://rungame/${environment.steamAppId}/${environment.steamRunGameOwnerId}/+connect_lobby=B${lobbyId}`;
}

export interface BZ98Lobby
{
    clientVersion: string;
    createdTime: string;
    id: string;
    isChat: boolean;
    isLocked: boolean;
    isPrivate: boolean;
    host: BZ98User;
    memberLimit: number;
    metaData: BZ98MetaData;
    stats: BZ98LobbyData;
    owner: string;
    userCount: number;
    users: BZ98User[];
    oddTeamUsers: BZ98User[];
    evenTeamUsers: BZ98User[];
    directJoinUrl: string;
}

export interface BZ98MetaData
{
    gameVersion: string;
    gameSettings: string;
    gameType: string;
    launched: boolean;
    name: string;
    nextMid: string;
    userCount: string;
    userPack: string;
}

export interface BZ98User
{
    authType: string;
    clientVersion: string;
    id: string;
    ipAddress: string;
    isAdmin: boolean;
    isAuth: boolean;
    isBB: boolean;
    isDangerous: boolean;
    isInLounge: boolean;
    isGOG: boolean;
    isTest: boolean;
    isSteam: boolean;
    lanAddresses: string[];
    lobby: number;
    metaData: BZ98UserMetaData;
    name: string;
    stats: BZ98LobbyData;
    steamCleanId: string;
    steamImgUri: string;
    wanAddress: string;
}

export interface BZ98UserMetaData
{
    clientsconnected: string;
    friendId: string;
    knownPlayers: string;
    launched: string;
    miniid: string;
    ready: string;
    team: string;
    vehicle: string;
}

export interface BZ98LobbyData
{
    mapFile: string;
    crc32: string;
    mod: string;
    attributes: BZ98LobbyDataAttributes;
}

export interface BZ98LobbyDataAttributes
{
    lives: string;
    satellite: boolean;
    barracks: boolean;
    sniper: boolean;
    splinter: boolean;
}
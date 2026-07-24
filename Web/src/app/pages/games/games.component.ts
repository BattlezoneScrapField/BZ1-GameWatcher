import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUser, faComputer, faMessage, faPlayCircle, faLock, faLockOpen, faCirclePlay, faLink } from '@fortawesome/free-solid-svg-icons';
import { faDiscord, faYoutube } from '@fortawesome/free-brands-svg-icons';
import { Subscription } from 'rxjs';
import { RouterOutlet } from '@angular/router';
import { BZ98Lobby } from '../../models/bz98-lobby-info';
import { BZ98Service } from '../../services/bz98.service';

@Component({
    selector: 'app-games',
    standalone: true,
    imports: [CommonModule, FontAwesomeModule, RouterOutlet],
    templateUrl: './games.component.html',
    styleUrl: './games.component.scss'
})
export class GamesComponent implements OnInit, OnDestroy {
    title = 'BZ1 Game Watcher';

    // Core variable. If this exists, don't call the API.
    lobbyId: string;

    refreshInterval;

    faLink = faLink;
    faYouTube = faYoutube;
    faDiscord = faDiscord;
    faUser = faUser;
    faComputer = faComputer;
    faMessage = faMessage;
    faPlayCircle = faPlayCircle;
    faLock = faLock;
    faLockOpen = faLockOpen;
    faCirclePlay = faCirclePlay;

    BZ98Lobbies: BZ98Lobby[];
    BZ98ChatLobbies: BZ98Lobby[];
    BZ98MaxPlayerSlots: number = 15;

    BZ98GetLobbiesSubscription: Subscription;

    constructor(private bz98Service: BZ98Service) {

    }

    ngOnInit(): void {
        this.getLobbyData();
        this.refreshInterval = setInterval(() => { this.getLobbyData(); }, 3000);
    }

    ngOnDestroy(): void {
        if (this.BZ98GetLobbiesSubscription) this.BZ98GetLobbiesSubscription.unsubscribe();
        if (this.refreshInterval) clearInterval(this.refreshInterval);
    }

    getLobbyData() {
        console.log("Refreshing Lobby Data...");

        this.bz98Service.getBZ98Lobbies().subscribe({
            next: (data) => {
                // Make sure the users are formatted to an array as well.
                for (let i = 0; i < data.length; i++) {
                    const lobby = data[i];
                    lobby.users = this.toArray(lobby.users);

                    // Split the users into teams based on their team number.
                    lobby.oddTeamUsers = lobby.users.filter(u => u.metaData.team % 2);
                    lobby.evenTeamUsers = lobby.users.filter(u => !(u.metaData.team % 2))
                }

                this.BZ98ChatLobbies = data.filter(l => l.isChat);
                this.BZ98Lobbies = data.filter(l => l.isChat === false);

                this.BZ98Lobbies.forEach((game) => {
                    if (game.metaData.gameSettings) {
                        game.stats = this.parseGameSettings(game.metaData.gameSettings);
                    }
                });
            }
        })
    }

    toArray(data) {
        return Object.keys(data).map(key => data[key]);
    }

    parseGameSettings(settings: string) {
        const parts = settings.split('*');

        return {
            mapFile: parts[1],
            crc32: parts[2],
            mod: parts[3],
            attributes: {
                lives: parts[8],
                satellite: Boolean(Number(parts[4])),
                barracks: Boolean(Number(parts[5])),
                sniper: Boolean(Number(parts[6])),
                splinter: Boolean(Number(parts[7])),
            },
        };
    }

    shareToDiscord(button: HTMLButtonElement, lobby: BZ98Lobby) {
        // Thanks to Sev for this solution.
        let textArea = button.querySelector('textarea');
        textArea.textContent = `${lobby.userCount}/${lobby.memberLimit} https://${window.location.host}/join/${lobby.id} @BZ1 Expert @BZ1 Novice`
        textArea.focus();
        textArea.select();

        document.execCommand('copy');

        window.location.replace('discord:///channels/1046303222165418078/1102709555391103127');
    }
}

import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { SiteNavComponent } from '../../components/site-nav/site-nav.component';
import { buildSteamJoinUrl } from '../../services/steam-join';

/** How long the launch page stays up before returning to the game list. */
const REDIRECT_DELAY_MS = 2000;

@Component({
    selector: 'app-join-game',
    standalone: true,
    imports: [CommonModule, RouterLink, SiteNavComponent],
    templateUrl: './join-game.component.html',
    styleUrl: './join-game.component.scss'
})
export class JoinGameComponent implements OnInit, OnDestroy {
    private redirectTimer?: ReturnType<typeof setTimeout>;

    constructor(private readonly route: ActivatedRoute, private readonly router: Router) {
    }

    ngOnInit(): void {
        const lobbyId = this.route.snapshot.paramMap.get('lobbyId');

        // A missing or non-numeric id would otherwise be pasted straight into the steam:// URL.
        if (lobbyId === null || !/^\d+$/.test(lobbyId)) {
            void this.router.navigate(['games']);
            return;
        }

        window.location.href = buildSteamJoinUrl(lobbyId);

        this.redirectTimer = setTimeout(() => void this.router.navigate(['games']), REDIRECT_DELAY_MS);
    }

    ngOnDestroy(): void {
        // Without this, navigating away early still bounced the user back to the game list.
        if (this.redirectTimer !== undefined) {
            clearTimeout(this.redirectTimer);
        }
    }
}

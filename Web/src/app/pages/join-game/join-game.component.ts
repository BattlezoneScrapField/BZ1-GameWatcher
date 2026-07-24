import { CommonModule } from "@angular/common";
import { Component, OnInit } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";
import { faDiscord, faYoutube } from "@fortawesome/free-brands-svg-icons";

@Component({
    selector: 'app-join-game',
    standalone: true,
    imports: [CommonModule, FontAwesomeModule],
    templateUrl: './join-game.component.html',
    styleUrl: './join-game.component.scss'
})
export class JoinGameComponent implements OnInit {
    faYouTube = faYoutube;
    faDiscord = faDiscord;

    // Core variable. If this exists, don't call the API.
    lobbyId: string;

    constructor(private route: ActivatedRoute, private router: Router) {

    }

    ngOnInit(): void {
        this.lobbyId = this.route.snapshot.paramMap.get('lobbyId');
        window.location.href = `steam://rungame/301650/76561198104781489/+connect_lobby=B${this.lobbyId}`;

        // Redirect after a little bit.
        setTimeout(() => {this.router.navigate(['games']);}, 2000);
    }    
}
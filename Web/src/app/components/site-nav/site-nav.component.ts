import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faDiscord, faYoutube } from '@fortawesome/free-brands-svg-icons';
import { environment } from '../../../environments/environment';

/**
 * Site header. Previously this markup was duplicated in every page template, with the active
 * link maintained by hand and an href alongside routerLink that forced a full page reload.
 */
@Component({
    selector: 'app-site-nav',
    imports: [FontAwesomeModule, RouterLink, RouterLinkActive],
    templateUrl: './site-nav.component.html'
})
export class SiteNavComponent {
    readonly faYouTube = faYoutube;
    readonly faDiscord = faDiscord;
    readonly youTubeUrl = environment.youTubeUrl;
    readonly discordInviteUrl = environment.discordInviteUrl;
}

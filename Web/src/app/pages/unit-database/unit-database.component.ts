import { CommonModule } from "@angular/common";
import { Component } from "@angular/core";
import { FontAwesomeModule } from "@fortawesome/angular-fontawesome";
import { faDiscord, faYoutube } from "@fortawesome/free-brands-svg-icons";

@Component({
    selector: 'app-unit-database',
    standalone: true,
    imports: [CommonModule, FontAwesomeModule],
    templateUrl: './unit-database.component.html',
    styleUrl: './unit-database.component.scss'
})
export class UnitDatabaseComponent {
    faYouTube = faYoutube;
    faDiscord = faDiscord;
}
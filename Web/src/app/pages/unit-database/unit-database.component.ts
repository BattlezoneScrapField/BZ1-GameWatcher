import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { SiteNavComponent } from '../../components/site-nav/site-nav.component';

/** Placeholder for the planned unit database; not yet linked from the navigation. */
@Component({
    selector: 'app-unit-database',
    imports: [CommonModule, SiteNavComponent],
    templateUrl: './unit-database.component.html',
    styleUrl: './unit-database.component.scss'
})
export class UnitDatabaseComponent {
}

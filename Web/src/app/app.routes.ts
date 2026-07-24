import { Routes } from '@angular/router';
import { GamesComponent } from './pages/games/games.component';
import { UnitDatabaseComponent } from './pages/unit-database/unit-database.component';
import { JoinGameComponent } from './pages/join-game/join-game.component';

export const routes: Routes = [
    {
        path: 'games',
        component: GamesComponent
    },
    {
        path: 'join/:lobbyId',
        component: JoinGameComponent
    },
    {
        path: 'unit-database',
        component: UnitDatabaseComponent
    },
    {
        path: '',
        redirectTo: '/games',
        pathMatch: 'full'
    },
    {
        path: '**',
        redirectTo: '/games',
        pathMatch: 'full'
    }
];

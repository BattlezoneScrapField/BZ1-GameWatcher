import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed, discardPeriodicTasks, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { environment } from '../../../environments/environment';
import { BZ98Lobby, BZ98User } from '../../models/bz98-lobby-info';
import { GamesComponent } from './games.component';

const LOBBIES_URL = `${environment.apiUrl}BZ98Lobby`;

function user(overrides: Partial<BZ98User>): BZ98User {
    return {
        authType: null,
        clientVersion: null,
        id: null,
        isAdmin: false,
        isAuth: false,
        isBB: false,
        isDangerous: false,
        isInLounge: false,
        isGOG: false,
        isTest: false,
        isSteam: false,
        lobby: 0,
        metaData: null,
        name: null,
        stats: null,
        steamCleanId: null,
        steamImgUri: null,
        ...overrides
    };
}

function lobby(overrides: Partial<BZ98Lobby>): BZ98Lobby {
    return {
        id: 1,
        clientVersion: null,
        createdTime: '2024-01-01T00:00:00+00:00',
        isChat: false,
        isLocked: false,
        isPrivate: false,
        host: null,
        memberLimit: 10,
        metaData: null,
        stats: null,
        owner: null,
        userCount: 0,
        users: {},
        directJoinUrl: null,
        ...overrides
    };
}

describe('GamesComponent', () => {
    let fixture: ComponentFixture<GamesComponent>;
    let httpMock: HttpTestingController;

    beforeEach(async () => {
        await TestBed.configureTestingModule({
            imports: [GamesComponent],
            providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
        }).compileComponents();

        fixture = TestBed.createComponent(GamesComponent);
        httpMock = TestBed.inject(HttpTestingController);
    });

    function load(lobbies: BZ98Lobby[]): void {
        fixture.detectChanges();
        tick();
        httpMock.expectOne(LOBBIES_URL).flush(lobbies);
        fixture.detectChanges();
    }

    function teardown(): void {
        fixture.destroy();
        discardPeriodicTasks();
        httpMock.verify();
    }

    it('separates chat lobbies from game lobbies', fakeAsync(() => {
        load([
            lobby({ id: 1, isChat: false }),
            lobby({ id: 2, isChat: true })
        ]);

        expect(fixture.componentInstance.BZ98Lobbies.map(l => l.id)).toEqual([1]);
        expect(fixture.componentInstance.BZ98ChatLobbies.map(l => l.id)).toEqual([2]);

        teardown();
    }));

    it('splits users into odd and even team columns', fakeAsync(() => {
        load([
            lobby({
                users: {
                    S1: user({ name: 'odd', metaData: { team: '1' } as never }),
                    S2: user({ name: 'even', metaData: { team: '2' } as never }),
                    S3: user({ name: 'unassigned', metaData: null })
                }
            })
        ]);

        const view = fixture.componentInstance.BZ98Lobbies[0];

        expect(view.oddTeamUsers.map(u => u.name)).toEqual(['odd']);
        expect(view.evenTeamUsers.map(u => u.name)).toEqual(['even', 'unassigned']);

        teardown();
    }));

    it('ignores game settings that are too short to parse', fakeAsync(() => {
        load([
            lobby({ metaData: { gameSettings: '*' } as never, stats: null })
        ]);

        // Previously this produced a stats object full of undefined fields.
        expect(fixture.componentInstance.BZ98Lobbies[0].stats).toBeNull();

        teardown();
    }));

    it('parses a full game settings string', fakeAsync(() => {
        load([
            lobby({ metaData: { gameSettings: 'x*bunker.bzn*ABC123*stock*1*0*1*0*5' } as never })
        ]);

        const stats = fixture.componentInstance.BZ98Lobbies[0].stats;

        expect(stats?.mapFile).toBe('bunker.bzn');
        expect(stats?.crc32).toBe('ABC123');
        expect(stats?.attributes?.satellite).toBeTrue();
        expect(stats?.attributes?.barracks).toBeFalse();
        expect(stats?.attributes?.lives).toBe('5');

        teardown();
    }));

    it('keeps polling after a failed request', fakeAsync(() => {
        fixture.detectChanges();
        tick();
        httpMock.expectOne(LOBBIES_URL).flush('boom', { status: 500, statusText: 'Server Error' });
        fixture.detectChanges();

        expect(fixture.componentInstance.loadFailed).toBeTrue();

        // The previous implementation lost its subscription on the first error and never
        // recovered; the next tick must still issue a request.
        tick(environment.lobbyRefreshIntervalMs);
        httpMock.expectOne(LOBBIES_URL).flush([lobby({ id: 7 })]);
        fixture.detectChanges();

        expect(fixture.componentInstance.loadFailed).toBeFalse();
        expect(fixture.componentInstance.BZ98Lobbies.map(l => l.id)).toEqual([7]);

        teardown();
    }));
});

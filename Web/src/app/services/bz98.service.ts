import { Injectable } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from '../../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class BZ98Service {
    constructor(private _httpClient: HttpClient) {

    }
    
    getBZ98Lobbies() : Observable<any> {
        return this._httpClient.get(environment.apiUrl + 'BZ98Lobby');
    }
}
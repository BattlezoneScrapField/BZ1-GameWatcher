import { Component, OnDestroy, OnInit } from '@angular/core';
import { BZ98Lobby } from './models/bz98-lobby-info';
import { CommonModule } from '@angular/common';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faUser, faComputer, faMessage, faPlayCircle, faLock, faLockOpen, faCirclePlay } from '@fortawesome/free-solid-svg-icons';
import { Subscription } from 'rxjs';
import { BZ98Service } from './services/bz98.service';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FontAwesomeModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'BZ1 Game Watcher';
}

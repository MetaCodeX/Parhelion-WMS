import { Component, ElementRef, ViewChild, AfterViewInit } from '@angular/core';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [],
  template: `
    <div class="app">
      <!-- Animated Grid Background -->
      <div #gridBg class="grid-background"></div>

      <main class="content-layer">
        <h1 class="font-logo">Parhelion</h1>
        <p class="logo-subtitle">Logistics</p>
        <p class="subtitle">Gestión Administrativa</p>
        <p class="status">En desarrollo</p>

        <a 
          href="https://github.com/MetaCodeX/Parhelion-Logistics" 
          target="_blank" 
          rel="noopener noreferrer"
          class="btn btn-primary"
        >
          <svg width="20" height="20" fill="currentColor" viewBox="0 0 24 24">
            <path d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z"/>
          </svg>
          Ver en GitHub
        </a>

        <div class="buttons">
          <button class="btn btn-oxide">Acción</button>
          <button class="btn btn-primary">Configurar</button>
        </div>
      </main>

      <footer class="content-layer">
        <p class="portfolio">Parhelion Logistics - MetaCodeX Portfolio</p>
        <p class="credits">UI: <a href="https://github.com/ekmas/neobrutalism-components" target="_blank">neobrutalism-components</a></p>
      </footer>
    </div>
  `,
  styles: [`
    .app {
      min-height: 100vh;
      background-color: var(--parhelion-sand);
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      position: relative;
      overflow: hidden;
    }
    
    main {
      text-align: center;
      max-width: 500px;
    }
    
    h1 {
      font-size: 3rem;
      margin-bottom: 0.5rem;
    }
    
    .font-logo {
      font-family: var(--font-logo);
    }
    
    .logo-subtitle {
      font-family: var(--font-logo);
      font-size: 2rem;
      color: var(--parhelion-oxide);
      margin: 0 0 1rem 0;
    }
    
    .subtitle {
      font-family: var(--font-heading);
      font-size: 1.25rem;
      color: #666;
      margin: 0 0 1.5rem 0;
    }
    
    .status {
      color: #666;
      margin-bottom: 2rem;
    }
    
    .btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
    }
    
    .buttons {
      display: flex;
      gap: 1rem;
      justify-content: center;
      margin-top: 2rem;
    }
    
    footer {
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      padding: 1rem;
      text-align: center;
    }
    
    .portfolio {
      color: #666;
      font-size: 0.875rem;
      margin: 0;
    }
    
    .credits {
      color: #999;
      font-size: 0.75rem;
      margin-top: 0.25rem;
    }
    
    .credits a {
      color: var(--parhelion-oxide);
      text-decoration: none;
    }
    
    .credits a:hover {
      text-decoration: underline;
    }
  `]
})
export class AppComponent implements AfterViewInit {
  @ViewChild('gridBg') gridBg!: ElementRef<HTMLDivElement>;

  ngAfterViewInit() {
    const directions = [
      { x: 150, y: 0 },
      { x: -150, y: 0 },
      { x: 0, y: 150 },
      { x: 0, y: -150 },
      { x: 120, y: 120 },
      { x: -120, y: 120 },
      { x: 120, y: -120 },
      { x: -120, y: -120 }
    ];
    
    const randomDir = directions[Math.floor(Math.random() * directions.length)];
    const randomDuration = 20 + Math.random() * 20;
    
    const el = this.gridBg.nativeElement;
    el.style.setProperty('--grid-x', `${randomDir.x}px`);
    el.style.setProperty('--grid-y', `${randomDir.y}px`);
    el.style.setProperty('--grid-duration', `${randomDuration}s`);
  }
}

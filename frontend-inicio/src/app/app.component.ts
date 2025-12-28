import { Component, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="app">
      <!-- Animated Grid Background -->
      <div #gridBg class="grid-background"></div>

      <!-- MARQUEE - Announcement Banner -->
      <div class="marquee-container">
        <div class="marquee">
          <span>★ PARHELION v0.6.0-alpha RELEASED</span>
          <span>•</span>
          <span>Python Analytics Service</span>
          <span>•</span>
          <span>FastAPI + SQLAlchemy</span>
          <span>•</span>
          <span>Clean Architecture</span>
          <span>•</span>
          <span>Multi-Service Docker</span>
          <span>•</span>
          <span>★ PARHELION v0.6.0-alpha RELEASED</span>
          <span>•</span>
          <span>Python Analytics Service</span>
          <span>•</span>
          <span>FastAPI + SQLAlchemy</span>
          <span>•</span>
          <span>Clean Architecture</span>
          <span>•</span>
          <span>Multi-Service Docker</span>
          <span>•</span>
        </div>
      </div>

      <!-- HERO SECTION -->
      <header class="hero content-layer">
        <div class="hero-content">
          <div class="float">
            <h1 class="logo">Parhelion</h1>
            <p class="logo-subtitle">Logistics</p>
          </div>
          
          <div class="badges">
            <span class="badge badge-new">NEW v0.6.0</span>
            <span class="badge badge-oxide">Python + .NET</span>
            <span class="badge">Multi-tenant</span>
          </div>

          <p class="tagline">Plataforma Unificada de Logística B2B</p>
          <p class="description">WMS + TMS | Flotillas Tipificadas | Red Hub & Spoke | Documentación Legal SAT</p>

          <!-- ALERT - Development Notice -->
          <div class="alert alert-oxide">
            <span class="alert-icon">⚠</span>
            <div>
              <strong>Development Preview</strong> - Sistema en desarrollo activo. 
              <a href="https://github.com/MetaCodeX/Parhelion-Logistics" target="_blank">Ver GitHub</a>
            </div>
          </div>

          <!-- APP BUTTONS -->
          <div class="app-buttons">
            <a href="https://phadmin.macrostasis.lat" class="btn btn-oxide btn-lg">
              <svg width="22" height="22" fill="currentColor" viewBox="0 0 24 24">
                <path d="M19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zm-5 14H7v-2h7v2zm3-4H7v-2h10v2zm0-4H7V7h10v2z"/>
              </svg>
              Panel Admin
            </a>
            
            <a href="https://phops.macrostasis.lat" class="btn btn-primary btn-lg">
              <svg width="22" height="22" fill="currentColor" viewBox="0 0 24 24">
                <path d="M12 2C8.13 2 5 5.13 5 9c0 5.25 7 13 7 13s7-7.75 7-13c0-3.87-3.13-7-7-7zm0 9.5c-1.38 0-2.5-1.12-2.5-2.5s1.12-2.5 2.5-2.5 2.5 1.12 2.5 2.5-1.12 2.5-2.5 2.5z"/>
              </svg>
              Operaciones
            </a>
            
            <a href="https://phdriver.macrostasis.lat" class="btn btn-primary btn-lg">
              <svg width="22" height="22" fill="currentColor" viewBox="0 0 24 24">
                <path d="M18.92 6.01C18.72 5.42 18.16 5 17.5 5h-11c-.66 0-1.21.42-1.42 1.01L3 12v8c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-1h12v1c0 .55.45 1 1 1h1c.55 0 1-.45 1-1v-8l-2.08-5.99zM6.5 16c-.83 0-1.5-.67-1.5-1.5S5.67 13 6.5 13s1.5.67 1.5 1.5S7.33 16 6.5 16zm11 0c-.83 0-1.5-.67-1.5-1.5s.67-1.5 1.5-1.5 1.5.67 1.5 1.5-.67 1.5-1.5 1.5zM5 11l1.5-4.5h11L19 11H5z"/>
              </svg>
              Driver App
            </a>
          </div>
        </div>
      </header>

      <!-- FEATURES SECTION with TABS -->
      <section class="features-section content-layer">
        <h2 class="section-title">Características <span>Principales</span></h2>
        
        <div class="tabs-list">
          <button 
            class="tab-trigger" 
            [class.active]="activeTab === 'core'"
            (click)="activeTab = 'core'"
          >Core</button>
          <button 
            class="tab-trigger" 
            [class.active]="activeTab === 'fleet'"
            (click)="activeTab = 'fleet'"
          >Flotilla</button>
          <button 
            class="tab-trigger" 
            [class.active]="activeTab === 'docs'"
            (click)="activeTab = 'docs'"
          >Documentos</button>
          <button 
            class="tab-trigger" 
            [class.active]="activeTab === 'automation'"
            (click)="activeTab = 'automation'"
          >Automatización</button>
        </div>

        <div class="tab-content" [class.active]="activeTab === 'core'">
          <div class="features-grid">
            <div class="card">
              <div class="card-header">
                <h3>Multi-Tenancy</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Query Filters globales por TenantId. Aislamiento completo de datos.
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>JWT Authentication</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Roles: SuperAdmin, Admin, Driver, Warehouse. BCrypt + Argon2id.
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>Clean Architecture</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Domain-Driven Design. 25 entidades, 17 enums, Repository Pattern.
              </div>
            </div>
          </div>
        </div>

        <div class="tab-content" [class.active]="activeTab === 'fleet'">
          <div class="features-grid">
            <div class="card">
              <div class="card-header">
                <h3>Camiones Tipificados</h3>
                <span class="badge badge-oxide">5 tipos</span>
              </div>
              <div class="card-content">
                DryBox, Refrigerado, HAZMAT, Plataforma, Blindado
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>Choferes GPS</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Búsqueda geoespacial con Haversine. Endpoint /drivers/nearby
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>FleetLog Automático</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Bitácora de cambios de vehículo: ShiftChange, Breakdown, Reassignment
              </div>
            </div>
          </div>
        </div>

        <div class="tab-content" [class.active]="activeTab === 'docs'">
          <div class="features-grid">
            <div class="card">
              <div class="card-header">
                <h3>Generación Dinámica</h3>
                <span class="badge badge-new">NEW</span>
              </div>
              <div class="card-content">
                PDFs on-demand sin almacenamiento. Blob URLs estilo WhatsApp Web.
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>5 Documentos</h3>
                <span class="badge badge-oxide">SAT</span>
              </div>
              <div class="card-content">
                Orden de Servicio, Carta Porte, Manifiesto, Hoja de Ruta, POD
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>Firma Digital</h3>
                <span class="badge badge-new">NEW</span>
              </div>
              <div class="card-content">
                Captura de firma, geolocalización y timestamp en POD
              </div>
            </div>
          </div>
        </div>

        <div class="tab-content" [class.active]="activeTab === 'automation'">
          <div class="features-grid">
            <div class="card">
              <div class="card-header">
                <h3>n8n Webhooks</h3>
                <span class="badge badge-oxide">AI</span>
              </div>
              <div class="card-content">
                5 eventos: Exception, BookingRequest, Handshake, StatusChanged, Checkpoint
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>Crisis Management</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Agente IA busca chofer cercano ante excepciones automáticamente
              </div>
            </div>
            <div class="card">
              <div class="card-header">
                <h3>ServiceApiKey</h3>
                <span class="badge badge-new">✓</span>
              </div>
              <div class="card-content">
                Autenticación para agentes por tenant con SHA256
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- PROGRESS SECTION -->
      <section class="progress-section content-layer">
        <h2 class="section-title">Progreso del <span>MVP</span></h2>
        
        <div class="progress-items">
          <div class="progress-item">
            <div class="progress-label">
              <span>Backend API</span>
              <span class="badge badge-new">95%</span>
            </div>
            <div class="progress-container">
              <div class="progress-bar" style="width: 95%"></div>
            </div>
          </div>
          <div class="progress-item">
            <div class="progress-label">
              <span>Database Schema</span>
              <span class="badge badge-new">100%</span>
            </div>
            <div class="progress-container">
              <div class="progress-bar" style="width: 100%"></div>
            </div>
          </div>
          <div class="progress-item">
            <div class="progress-label">
              <span>Unit Tests</span>
              <span class="badge badge-oxide">122</span>
            </div>
            <div class="progress-container">
              <div class="progress-bar" style="width: 85%"></div>
            </div>
          </div>
          <div class="progress-item">
            <div class="progress-label">
              <span>Frontend Apps</span>
              <span class="badge badge-warning">60%</span>
            </div>
            <div class="progress-container">
              <div class="progress-bar" style="width: 60%"></div>
            </div>
          </div>
        </div>
      </section>

      <!-- CAROUSEL - Complete Changelog -->
      <section class="changelog-section content-layer">
        <h2 class="section-title">Changelog <span>Completo</span></h2>
        
        <div class="carousel card">
          <div class="carousel-track" [style.transform]="'translateX(-' + (currentSlide * 100) + '%)'">
            
            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge badge-new">NEW</span>
                  <h3>v0.6.0-alpha</h3>
                </div>
                <p class="changelog-date">2025-12-28</p>
                <ul>
                  <li>Python Analytics Service (FastAPI + SQLAlchemy)</li>
                  <li>Clean Architecture: domain, application, infrastructure</li>
                  <li>Async PostgreSQL connection (asyncpg)</li>
                  <li>Multi-service Docker deployment (9 containers)</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge badge-oxide">PDFs</span>
                  <h3>v0.5.7</h3>
                </div>
                <p class="changelog-date">2025-12-23</p>
                <ul>
                  <li>Dynamic PDF Generation (5 document types)</li>
                  <li>Checkpoint Timeline with Spanish labels</li>
                  <li>POD Signature fields (SignatureBase64)</li>
                  <li>Controllers refactored to Clean Architecture</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge badge-oxide">MAJOR</span>
                  <h3>v0.5.6</h3>
                </div>
                <p class="changelog-date">2025-12-22</p>
                <ul>
                  <li>n8n Webhook Integration (5 event types)</li>
                  <li>Push Notifications system</li>
                  <li>ServiceApiKey multi-tenant (SHA256)</li>
                  <li>Crisis Management AI Agent</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge">RULES</span>
                  <h3>v0.5.5</h3>
                </div>
                <p class="changelog-date">2025-12-18</p>
                <ul>
                  <li>Cargo-Truck compatibility validation</li>
                  <li>Refrigeration, HAZMAT, High-value constraints</li>
                  <li>Automatic FleetLog generation</li>
                  <li>122 unit tests passing</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge">SERVICES</span>
                  <h3>v0.5.2 - v0.5.4</h3>
                </div>
                <p class="changelog-date">2025-12-16</p>
                <ul>
                  <li>22 Services implemented</li>
                  <li>Swagger/OpenAPI documentation</li>
                  <li>Business Logic Workflow</li>
                  <li>72 integration tests</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge">FOUNDATION</span>
                  <h3>v0.5.0 - v0.5.1</h3>
                </div>
                <p class="changelog-date">2025-12-15</p>
                <ul>
                  <li>Repository Pattern + UnitOfWork</li>
                  <li>GenericRepository with Soft Delete</li>
                  <li>Foundation Tests (DTOs, Repository)</li>
                  <li>Services Layer architecture</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge">API</span>
                  <h3>v0.4.x</h3>
                </div>
                <p class="changelog-date">2025-12-14</p>
                <ul>
                  <li>JWT Authentication (BCrypt + Argon2id)</li>
                  <li>22 Controllers base</li>
                  <li>SuperAdmin + Employee layer</li>
                  <li>E2E Super Test infrastructure</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge">INFRA</span>
                  <h3>v0.3.x</h3>
                </div>
                <p class="changelog-date">2025-12-13</p>
                <ul>
                  <li>EF Core + PostgreSQL 17</li>
                  <li>Code First Migrations</li>
                  <li>Multi-tenancy Query Filters</li>
                  <li>24 database tables</li>
                </ul>
              </div>
            </div>

            <div class="carousel-slide">
              <div class="changelog-item">
                <div class="changelog-header">
                  <span class="badge">DOMAIN</span>
                  <h3>v0.1.0 - v0.2.0</h3>
                </div>
                <p class="changelog-date">2025-12-09</p>
                <ul>
                  <li>Requirements documentation</li>
                  <li>Database schema design</li>
                  <li>25 Domain entities</li>
                  <li>17 Enumerations</li>
                </ul>
              </div>
            </div>

          </div>
          
          <div class="carousel-nav">
            <button 
              *ngFor="let slide of [0,1,2,3,4,5,6,7,8]; let i = index"
              class="carousel-dot"
              [class.active]="currentSlide === i"
              (click)="currentSlide = i"
            ></button>
          </div>
        </div>
      </section>

      <!-- ACCORDION - Stack Info -->
      <section class="stack-section content-layer">
        <h2 class="section-title">Stack <span>Tecnológico</span></h2>
        
        <div class="accordion">
          <div class="accordion-item" [class.open]="openAccordion === 'backend'">
            <button class="accordion-trigger" (click)="toggleAccordion('backend')">
              <span>Backend (.NET 8)</span>
              <span class="icon">▼</span>
            </button>
            <div class="accordion-content">
              <div class="accordion-content-inner">
                <p><strong>.NET:</strong> ASP.NET Core 8 Web API</p>
                <p><strong>Python:</strong> FastAPI 0.115+ Analytics Service</p>
                <p><strong>ORM:</strong> EF Core + SQLAlchemy (async)</p>
                <p><strong>Database:</strong> PostgreSQL 17</p>
              </div>
            </div>
          </div>
          <div class="accordion-item" [class.open]="openAccordion === 'frontend'">
            <button class="accordion-trigger" (click)="toggleAccordion('frontend')">
              <span>Frontend (Angular + React)</span>
              <span class="icon">▼</span>
            </button>
            <div class="accordion-content">
              <div class="accordion-content-inner">
                <p><strong>Admin:</strong> Angular 18 + Material Design</p>
                <p><strong>Operaciones:</strong> React + Vite + Tailwind (PWA)</p>
                <p><strong>Driver:</strong> React + Vite + Tailwind (PWA)</p>
                <p><strong>Design:</strong> Neo-Brutalism</p>
              </div>
            </div>
          </div>
          
          <div class="accordion-item" [class.open]="openAccordion === 'infra'">
            <button class="accordion-trigger" (click)="toggleAccordion('infra')">
              <span>Infraestructura</span>
              <span class="icon">▼</span>
            </button>
            <div class="accordion-content">
              <div class="accordion-content-inner">
                <p><strong>Container:</strong> Docker + Docker Compose</p>
                <p><strong>Tunnel:</strong> Cloudflare Tunnel (Zero Trust)</p>
                <p><strong>Server:</strong> Digital Ocean Droplet (Linux)</p>
                <p><strong>Automation:</strong> n8n Workflow Engine</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- FOOTER -->
      <footer class="footer content-layer">
        <div class="footer-content">
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
          <p class="version">v0.6.0-alpha | Python Analytics Integration</p>
          <p class="portfolio">Parhelion Logistics — MetaCodeX Portfolio 2025</p>
          <p class="credits">UI: <a href="https://github.com/ekmas/neobrutalism-components" target="_blank">neobrutalism-components</a></p>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    .app {
      min-height: 100vh;
      background-color: var(--parhelion-sand);
      position: relative;
      overflow-x: hidden;
    }

    /* HERO */
    .hero {
      min-height: 90vh;
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      padding-top: 4rem;
    }

    .hero-content {
      text-align: center;
      max-width: 800px;
    }

    .logo {
      font-family: var(--font-logo);
      font-size: 5rem;
      margin: 0;
      line-height: 1;
    }

    .logo-subtitle {
      font-family: var(--font-logo);
      font-size: 3rem;
      color: var(--parhelion-oxide);
      margin: 0 0 1rem 0;
    }

    .badges {
      display: flex;
      justify-content: center;
      gap: 0.75rem;
      margin-bottom: 1.5rem;
      flex-wrap: wrap;
    }

    .tagline {
      font-family: var(--font-heading);
      font-size: 1.75rem;
      color: var(--parhelion-gray);
      margin: 0 0 0.5rem 0;
    }

    .description {
      color: #666;
      margin-bottom: 2rem;
      font-size: 1.1rem;
    }

    .alert {
      max-width: 500px;
      margin: 0 auto 2rem auto;
      text-align: left;
    }

    .alert a {
      color: var(--parhelion-oxide-dark);
      font-weight: 600;
    }

    .app-buttons {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }

    /* FEATURES */
    .features-section {
      background-color: var(--parhelion-white);
      border-top: 2px solid var(--parhelion-black);
      border-bottom: 2px solid var(--parhelion-black);
    }

    .features-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
      gap: 1.5rem;
      margin-top: 1rem;
    }

    .features-grid .card:hover {
      border-color: var(--parhelion-oxide);
    }

    .features-grid .card-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
    }

    .features-grid .card-header h3 {
      font-size: 1.1rem;
      margin: 0;
    }

    .features-grid .card-content {
      font-size: 0.95rem;
      color: #555;
    }

    /* PROGRESS */
    .progress-section {
      max-width: 700px;
      margin: 0 auto;
    }

    .progress-items {
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .progress-item {
      animation: slideIn 0.5s ease;
    }

    .progress-label {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 0.5rem;
      font-weight: 600;
    }

    /* CAROUSEL */
    .changelog-section {
      max-width: 700px;
      margin: 0 auto;
    }

    .changelog-item {
      text-align: left;
    }

    .changelog-header {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-bottom: 0.25rem;
    }

    .changelog-header h3 {
      margin: 0;
      font-size: 1.25rem;
    }

    .changelog-date {
      color: #888;
      font-size: 0.85rem;
      margin: 0 0 1rem 0;
    }

    .changelog-item ul {
      list-style: none;
      padding: 0;
    }

    .changelog-item li {
      padding: 0.5rem 0;
      border-bottom: 1px dashed #ccc;
      font-size: 0.95rem;
    }

    .changelog-item li:last-child {
      border-bottom: none;
    }

    /* ACCORDION */
    .stack-section {
      max-width: 700px;
      margin: 0 auto;
    }

    .accordion {
      border: 2px solid var(--parhelion-black);
    }

    .accordion-content-inner p {
      margin: 0.5rem 0;
      font-size: 0.95rem;
    }

    /* FOOTER */
    .footer {
      background-color: var(--parhelion-black);
      color: var(--parhelion-white);
      padding: 3rem 2rem;
      text-align: center;
      border-top: 4px solid var(--parhelion-oxide);
    }

    .footer-content {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 1rem;
    }

    .footer .btn-primary {
      background-color: var(--parhelion-white);
      color: var(--parhelion-black);
    }

    .footer .btn-primary:hover {
      background-color: var(--parhelion-oxide);
      color: var(--parhelion-white);
    }

    .version {
      color: var(--parhelion-oxide);
      font-weight: 600;
      margin: 0;
    }

    .portfolio {
      color: #aaa;
      font-size: 0.9rem;
      margin: 0;
    }

    .credits {
      color: #666;
      font-size: 0.8rem;
      margin: 0;
    }

    .credits a {
      color: var(--parhelion-oxide);
    }

    /* RESPONSIVE - Mobile First */
    
    /* Mobile (default) */
    .logo { font-size: 2.5rem; }
    .logo-subtitle { font-size: 1.5rem; }
    .tagline { font-size: 1rem; margin: 0 0 0.25rem 0; }
    .description { font-size: 0.9rem; margin-bottom: 1.5rem; }
    .hero { min-height: auto; padding: 2rem 1rem; padding-top: 3rem; }
    .hero-content { max-width: 100%; }
    .app-buttons { gap: 0.75rem; }
    .features-grid { grid-template-columns: 1fr; gap: 1rem; }
    .changelog-header h3 { font-size: 1rem; }
    .changelog-item li { font-size: 0.85rem; padding: 0.35rem 0; }
    .changelog-date { font-size: 0.75rem; }
    .progress-section, .changelog-section, .stack-section { 
      max-width: 100%; 
      padding-left: 1rem; 
      padding-right: 1rem; 
    }
    .progress-items { gap: 1rem; }
    .progress-label { font-size: 0.9rem; }
    .footer { padding: 2rem 1rem; }
    .footer .btn-primary { padding: 10px 16px; font-size: 0.75rem; }
    .portfolio { font-size: 0.8rem; }
    .credits { font-size: 0.7rem; }
    .version { font-size: 0.85rem; }
    
    /* Small phones (480px+) */
    @media (min-width: 480px) {
      .logo { font-size: 3rem; }
      .logo-subtitle { font-size: 1.75rem; }
      .tagline { font-size: 1.15rem; }
      .description { font-size: 0.95rem; }
      .hero { padding: 2.5rem 1.25rem; }
      .changelog-header h3 { font-size: 1.1rem; }
    }
    
    /* Tablets (768px+) */
    @media (min-width: 768px) {
      .logo { font-size: 4rem; }
      .logo-subtitle { font-size: 2.25rem; }
      .tagline { font-size: 1.35rem; margin: 0 0 0.5rem 0; }
      .description { font-size: 1rem; margin-bottom: 2rem; }
      .hero { min-height: 85vh; padding: 3rem 1.5rem; padding-top: 4rem; }
      .hero-content { max-width: 700px; }
      .app-buttons { gap: 1rem; }
      .features-grid { grid-template-columns: repeat(2, 1fr); gap: 1.25rem; }
      .changelog-header h3 { font-size: 1.15rem; }
      .changelog-item li { font-size: 0.9rem; padding: 0.4rem 0; }
      .changelog-date { font-size: 0.8rem; }
      .progress-section, .changelog-section, .stack-section { max-width: 600px; }
      .progress-items { gap: 1.25rem; }
      .footer { padding: 2.5rem 1.5rem; }
      .footer .btn-primary { padding: 12px 20px; font-size: 0.8rem; }
    }
    
    /* Desktop (1024px+) */
    @media (min-width: 1024px) {
      .logo { font-size: 5rem; }
      .logo-subtitle { font-size: 3rem; }
      .tagline { font-size: 1.75rem; }
      .description { font-size: 1.1rem; }
      .hero { min-height: 90vh; padding: 3rem 2rem; }
      .hero-content { max-width: 800px; }
      .features-grid { grid-template-columns: repeat(3, 1fr); gap: 1.5rem; }
      .changelog-header h3 { font-size: 1.25rem; }
      .changelog-item li { font-size: 0.95rem; padding: 0.5rem 0; }
      .changelog-date { font-size: 0.85rem; }
      .progress-section, .changelog-section, .stack-section { max-width: 700px; }
      .progress-items { gap: 1.5rem; }
      .footer { padding: 3rem 2rem; }
      .footer .btn-primary { padding: 14px 24px; font-size: 0.85rem; }
      .portfolio { font-size: 0.9rem; }
      .credits { font-size: 0.8rem; }
    }
  `]
})
export class AppComponent implements AfterViewInit {
  activeTab = 'core';
  currentSlide = 0;
  openAccordion = 'backend';

  ngAfterViewInit() {
    // Auto-rotate carousel every 6 seconds
    setInterval(() => {
      this.currentSlide = (this.currentSlide + 1) % 9;
    }, 6000);
  }

  toggleAccordion(id: string) {
    this.openAccordion = this.openAccordion === id ? '' : id;
  }
}

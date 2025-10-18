import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, NavigationEnd, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
      <div class="container">
        <a class="navbar-brand" routerLink="/compras">
          🛍️ Portal de Compras
        </a>

        <div class="navbar-collapse">
          <ul class="navbar-nav me-auto" *ngIf="isLoggedIn">
            <li class="nav-item">
              <a class="nav-link" routerLink="/compras" routerLinkActive="active">
                🏠 Inicio
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#productos">
                📦 Productos
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#carrito">
                🛒 Carrito
              </a>
            </li>
            <li class="nav-item">
              <a class="nav-link" href="#historial">
                📋 Historial
              </a>
            </li>
          </ul>
        </div>

        <!-- Sección de usuario -->
        <div class="navbar-nav ms-auto">
          <!-- Estado: Verificando -->
          <div class="nav-item" *ngIf="authStatus === 'checking'">
            <span class="nav-link text-warning">
              <span class="spinner-border spinner-border-sm me-2"></span>
              Verificando...
            </span>
          </div>

          <!-- Estado: Autenticado -->
          <div class="nav-item dropdown" *ngIf="authStatus === 'authenticated'">
            <a class="nav-link dropdown-toggle text-white" href="#" role="button" data-bs-toggle="dropdown">
              👤 {{ userName }}
            </a>
            <ul class="dropdown-menu dropdown-menu-end">
              <li><span class="dropdown-item-text small text-success">✅ Sesión activa</span></li>
              <li><hr class="dropdown-divider"></li>
              <li><a class="dropdown-item" href="#">👤 Mi Perfil</a></li>
              <li><a class="dropdown-item" href="#">📊 Dashboard</a></li>
              <li><hr class="dropdown-divider"></li>
              <li>
                <!-- LOGOUT que SÍ funciona -->
                <a class="dropdown-item text-danger fw-bold" routerLink="/logout">
                  🚪 Cerrar Sesión
                </a>
              </li>
            </ul>
          </div>

          <!-- Estado: No autenticado -->
          <div class="nav-item" *ngIf="authStatus === 'unauthenticated'">
            <button class="btn btn-outline-light btn-sm" (click)="login()">
              🔑 Iniciar Sesión
            </button>
          </div>
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .active {
      font-weight: bold;
      background-color: rgba(255,255,255,0.1);
      border-radius: 0.375rem;
    }
  `]
})
export class HeaderComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  
  authStatus: 'checking' | 'authenticated' | 'unauthenticated' = 'checking';
  isLoggedIn = false;
  userName = '';

  ngOnInit() {
    this.checkAuthStatus();
    
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.checkAuthStatus();
      });
  }

  async checkAuthStatus() {
    this.authStatus = 'checking';
    
    try {
      const loggedIn = await this.authService.isLoggedIn();
      
      if (loggedIn) {
        this.authStatus = 'authenticated';
        this.isLoggedIn = true;
        this.userName = this.authService.getUserName();
        console.log('✅ Header - Usuario autenticado:', this.userName);
      } else {
        this.authStatus = 'unauthenticated';
        this.isLoggedIn = false;
        console.log('🚫 Header - Usuario no autenticado');
      }
    } catch (error) {
      console.error('💥 Error en header:', error);
      this.authStatus = 'unauthenticated';
      this.isLoggedIn = false;
    }
  }

  async login() {
    console.log('🔑 Iniciando sesión desde header...');
    try {
      await this.authService.login();
    } catch (error) {
      console.error('❌ Error en login:', error);
    }
  }
}
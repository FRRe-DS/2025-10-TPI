import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { KeycloakService } from 'keycloak-angular';

@Component({
  selector: 'app-keycloak-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="text-center">
        <div class="spinner-border text-primary mb-3" role="status"></div>
        <h4>Autenticación completada</h4>
        <p class="text-muted">Redirigiendo al portal...</p>
        <p class="small text-success">{{ statusMessage }}</p>
        <button class="btn btn-primary btn-sm mt-3" (click)="redirectNow()">
          Ir ahora al Portal
        </button>
      </div>
    </div>
  `
})
export class KeycloakCallbackComponent implements OnInit {
  private keycloakService = inject(KeycloakService);
  private router = inject(Router);

  statusMessage: string = 'Procesando...';

  async ngOnInit() {
    console.log('🔄 Callback recibido');
    console.log('📍 URL:', window.location.href);
    
    // Simplemente redirigir después de un tiempo - Keycloak ya procesó la autenticación
    this.statusMessage = '✅ Autenticación exitosa';
    
    setTimeout(() => {
      this.redirectToPortal();
    }, 2000);
  }

  redirectToPortal() {
    console.log('📍 Redirigiendo a /compras...');
    // Limpiar la URL del callback para evitar bucles
    window.history.replaceState({}, document.title, '/');
    this.router.navigate(['/compras']);
  }

  redirectNow() {
    this.redirectToPortal();
  }
}
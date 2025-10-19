/*
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
*/

import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ManualAuthService } from '../../services/manual-auth.service';

@Component({
  selector: 'app-keycloak-callback',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="min-vh-100 d-flex align-items-center justify-content-center bg-light">
      <div class="text-center">
        <div class="spinner-border text-primary mb-3" role="status"></div>
        <h4>Procesando autenticación</h4>
        <p class="text-muted">{{ statusMessage }}</p>
        <p class="small text-info">{{ debugMessage }}</p>
      </div>
    </div>
  `
})
export class KeycloakCallbackComponent implements OnInit {
  private manualAuthService = inject(ManualAuthService);
  private router = inject(Router);

  statusMessage: string = 'Procesando...';
  debugMessage: string = '';

  async ngOnInit() {
    console.log('🔄 Callback recibido - Usando flujo manual');
    await this.processAuthentication();
  }

  private async processAuthentication(): Promise<void> {
    try {
      // Paso 1: Extraer código
      const urlParams = new URLSearchParams(window.location.search);
      const code = urlParams.get('code');

      this.statusMessage = 'Verificando código...';
      this.debugMessage = 'Extrayendo código de la URL...';

      if (!code) {
        throw new Error('No se encontró código de autorización');
      }

      this.statusMessage = 'Intercambiando código por token...';
      this.debugMessage = 'Comunicándose con Keycloak...';

      // Paso 2: Intercambiar código por token (MANUAL)
      const token = await this.manualAuthService.exchangeCodeForToken(code);
      
      if (token) {
        this.statusMessage = '✅ Autenticación exitosa!';
        this.debugMessage = 'Redirigiendo al portal...';
        
        console.log('🎉 AUTENTICACIÓN MANUAL EXITOSA');
        console.log('🔑 Token almacenado:', token.substring(0, 50) + '...');

        // Paso 3: Redirigir
        setTimeout(() => {
          window.history.replaceState({}, document.title, '/');
          this.router.navigate(['/compras']);
        }, 1000);
        
      } else {
        throw new Error('No se pudo obtener token');
      }

    } catch (error: any) {
      console.error('❌ Error en autenticación manual:', error);
      this.statusMessage = '❌ Error en autenticación';
      this.debugMessage = error.message || 'Error desconocido';
      
      setTimeout(() => {
        this.router.navigate(['/login']);
      }, 3000);
    }
  }
}
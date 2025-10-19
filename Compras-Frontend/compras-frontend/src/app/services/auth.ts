/*
import { Injectable, inject } from '@angular/core';
import { KeycloakService } from 'keycloak-angular';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private keycloakService = inject(KeycloakService);

  async isLoggedIn(): Promise<boolean> {
    try {
      return await this.keycloakService.isLoggedIn();
    } catch (error: any) {
      console.warn('⚠️ Error verificando login:', error);
      return false;
    }
  }

  login(): void {
    console.log('🔑 Redirigiendo a Keycloak...');
    
    // Login MANUAL simple - Keycloak redirigirá automáticamente al callback
    const redirectUri = encodeURIComponent('http://localhost:4200/keycloak-callback');
    const loginUrl = `http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/auth?client_id=grupo-10&redirect_uri=${redirectUri}&response_type=code&scope=openid`;
    
    console.log('📍 URL de login:', loginUrl);
    window.location.href = loginUrl;
  }

  async logout(): Promise<void> {
    try {
      await this.keycloakService.logout();
    } catch (error: any) {
      console.error('❌ Error en logout:', error);
      // Fallback manual
      window.location.href = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/logout';
    }
  }

  async getToken(): Promise<string> {
    try {
      return await this.keycloakService.getToken();
    } catch (error: any) {
      console.warn('⚠️ Error obteniendo token:', error);
      return '';
    }
  }

  getUserName(): string {
    try {
      return this.keycloakService.getUsername();
    } catch (error: any) {
      console.warn('⚠️ Error obteniendo username:', error);
      return '';
    }
  }

  getEmail(): string {
    try {
      const user = this.keycloakService.getKeycloakInstance().idTokenParsed;
      return user?.['email'] || '';
    } catch (error: any) {
      console.warn('⚠️ Error obteniendo email:', error);
      return '';
    }
  }
}
*/

// src/app/services/auth.ts
import { Injectable, inject } from '@angular/core';
import { ManualAuthService } from './manual-auth.service';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private manualAuthService = inject(ManualAuthService);

  async isLoggedIn(): Promise<boolean> {
    return this.manualAuthService.isLoggedIn();
  }

  login(): void {
    console.log('🔑 Redirigiendo a Keycloak...');
    
    const redirectUri = encodeURIComponent('http://localhost:4200/keycloak-callback');
    const loginUrl = `http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/auth?client_id=grupo-10&redirect_uri=${redirectUri}&response_type=code&scope=openid`;
    
    window.location.href = loginUrl;
  }

  async logout(): Promise<void> {
    this.manualAuthService.logout();
  }

  async getToken(): Promise<string> {
    return this.manualAuthService.getToken();
  }

  getUserName(): string {
    // Por ahora devolver un valor fijo, luego podemos extraer del token
    return 'Usuario';
  }

  getEmail(): string {
    // Por ahora vacío, luego podemos extraer del token
    return '';
  }

  async debugAuthStatus(): Promise<string> {
    const isLoggedIn = this.manualAuthService.isLoggedIn();
    const token = this.manualAuthService.getToken();
    return `LoggedIn: ${isLoggedIn} | Token: ${token ? '✅' : '❌'}`;
  }
}
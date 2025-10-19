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
<<<<<<< Updated upstream
}
*/

// src/app/services/auth.ts
import { Injectable, inject } from '@angular/core';
import { ManualAuthService } from './manual-auth.service';
=======
  
}

*/

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
>>>>>>> Stashed changes

@Injectable({
  providedIn: 'root'
})
export class AuthService {
<<<<<<< Updated upstream
  private manualAuthService = inject(ManualAuthService);

  async isLoggedIn(): Promise<boolean> {
    return this.manualAuthService.isLoggedIn();
  }

  login(): void {
    console.log('🔑 Redirigiendo a Keycloak...');
=======
  private http = inject(HttpClient);

  // ===== MÉTODOS DE AUTENTICACIÓN MANUAL =====
  
  async exchangeCodeForToken(code: string): Promise<string> {
    console.log('🔄 AuthService - Intercambiando código por token...');
    
    const tokenUrl = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/token';
    const redirectUri = 'http://localhost:4200/keycloak-callback';
    
    const body = new URLSearchParams();
    body.set('grant_type', 'authorization_code');
    body.set('client_id', 'grupo-10');
    body.set('code', code);
    body.set('redirect_uri', redirectUri);
    body.set('client_secret', '66ff9787-4fa5-46b3-b546-4ccbe604d233'); // Si necesitas

    try {
      console.log('📤 Enviando request a Keycloak para obtener token...');
      
      const response: any = await this.http.post(tokenUrl, body.toString(), {
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded'
        }
      }).toPromise();

      console.log('✅ Token obtenido correctamente de Keycloak');
      console.log('🔑 Token:', response.access_token ? `✅ (${response.access_token.length} chars)` : '❌ No token');
      
      return response.access_token;
      
    } catch (error) {
      console.error('❌ Error intercambiando código por token:', error);
      throw error;
    }
  }

  // ===== MÉTODOS DE ESTADO =====

  isLoggedIn(): boolean {
    return this.checkManualAuth();
  }

  private checkManualAuth(): boolean {
    const token = localStorage.getItem('auth_token');
    if (!token) {
      console.log('🔐 AuthService - No hay token en localStorage');
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const isExpired = Date.now() >= payload.exp * 1000;
      
      console.log('🔐 AuthService - Verificación token:', {
        hasToken: !!token,
        expired: isExpired,
        expires: new Date(payload.exp * 1000),
        now: new Date()
      });

      if (isExpired) {
        console.log('⚠️ Token expirado, limpiando...');
        localStorage.removeItem('auth_token');
      }

      return !isExpired;
    } catch (error) {
      console.error('❌ Error verificando token:', error);
      return false;
    }
  }

  // ===== MÉTODOS DE LOGIN/LOGOUT =====

  login(): void {
    console.log('🔑 AuthService - Redirigiendo a Keycloak...');
>>>>>>> Stashed changes
    
    const redirectUri = encodeURIComponent('http://localhost:4200/keycloak-callback');
    const loginUrl = `http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/auth?client_id=grupo-10&redirect_uri=${redirectUri}&response_type=code&scope=openid`;
    
<<<<<<< Updated upstream
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
=======
    console.log('📍 URL de login:', loginUrl);
    window.location.href = loginUrl;
  }

  logout(): void {
    console.log('🚪 AuthService - Cerrando sesión...');
    
    // Limpiar token local
    localStorage.removeItem('auth_token');
    console.log('✅ Token removido de localStorage');
    
    // Redirigir a logout de Keycloak
    const logoutUrl = `http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/logout`;
    window.location.href = logoutUrl;
  }

  // ===== MÉTODOS DE INFORMACIÓN DE USUARIO =====

  getToken(): string {
    const token = localStorage.getItem('auth_token');
    console.log('🗝️ AuthService - getToken:', token ? `✅ (${token.length} chars)` : '❌ No token');
    return token || '';
  }

  getUserName(): string {
    const token = this.getToken();
    if (!token) {
      console.log('👤 AuthService - No hay token para obtener nombre');
      return 'Usuario';
    }
  

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      console.log('📋 Payload del token:', payload);
      
      const userName = payload.preferred_username || 
                      payload.name || 
                      payload.given_name || 
                      payload.client_id || 
                      'Usuario';
      
      console.log('👤 AuthService - getUserName:', userName);
      return userName;
    } catch (error) {
      console.error('❌ Error obteniendo nombre del token:', error);
      return 'Usuario';
    }
  }

  saveManualToken(token: string): void {
    console.log('💾 AuthService - Guardando token en localStorage...');
    localStorage.setItem('auth_token', token);
    
    // Verificar que se guardó correctamente
    const saved = localStorage.getItem('auth_token');
    console.log('✅ Token guardado:', saved ? `✅ (${saved.length} chars)` : '❌ No se guardó');
    
    // Debug: mostrar todo el localStorage
    console.log('📦 Estado completo del localStorage:');
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key) {
        const value = localStorage.getItem(key);
        console.log(`   ${key}: ${value ? '✅' : '❌'}`);
      }
    }
  }

  // ===== MÉTODO DE DEBUG =====

  debugAuth(): void {
    console.log('🐛 DEBUG Auth State:');
    console.log('🔐 Token en localStorage:', localStorage.getItem('auth_token') ? '✅ EXISTE' : '❌ NO EXISTE');
    console.log('🔐 isLoggedIn():', this.isLoggedIn());
    console.log('👤 getUserName():', this.getUserName());
    
    const token = this.getToken();
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log('📋 Payload:', payload);
      } catch (e) {
        console.error('❌ Error decodificando token:', e);
      }
    }
>>>>>>> Stashed changes
  }
}
/*
// manual-auth.service.ts
// src/app/services/manual-auth.service.ts
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ManualAuthService {
  private http = inject(HttpClient);
  private token: string = '';

  constructor() {
    console.log('🔧 ManualAuthService inicializado');
    // Cargar token existente al iniciar
    this.token = localStorage.getItem('keycloak_token') || '';
  }

  async exchangeCodeForToken(code: string): Promise<string> {
    try {
      console.log('🔄 STEP 3: Intercambiando código por token...');
      
      const tokenUrl = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/token';
      
      const body = new URLSearchParams();
      body.set('grant_type', 'authorization_code');
      body.set('client_id', 'grupo-10');
      body.set('code', code);
      body.set('redirect_uri', 'http://localhost:4200/keycloak-callback');

      console.log('📤 Enviando request a Keycloak...');

      const response: any = await firstValueFrom(
        this.http.post(tokenUrl, body.toString(), {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        })
      );

      console.log('✅ Token recibido de Keycloak');
      this.token = response.access_token;
      
      // Guardar en localStorage
      localStorage.setItem('keycloak_token', this.token);
      
      return this.token;

    } catch (error: any) {
      console.error('❌ Error obteniendo token:', error);
      if (error.error) {
        console.error('❌ Detalles:', error.error);
      }
      throw error;
    }
  }

  getToken(): string {
    return this.token;
  }

  isLoggedIn(): boolean {
    return !!this.token;
  }

  logout(): void {
    this.token = '';
    localStorage.removeItem('keycloak_token');
    window.location.href = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/logout?redirect_uri=' + encodeURIComponent('http://localhost:4200');
  }
}
*/

import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ManualAuthService {
  private http = inject(HttpClient);
  private token: string = '';
  
  // 🔑 REEMPLAZA ESTO CON TU CLIENT SECRET REAL
  private clientSecret = '66ff9787-4fa5-46b3-b546-4ccbe604d233';

  constructor() {
    console.log('🔧 ManualAuthService inicializado');
    this.token = localStorage.getItem('keycloak_token') || '';
  }

  async exchangeCodeForToken(code: string): Promise<string> {
    try {
      console.log('🔄 STEP 3: Intercambiando código por token...');
      
      const tokenUrl = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/token';
      
      const body = new URLSearchParams();
      body.set('grant_type', 'authorization_code');
      body.set('client_id', 'grupo-10');
      body.set('client_secret', this.clientSecret); // ← AGREGAR EL SECRET
      body.set('code', code);
      body.set('redirect_uri', 'http://localhost:4200/keycloak-callback');

      console.log('📤 Enviando request a Keycloak con client secret...');

      const response: any = await firstValueFrom(
        this.http.post(tokenUrl, body.toString(), {
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded'
          }
        })
      );

      console.log('✅ Token recibido correctamente de Keycloak');
      console.log('🔑 Token:', response.access_token ? `✅ (${response.access_token.length} caracteres)` : '❌');
      
      this.token = response.access_token;
      localStorage.setItem('keycloak_token', this.token);
      
      return this.token;

    } catch (error: any) {
      console.error('❌ Error obteniendo token:', error);
      if (error.error) {
        console.error('❌ Error details:', error.error);
      }
      if (error.status) {
        console.error('❌ HTTP Status:', error.status);
      }
      throw error;
    }
  }

  getToken(): string {
    return this.token;
  }

  isLoggedIn(): boolean {
    return !!this.token;
  }

  logout(): void {
    this.token = '';
    localStorage.removeItem('keycloak_token');
    window.location.href = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/logout?redirect_uri=' + encodeURIComponent('http://localhost:4200');
  }
}
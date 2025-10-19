import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ManualAuthService {

  async exchangeCodeForToken(code: string): Promise<string> {
    console.log('🔄 ManualAuthService - Intercambiando código por token...');
    
    const tokenUrl = 'http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/token';
    const redirectUri = 'http://localhost:4200/keycloak-callback';
    
    const body = new URLSearchParams();
    body.set('grant_type', 'authorization_code');
    body.set('client_id', 'grupo-10');
    body.set('code', code);
    body.set('redirect_uri', redirectUri);
    // Si necesitas client_secret, agrégalo aquí:
    // body.set('client_secret', 'tu-client-secret');

    try {
      console.log('📤 Enviando request a Keycloak...');
      const response = await fetch(tokenUrl, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/x-www-form-urlencoded'
        },
        body: body.toString()
      });

      console.log('📥 Response status:', response.status);
      
      if (!response.ok) {
        const errorText = await response.text();
        console.error('❌ Error response:', errorText);
        throw new Error(`Error ${response.status}: ${errorText}`);
      }

      const data = await response.json();
      console.log('✅ Token obtenido correctamente');
      console.log('🔑 Token recibido:', data.access_token ? `✅ (${data.access_token.length} chars)` : '❌ No token');
      
      return data.access_token;
      
    } catch (error) {
      console.error('❌ Error intercambiando código por token:', error);
      throw error;
    }
  }
}
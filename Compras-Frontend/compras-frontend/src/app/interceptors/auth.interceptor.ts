/*
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { KeycloakService } from 'keycloak-angular';
import { from, of, switchMap } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const keycloakService = inject(KeycloakService);

  // Excluir URLs que no necesitan autenticación
  if (req.url.includes('/assets/') || req.url.includes('keycloak')) {
    return next(req);
  }

  // Verificar si Keycloak está disponible
  const keycloakInstance = keycloakService.getKeycloakInstance();
  if (!keycloakInstance || !keycloakInstance.authenticated) {
    console.log('⚠️ Keycloak no disponible, enviando request sin token');
    return next(req);
  }

  return from(keycloakService.getToken()).pipe(
    switchMap(token => {
      if (token) {
        const clonedReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        return next(clonedReq);
      }
      return next(req);
    })
  );
};
*/
// auth.interceptor.ts
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { from, switchMap } from 'rxjs';
import { AuthService } from '../services/auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  // Solo agregar token a requests a tu API
  if (req.url.includes('localhost:7248')) {
    // Convertir la Promise a Observable usando from() y switchMap
    return from(authService.getToken()).pipe(
      switchMap(token => {
        if (token) {
          console.log('✅ Añadiendo token a la request');
          const authReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${token}`
            }
          });
          return next(authReq);
        } else {
          console.warn('⚠️ No hay token disponible para la request');
          return next(req);
        }
      })
    );
  }

  return next(req);
};
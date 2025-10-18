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
import { Routes } from '@angular/router';
import { ProductsComponent } from './components/products/products';
import { KeycloakCallbackComponent } from './components/keycloak-callback/keycloak-callback';
import { LogoutComponent } from './components/logout/logout';
import { LoginComponent } from './components/login/login';

export const routes: Routes = [
  { path: '', component: LoginComponent }, // ← Página de login por defecto
  { path: 'login', component: LoginComponent },
  { path: 'compras', component: ProductsComponent },
  { path: 'keycloak-callback', component: KeycloakCallbackComponent },
  { path: 'logout', component: LogoutComponent },
  { path: '**', redirectTo: '/compras' }
];
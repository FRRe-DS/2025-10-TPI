
import { Routes } from '@angular/router';
import { ProductsComponent } from './components/products/products';
import { KeycloakCallbackComponent } from './components/keycloak-callback/keycloak-callback';
import { LogoutComponent } from './components/logout/logout';
import { RegisterComponent } from './components/auth/register/register';
import { LoginComponent } from './components/login/login';

export const routes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' }, // 👈 redirige al login
  { path: 'login', component: LoginComponent },
  { path: 'compras', component: ProductsComponent },
  { path: 'keycloak-callback', component: KeycloakCallbackComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'register', component: RegisterComponent },
  { path: '**', redirectTo: 'login' } // 👈 si no existe la ruta, redirige al login
];

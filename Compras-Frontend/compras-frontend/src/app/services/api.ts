import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth';
import { Observable, from, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  
  private apiUrl = 'https://localhost:7248/api'; // Tu backend .NET

  // Método para hacer requests autenticados
  private authenticatedRequest<T>(url: string, method: string = 'GET', data?: any): Observable<T> {
    return from(this.authService.getToken()).pipe(
      switchMap(token => {
        const options = {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        };

        const fullUrl = `${this.apiUrl}${url}`;

        switch (method.toUpperCase()) {
          case 'GET':
            return this.http.get<T>(fullUrl, options);
          case 'POST':
            return this.http.post<T>(fullUrl, data, options);
          case 'PUT':
            return this.http.put<T>(fullUrl, data, options);
          case 'DELETE':
            return this.http.delete<T>(fullUrl, options);
          default:
            return this.http.get<T>(fullUrl, options);
        }
      })
    );
  }

  // ========== PRODUCTOS ==========
  getProducts(): Observable<any[]> {
    return this.authenticatedRequest('/product');
  }

  getProduct(id: number): Observable<any> {
    return this.authenticatedRequest(`/product/${id}`);
  }

  // ========== CARRITO ==========
  getCart(): Observable<any> {
    return this.authenticatedRequest('/shopcart');
  }

  addToCart(productId: number, quantity: number): Observable<any> {
    return this.authenticatedRequest('/shopcart', 'POST', {
      productId,
      quantity
    });
  }

  updateCartItem(productId: number, quantity: number): Observable<any> {
    return this.authenticatedRequest('/shopcart', 'PUT', {
      productId,
      quantity
    });
  }

  removeFromCart(productId: number): Observable<any> {
    return this.authenticatedRequest(`/shopcart/${productId}`, 'DELETE');
  }

  clearCart(): Observable<any> {
    return this.authenticatedRequest('/shopcart', 'DELETE');
  }

  // ========== PEDIDOS ==========
  createOrder(orderData: any): Observable<any> {
    return this.authenticatedRequest('/shopcart/checkout', 'POST', orderData);
  }

  getOrderHistory(): Observable<any[]> {
    return this.authenticatedRequest('/shopcart/history');
  }

  getOrder(id: number): Observable<any> {
    return this.authenticatedRequest(`/shopcart/history/${id}`);
  }

  cancelOrder(id: number): Observable<any> {
    return this.authenticatedRequest(`/shopcart/history/${id}`, 'DELETE');
  }

  // ========== USUARIO ==========
  getUserProfile(): Observable<any> {
    return this.authenticatedRequest('/user/profile');
  }

  createUserProfile(profileData: any): Observable<any> {
    return this.authenticatedRequest('/user/profile', 'POST', profileData);
  }

  updateUserProfile(profileData: any): Observable<any> {
    return this.authenticatedRequest('/user/profile', 'PUT', profileData);
  }

  // ========== SINCronización con Keycloak ==========
  syncUser(userData: any): Observable<any> {
    // Este endpoint lo debes crear en tu backend para sincronizar usuarios de Keycloak
    return this.authenticatedRequest('/user/sync', 'POST', userData);
  }
}
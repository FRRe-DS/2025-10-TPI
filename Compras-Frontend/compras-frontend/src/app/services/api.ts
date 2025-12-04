// api.service.ts - VERSIÓN ACTUALIZADA CON CÁLCULO DE ENVÍO
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth';
import { Observable, from, throwError } from 'rxjs';
import { take, tap, catchError, finalize, switchMap } from 'rxjs/operators';

export interface ShippingCalculationRequest {
  deliveryAddress: {
    street: string;
    city: string;
    state: string;
    postalCode: string;
    country?: string;
  };
  transportType: 'truck' | 'boat' | 'plane' | string;
}

export interface ShippingCalculationResponse {
  shippingCost: number;
  currency: string;
  transportType: string;
  estimatedDeliveryDays: {
    minDays: number;
    maxDays: number;
    display: string;
  };
  estimatedDeliveryDate: string;
  productsTotal: number;
  grandTotal: number;
  itemsCount: number;
  postalCode: string;
  calculationDate: string;
}

export interface TransportMethod {
  type: string;
  name: string;
  estimatedDays: string;
  description?: string;
}

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
      take(1),
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

  // ========== CÁLCULO DE ENVÍO ==========

  /**
   * Calcula el costo de envío antes del checkout
   */
  calculateShipping(shippingData: ShippingCalculationRequest): Observable<ShippingCalculationResponse> {
    console.log('🚚 API SERVICE - Calculando envío:', shippingData);
    
    return from(this.authService.getToken()).pipe(
      take(1),
      switchMap(token => {
        console.log('🔑 API SERVICE - Token obtenido para cálculo de envío');
        
        const options = {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        };

        const fullUrl = `${this.apiUrl}/shopcart/calculate-shipping`;
        console.log('📡 API SERVICE - URL para cálculo:', fullUrl);

        return this.http.post<ShippingCalculationResponse>(fullUrl, shippingData, options).pipe(
          tap(response => console.log('✅ API SERVICE - Envío calculado:', response)),
          catchError(error => {
            console.error('❌ API SERVICE - Error calculando envío:', error);
            
            // Convertir error a formato manejable
            const errorMsg = error.error?.message || error.message || 'Error desconocido';
            const status = error.status || 0;
            
            return throwError(() => ({
              message: errorMsg,
              status: status,
              error: error.error
            }));
          })
        );
      }),
      finalize(() => console.log('🏁 API SERVICE - calculateShipping finalizado'))
    );
  }

  /**
   * Obtiene los métodos de transporte disponibles
   */
  getTransportMethods(): Observable<TransportMethod[]> {
    console.log('🚛 API SERVICE - Obteniendo métodos de transporte...');
    
    return from(this.authService.getToken()).pipe(
      take(1),
      switchMap(token => {
        const options = {
          headers: {
            'Authorization': `Bearer ${token}`
          }
        };

        return this.http.get<TransportMethod[]>(`${this.apiUrl}/shopcart/transport-methods`, options).pipe(
          tap(methods => console.log(`✅ API SERVICE - ${methods?.length || 0} métodos obtenidos`)),
          catchError(error => {
            console.warn('⚠️ API SERVICE - Error obteniendo métodos, usando valores por defecto:', error);
            
            // Retornar valores por defecto si falla
            const defaultMethods: TransportMethod[] = [
              { type: 'truck', name: '🚚 Camión', estimatedDays: '3-5', description: 'Económico' },
              { type: 'plane', name: '✈️ Avión', estimatedDays: '1-2', description: 'Express' },
              { type: 'boat', name: '🚢 Barco', estimatedDays: '7-10', description: 'Internacional' }
            ];
            
            return [defaultMethods];
          })
        );
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
    console.log('🚀 API SERVICE - Iniciando addToCart');
    
    return from(this.authService.getToken()).pipe(
      take(1),
      switchMap(token => {
        console.log('🔑 API SERVICE - Token obtenido, haciendo request');
        
        const options = {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        };

        const fullUrl = `${this.apiUrl}/shopcart`;
        console.log('📡 API SERVICE - URL:', fullUrl);

        return this.http.post(fullUrl, { productId, quantity }, options).pipe(
          tap(response => console.log('✅ API SERVICE - Response recibida')),
          catchError(error => {
            console.error('❌ API SERVICE - Error:', error);
            return throwError(() => error);
          })
        );
      }),
      finalize(() => console.log('🏁 API SERVICE - addToCart finalizado'))
    );
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
    console.log('🚀 API SERVICE - Creando orden:', orderData);
    
    return from(this.authService.getToken()).pipe(
      take(1),
      switchMap(token => {
        console.log('🔑 API SERVICE - Token obtenido, creando orden');
        
        const options = {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        };

        const fullUrl = `${this.apiUrl}/shopcart/checkout`;
        console.log('📡 API SERVICE - URL:', fullUrl);

        return this.http.post(fullUrl, orderData, options).pipe(
          tap(response => console.log('✅ API SERVICE - Orden creada:', response)),
          catchError(error => {
            console.error('❌ API SERVICE - Error creando orden:', error);
            return throwError(() => error);
          })
        );
      }),
      finalize(() => console.log('🏁 API SERVICE - createOrder finalizado'))
    );
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
    return this.authenticatedRequest('/user/sync', 'POST', userData);
  }

  // ========== MÉTODOS AUXILIARES ==========
  
  /**
   * Método simplificado para cálculo de envío (alternativo)
   */
  calculateShippingSimple(addressData: any): Observable<any> {
    return this.authenticatedRequest('/shopcart/calculate-shipping', 'POST', addressData);
  }

  /**
   * Verifica si el carrito tiene items antes de calcular envío
   */
  validateCartBeforeShipping(): Observable<boolean> {
    return new Observable(observer => {
      this.getCart().subscribe({
        next: (cart) => {
          const hasItems = cart?.items?.length > 0 || cart?.Items?.length > 0;
          observer.next(hasItems);
          observer.complete();
        },
        error: (error) => {
          console.error('Error validando carrito:', error);
          observer.next(false);
          observer.complete();
        }
      });
    });
  }
}
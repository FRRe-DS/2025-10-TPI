import { Injectable, inject } from '@angular/core';
import { ApiService } from './api';

@Injectable({
  providedIn: 'root'
})
export class CartService {
  private apiService = inject(ApiService);
  private cart: any[] = [];
  
  // 🔒 PROTECCIÓN CONTRA DUPLICADOS
  private isAddingToBackend = false;
  private pendingRequests = new Map<number, boolean>(); // productId -> en progreso

  getItems() {
    return this.cart;
  }

  addToCart(product: any) {
    console.log('🛒 addToCart ejecutado - Producto:', product.nombre, 'ID:', product.id);
    
    if (product.stockDisponible <= 0) {
      alert('Producto sin stock disponible');
      return;
    }

    // 1. Agregar al carrito local
    const existingItem = this.cart.find(item => item.id === product.id);
    if (existingItem) {
      if (existingItem.quantity >= product.stockDisponible) {
        alert('No hay suficiente stock disponible');
        return;
      }
      existingItem.quantity++;
      console.log('📦 Producto existente, cantidad aumentada a:', existingItem.quantity);
    } else {
      this.cart.push({
        ...product,
        quantity: 1
      });
      console.log('📦 Nuevo producto agregado al carrito local');
    }

    // 2. Sincronizar con backend CON PROTECCIÓN
    this.addToBackendCart(product.id, 1);

    alert('✅ ' + product.nombre + ' agregado al carrito');
  }

  addToBackendCart(productId: number, quantity: number) {
    // 🔒 EVITAR MÚLTIPLES LLAMADAS PARA EL MISMO PRODUCTO
    if (this.pendingRequests.has(productId)) {
      console.log('⏳ Llamada para producto', productId, 'ya en progreso, ignorando...');
      return;
    }

    // 🔒 BLOQUEAR LLAMADAS SIMULTÁNEAS
    if (this.isAddingToBackend) {
      console.log('🚫 Ya hay una operación en progreso, ignorando...');
      return;
    }

    this.pendingRequests.set(productId, true);
    this.isAddingToBackend = true;

    console.log('📡 Haciendo POST a /api/shopcart - Producto:', productId, 'Cantidad:', quantity);
    
    this.apiService.addToCart(productId, quantity).subscribe({
      next: (response) => {
        console.log('✅ Producto agregado al carrito del backend:', response);
      },
      error: (error) => {
        console.warn('⚠️ Error al agregar al carrito del backend:', error.message);
        // No mostrar alerta para no molestar al usuario
      },
      complete: () => {
        console.log('🎯 Llamada al backend completada para producto:', productId);
        // 🔓 LIBERAR BLOQUEOS
        this.pendingRequests.delete(productId);
        this.isAddingToBackend = false;
      }
    });
  }

  // ... los otros métodos permanecen igual
  validateQuantity(item: any) {
    if (item.quantity > item.stockDisponible) {
      alert('No puedes agregar más de ' + item.stockDisponible + ' unidades de este producto');
      item.quantity = item.stockDisponible;
    }
    if (item.quantity < 1) {
      item.quantity = 1;
    }
  }

  removeFromCart(index: number) {
    this.cart.splice(index, 1);
  }

  getCartTotal(): number {
    return this.cart.reduce(
      (total, item) => total + (item.precio * item.quantity),
      0
    );
  }

  clearCart() {
    this.cart = [];
  }
}
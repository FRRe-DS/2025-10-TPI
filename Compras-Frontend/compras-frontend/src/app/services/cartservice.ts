import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'   // servicio global en toda la app
})
export class CartService {
  private cart: any[] = [];

  getItems() {
    return this.cart;
  }

  addToCart(product: any) {
    if (product.stockDisponible <= 0) {
      alert('Producto sin stock disponible');
      return;
    }

    const existingItem = this.cart.find(item => item.id === product.id);
    if (existingItem) {
      if (existingItem.quantity >= product.stockDisponible) {
        alert('No hay suficiente stock disponible');
        return;
      }
      existingItem.quantity++;
    } else {
      this.cart.push({
        ...product,
        quantity: 1
      });
    }

    alert('✅ ' + product.nombre + ' agregado al carrito');
  }

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

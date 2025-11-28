import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CartService } from '../../services/cartservice';

@Component({
  selector: 'app-carrito',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './carrito.html',
  styleUrl: './carrito.css'
})
export class CarritoComponent {

  constructor(private cartService: CartService) {}

  get cart() {
    return this.cartService.getItems();
  }

  validateQuantity(item: any) {
    this.cartService.validateQuantity(item);
  }

  removeFromCart(index: number) {
    this.cartService.removeFromCart(index);
  }

  getCartTotal(): number {
    return this.cartService.getCartTotal();
  }
}

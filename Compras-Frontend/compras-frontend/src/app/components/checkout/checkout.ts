// src/app/components/checkout/checkout.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CartServiceFixed } from '../../services/cartservice-fixed';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class CheckoutComponent implements OnInit {
  cartTotal: number = 0;
  cartItemsCount: number = 0;
  loading: boolean = false;
  submitting: boolean = false;
  errorMessage: string = '';

  // Datos del formulario
  deliveryAddress = {
    street: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'AR' // Siempre Argentina por defecto
  };

  transportType: string = 'truck'; // Valor por defecto

  // Opciones para el select de transporte
  transportOptions = [
    { value: 'truck', label: '🚚 Camión' },
    { value: 'boat', label: '🚢 Barco' },
    { value: 'plane', label: '✈️ Avión' }
  ];

  constructor(
    private cartService: CartServiceFixed,
    private apiService: ApiService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadCartData();
  }

  async loadCartData() {
    try {
      this.loading = true;
      const cart = this.cartService.getItems();
      this.cartTotal = this.cartService.getCartTotal();
      this.cartItemsCount = cart.length;
      
      if (cart.length === 0) {
        this.errorMessage = 'Tu carrito está vacío. Agrega productos antes de continuar.';
      }
    } catch (error) {
      this.errorMessage = 'Error al cargar los datos del carrito';
      console.error('Error:', error);
    } finally {
      this.loading = false;
    }
  }

  validateForm(): boolean {
    if (this.cartItemsCount === 0) {
      this.errorMessage = 'No puedes realizar un pedido con el carrito vacío';
      return false;
    }

    if (!this.deliveryAddress.street.trim()) {
      this.errorMessage = 'La calle es requerida';
      return false;
    }

    if (!this.deliveryAddress.city.trim()) {
      this.errorMessage = 'La ciudad es requerida';
      return false;
    }

    if (!this.deliveryAddress.state.trim()) {
      this.errorMessage = 'La provincia es requerida';
      return false;
    }

    if (!this.deliveryAddress.postalCode.trim()) {
      this.errorMessage = 'El código postal es requerido';
      return false;
    }

    if (!this.transportType) {
      this.errorMessage = 'Selecciona un método de envío';
      return false;
    }

    this.errorMessage = '';
    return true;
  }

  async submitOrder() {
    if (!this.validateForm()) {
      return;
    }

    this.submitting = true;
    
    try {
      const orderData = {
        deliveryAddress: this.deliveryAddress,
        transportType: this.transportType
      };

      console.log('📦 Enviando orden:', orderData);
      
      const response = await this.apiService.createOrder(orderData).toPromise();
      
      console.log('✅ Orden creada exitosamente:', response);
      
      // Mostrar mensaje de éxito
      alert('🎉 ¡Pedido realizado con éxito!\nTu número de orden es: ' + 
            (response.orderId || response.id || 'N/A'));
      
      // Limpiar carrito después de éxito
      this.cartService.clearCart();
      
      // Redirigir a la página de confirmación o historial
      this.router.navigate(['/carrito']);
      
    } catch (error: any) {
      console.error('❌ Error creando orden:', error);
      this.errorMessage = 'Error al procesar el pedido: ' + 
                         (error.error?.message || error.message || 'Error desconocido');
    } finally {
      this.submitting = false;
    }
  }

  goBack() {
    this.router.navigate(['/carrito']);
  }
}
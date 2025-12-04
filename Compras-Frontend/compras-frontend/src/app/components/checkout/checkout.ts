// src/app/components/checkout/checkout.ts
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { CartServiceFixed } from '../../services/cartservice-fixed';
import { ApiService, ShippingCalculationRequest, ShippingCalculationResponse, TransportMethod } from '../../services/api';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './checkout.html',
  styleUrl: './checkout.css'
})
export class CheckoutComponent implements OnInit {
  // Estados
  loading: boolean = false;
  calculatingShipping: boolean = false;
  submitting: boolean = false;
  shippingCalculated: boolean = false;
  
  // Mensajes
  errorMessage: string = '';
  successMessage: string = '';
  
  // Datos del carrito
  cartTotal: number = 0;
  cartItemsCount: number = 0;
  
  // Datos del envío
  shippingData: ShippingCalculationResponse | null = null;
  transportMethods: TransportMethod[] = [];
  
  // Datos del formulario
  deliveryAddress = {
    street: '',
    city: '',
    state: '',
    postalCode: '',
    country: 'AR'
  };

  transportType: string = 'truck';

  // Opciones de transporte por defecto (backup)
  defaultTransportOptions = [
  { 
    type: 'truck', 
    name: '🚚 Camión', 
    estimatedDays: '3-5',
    description: 'Opción económica para envíos terrestres'
  },
  { 
    type: 'plane', 
    name: '✈️ Avión', 
    estimatedDays: '1-2',
    description: 'Envía express para entregas urgentes'
  },
  { 
    type: 'boat', 
    name: '🚢 Barco', 
    estimatedDays: '7-10',
    description: 'Para productos voluminosos o internacionales'
  }
];

  constructor(
    private cartService: CartServiceFixed,
    private apiService: ApiService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadCartData();
    this.loadTransportMethods();
  }

  async loadCartData() {
    try {
      this.loading = true;
      this.errorMessage = '';
      
      // Cargar datos del carrito
      const cart = this.cartService.getItems();
      this.cartTotal = this.cartService.getCartTotal();
      this.cartItemsCount = cart.length;
      
      if (cart.length === 0) {
        this.errorMessage = 'Tu carrito está vacío. Agrega productos antes de continuar.';
        this.shippingCalculated = false;
        this.shippingData = null;
      } else {
        // Si ya hay datos de envío pero el carrito cambió, resetear
        if (this.shippingCalculated) {
          this.shippingCalculated = false;
          this.shippingData = null;
        }
      }
    } catch (error) {
      this.errorMessage = 'Error al cargar los datos del carrito';
      console.error('Error:', error);
    } finally {
      this.loading = false;
    }
  }

  loadTransportMethods() {
    this.apiService.getTransportMethods().subscribe({
      next: (methods) => {
        if (methods && methods.length > 0) {
          this.transportMethods = methods;
          console.log('🚚 Métodos de transporte cargados:', methods);
        } else {
          this.transportMethods = this.defaultTransportOptions;
          console.log('⚠️ Usando métodos de transporte por defecto');
        }
        
        // Establecer el primer método como valor por defecto si no hay selección
        if (this.transportMethods.length > 0 && !this.transportType) {
          this.transportType = this.transportMethods[0].type;
        }
      },
      error: (error) => {
        console.warn('No se pudieron cargar métodos de transporte:', error);
        this.transportMethods = this.defaultTransportOptions;
        this.errorMessage = 'No se pudieron cargar las opciones de envío, usando valores por defecto.';
        
        // Limpiar mensaje después de 3 segundos
        setTimeout(() => {
          if (this.errorMessage.includes('opciones de envío')) {
            this.errorMessage = '';
          }
        }, 3000);
      }
    });
  }

  // 📦 MÉTODO PRINCIPAL: Calcular envío
  calculateShipping() {
    // Validar que el carrito tenga items
    if (this.cartItemsCount === 0) {
      this.errorMessage = 'Tu carrito está vacío. Agrega productos antes de calcular el envío.';
      return;
    }

    // Validar datos de dirección
    if (!this.validateAddress()) {
      return;
    }

    this.calculatingShipping = true;
    this.errorMessage = '';
    this.successMessage = '';

    const shippingRequest: ShippingCalculationRequest = {
      deliveryAddress: this.deliveryAddress,
      transportType: this.transportType
    };

    console.log('🚚 Calculando envío con datos:', shippingRequest);

    this.apiService.calculateShipping(shippingRequest).subscribe({
      next: (response) => {
        this.shippingData = response;
        this.shippingCalculated = true;
        this.successMessage = '¡Envío calculado exitosamente!';
        
        console.log('✅ Envío calculado:', response);
        
        // Actualizar el total del carrito con los productos
        this.cartTotal = response.productsTotal;
        
        // Limpiar mensaje de éxito después de 3 segundos
        setTimeout(() => {
          this.successMessage = '';
        }, 3000);
      },
      error: (error) => {
        console.error('❌ Error calculando envío:', error);
        this.shippingCalculated = false;
        this.shippingData = null;
        
        // Manejar diferentes tipos de errores
        this.handleShippingError(error);
      },
      complete: () => {
        this.calculatingShipping = false;
        console.log('🏁 Cálculo de envío completado');
      }
    });
  }

  // Validación de dirección (para cálculo de envío)
  validateAddress(): boolean {
    const errors: string[] = [];
    
    if (!this.deliveryAddress.street?.trim()) {
      errors.push('La calle es requerida');
    }
    
    if (!this.deliveryAddress.city?.trim()) {
      errors.push('La ciudad es requerida');
    }
    
    if (!this.deliveryAddress.state?.trim()) {
      errors.push('La provincia es requerida');
    }
    
    if (!this.deliveryAddress.postalCode?.trim()) {
    errors.push('El código postal es requerido');
    } else {
      // ✅VALIDACIÓN CORREGIDA: acepta letras y números
      const postalCode = this.deliveryAddress.postalCode.replace(/\s/g, '');
      if (!/^[A-Za-z0-9]{4,8}$/.test(postalCode)) {
        errors.push('El código postal debe tener entre 4 y 8 caracteres (letras o números)');
      }
    }
    
    if (!this.transportType) {
      errors.push('Selecciona un método de transporte');
    }
    
    if (errors.length > 0) {
      this.errorMessage = errors.join('. ') + '.';
      return false;
    }
    
    this.errorMessage = '';
    return true;
  }

  // Validación completa (para envío del pedido)
  validateForm(): boolean {
    // Validar carrito
    if (this.cartItemsCount === 0) {
      this.errorMessage = 'No puedes realizar un pedido con el carrito vacío';
      return false;
    }

    // Validar que se haya calculado el envío
    if (!this.shippingCalculated) {
      this.errorMessage = 'Debes calcular el envío antes de confirmar el pedido';
      return false;
    }

    // Validar dirección
    return this.validateAddress();
  }

  // Calcular total con envío
  getTotalWithShipping(): number {
    if (!this.shippingData) return this.cartTotal;
    return this.shippingData.grandTotal;
  }

  // Obtener fecha de entrega formateada
  getFormattedDeliveryDate(): string {
    if (!this.shippingData?.estimatedDeliveryDate) return 'No disponible';
    
    try {
      const date = new Date(this.shippingData.estimatedDeliveryDate);
      return date.toLocaleDateString('es-AR', {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      });
    } catch (error) {
      return this.shippingData.estimatedDeliveryDate;
    }
  }

  // Traducir tipo de transporte
  translateTransportType(type: string): string {
    const translations: { [key: string]: string } = {
      'truck': 'Camión',
      'plane': 'Avión',
      'boat': 'Barco',
      'air': 'Aéreo',
      'road': 'Terrestre',
      'sea': 'Marítimo'
    };
    return translations[type] || type;
  }

  // Enviar pedido (checkout final)
  submitOrder() {
    if (!this.validateForm()) {
      return;
    }

    this.submitting = true;
    this.errorMessage = '';
    this.successMessage = '';
    
    const orderData = {
      deliveryAddress: this.deliveryAddress,
      transportType: this.transportType
    };

    console.log('📦 Enviando orden de checkout:', orderData);
    
    this.apiService.createOrder(orderData).subscribe({
      next: (response) => {
        console.log('✅ Orden creada exitosamente:', response);
        
        // Preparar mensaje de éxito con detalles
        this.prepareSuccessMessage(response);
        
        // Limpiar carrito después de éxito
        this.cartService.clearCart();
        this.shippingCalculated = false;
        this.shippingData = null;
        
        // Redirigir después de mostrar el mensaje
        setTimeout(() => {
          this.router.navigate(['/carrito']);
        }, 3000);
        
        this.submitting = false;
      },
      error: (error) => {
        console.error('❌ Error creando orden:', error);
        this.handleOrderError(error);
        this.submitting = false;
      },
      complete: () => {
        console.log('🏁 Proceso de checkout completado');
      }
    });
  }

  // Preparar mensaje de éxito
  private prepareSuccessMessage(response: any) {
    const orderNumber = response.reservaId || response.orderId || 'N/A';
    const shippingId = response.shippingId || 'N/A';
    
    // Obtener costos de diferentes lugares posibles
    const shippingCost = response.costos?.envio || 
                        response.shippingCost || 
                        this.shippingData?.shippingCost || 
                        0;
    
    const totalPaid = response.costos?.total || 
                     this.getTotalWithShipping() || 
                     0;
    
    // Obtener fecha de entrega
    const estimatedDelivery = response.estimatedDelivery || 
                            this.getFormattedDeliveryDate() || 
                            'No disponible';
    
    // Crear mensaje
    this.successMessage = `
      🎉 ¡Pedido realizado con éxito!
      
      📦 Número de reserva: ${orderNumber}
      🚚 Número de envío: ${shippingId}
      💰 Costo de envío: $${shippingCost}
      💵 Total pagado: $${totalPaid}
      📅 Entrega estimada: ${estimatedDelivery}
      
      ${response.message || 'Tu pedido ha sido procesado exitosamente.'}
      
      Serás redirigido al carrito en unos segundos...
    `;
  }

  // Manejar errores de cálculo de envío
  private handleShippingError(error: any) {
    let errorMsg = 'Error al calcular el envío';
    
    if (error.status === 0) {
      errorMsg = 'No se pudo conectar con el servidor. Verifica tu conexión.';
    } else if (error.status === 400) {
      if (error.error?.code === 'EMPTY_CART') {
        errorMsg = 'Tu carrito está vacío. Agrega productos antes de calcular el envío.';
      } else {
        errorMsg = 'Datos inválidos: ' + (error.error?.message || 'Revisa la información de envío');
      }
    } else if (error.status === 401) {
      errorMsg = 'No estás autenticado. Por favor, inicia sesión nuevamente.';
    } else if (error.status === 500) {
      errorMsg = 'Error interno del servidor al calcular el envío. Intenta nuevamente.';
    } else if (error.message) {
      errorMsg = error.message;
    } else if (typeof error === 'string') {
      errorMsg = error;
    }
    
    this.errorMessage = errorMsg;
  }

  // Manejar errores de orden (checkout)
  private handleOrderError(error: any) {
    let errorMsg = 'Error al procesar el pedido';
    
    if (error.status === 0) {
      errorMsg = 'No se pudo conectar con el servidor. Verifica tu conexión.';
    } else if (error.status === 400) {
      errorMsg = 'Datos inválidos: ' + (error.error?.message || 'Revisa la información');
    } else if (error.status === 401) {
      errorMsg = 'No estás autenticado. Por favor, inicia sesión nuevamente.';
    } else if (error.status === 500) {
      errorMsg = 'Error interno del servidor. Intenta nuevamente más tarde.';
    } else if (error.error?.message) {
      errorMsg = error.error.message;
    } else if (error.message) {
      errorMsg = error.message;
    }
    
    this.errorMessage = errorMsg;
  }

  // Resetear cálculo de envío cuando cambian los datos
  onAddressChange() {
    if (this.shippingCalculated) {
      this.shippingCalculated = false;
      this.shippingData = null;
    }
  }

  onTransportChange() {
    if (this.shippingCalculated) {
      this.shippingCalculated = false;
      this.shippingData = null;
      this.successMessage = 'El método de envío ha cambiado. Por favor, recalcula el envío.';
      
      setTimeout(() => {
        this.successMessage = '';
      }, 3000);
    }
  }

  // Navegación
  goBack() {
    this.router.navigate(['/carrito']);
  }

  goToProducts() {
    this.router.navigate(['/paginaproductos']);
  }
}
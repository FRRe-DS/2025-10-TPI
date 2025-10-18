import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router'; // ← Agregar si falta
import { AuthService } from '../../services/auth';
import { ApiService } from '../../services/api';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="container mt-4">
      <!-- Header -->
      <div class="row mb-4">
        <div class="col">
          <h1>Portal de Compras</h1>
          <p class="lead">Bienvenido, {{userName}}!</p>
        </div>
      </div>

      <div class="text-center mt-4" *ngIf="userName">
        <div class="alert alert-info">
          <h5>✅ Sesión activa como: {{userName}}</h5>
          <button class="btn btn-warning btn-sm" (click)="forceLogout()">
            🔄 Forzar Logout de Prueba
          </button>
        </div>
      </div>

      <!-- Barra de búsqueda -->
      <div class="row mb-4">
        <div class="col">
          <div class="input-group">
            <input 
              type="text" 
              class="form-control" 
              placeholder="Buscar productos..." 
              [(ngModel)]="searchTerm"
              (keyup.enter)="searchProducts()">
            <button class="btn btn-primary" (click)="searchProducts()">
              Buscar
            </button>
          </div>
        </div>
      </div>

      <!-- Lista de productos -->
      <div class="row">
        <div class="col">
          <h3>Productos Disponibles</h3>
          <div class="row">
            <div class="col-md-4 mb-3" *ngFor="let product of products">
              <div class="card">
                <div class="card-body">
                  <h5 class="card-title">{{product.name}}</h5>
                  <p class="card-text">{{product.description}}</p>
                  <p class="card-text"><strong>&#36;{{product.price}}</strong></p>
                  <button class="btn btn-success btn-sm" (click)="addToCart(product)">
                    Agregar al Carrito
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Carrito de compras -->
      <div class="row mt-4" *ngIf="cart.length > 0">
        <div class="col">
          <h3>Mi Carrito</h3>
          <table class="table table-striped">
            <thead>
              <tr>
                <th>Producto</th>
                <th>Precio</th>
                <th>Cantidad</th>
                <th>Subtotal</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let item of cart; let i = index">
                <td>{{item.name}}</td>
                <td>{{item.price}}</td>
                <td>
                  <input type="number" 
                         class="form-control form-control-sm" 
                         [(ngModel)]="item.quantity" 
                         min="1"
                         style="width: 80px;">
                </td>
                <td>{{item.price * item.quantity}}</td>
                <td>
                  <button class="btn btn-danger btn-sm" (click)="removeFromCart(i)">
                    Eliminar
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
          <div class="text-end">
            <h4>Total: {{ getCartTotal()}}</h4>
            <button class="btn btn-success btn-lg" (click)="checkout()">
              Realizar Compra
            </button>
          </div>
        </div>
      </div>

      <!-- Mis compras -->
      <div class="row mt-5">
        <div class="col">
          <h3>Mis Compras Anteriores</h3>
          <button class="btn btn-outline-primary mb-3" (click)="loadCompras()">
            Actualizar Compras
          </button>
          <div *ngIf="compras.length > 0">
            <div class="card mb-3" *ngFor="let compra of compras">
              <div class="card-body">
                <h5 class="card-title">Compra #{{compra.id}}</h5>
                <p class="card-text">Fecha: {{compra.fecha | date}}</p>
                <p class="card-text">Estado: 
                  <span [class]="getStatusClass(compra.estado)">{{compra.estado}}</span>
                </p>
                <p class="card-text"><strong>Total: {{compra.total}}</strong></p>
              </div>
            </div>
          </div>
          <div *ngIf="compras.length === 0">
            <p class="text-muted">No tienes compras registradas.</p>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .status-pendiente { color: orange; font-weight: bold; }
    .status-enviado { color: blue; font-weight: bold; }
    .status-entregado { color: green; font-weight: bold; }
    .status-cancelado { color: red; font-weight: bold; }
  `]
})
export class ProductsComponent implements OnInit {
  private authService = inject(AuthService);
  private apiService = inject(ApiService);
  
  userName: string = '';
  searchTerm: string = '';
  products: any[] = [];
  cart: any[] = [];
  compras: any[] = [];

  forceLogout() {
  const logoutUrl = `http://localhost:8080/realms/ds-2025-realm/protocol/openid-connect/logout?redirect_uri=${encodeURIComponent(window.location.origin)}`;
  window.location.href = logoutUrl;
  }

  ngOnInit() {
    this.userName = this.authService.getUserName();
    this.loadProducts();
    this.loadCompras();
  }

  loadProducts() {
    this.apiService.getProducts().subscribe({
      next: (products: any) => {
        this.products = products;
      },
      error: (error: any) => {
        console.error('Error cargando productos:', error);
        // Productos de ejemplo como fallback
        this.products = [
          { id: 1, name: 'Laptop Gamer', description: 'Laptop para gaming', price: 1200, stock: 5, category: 'Tecnología' },
          { id: 2, name: 'Smartphone', description: 'Teléfono inteligente', price: 800, stock: 10, category: 'Tecnología' },
          { id: 3, name: 'Tablet', description: 'Tablet 10 pulgadas', price: 400, stock: 8, category: 'Tecnología' },
          { id: 4, name: 'Auriculares', description: 'Auriculares inalámbricos', price: 150, stock: 15, category: 'Audio' },
          { id: 5, name: 'Teclado Mecánico', description: 'Teclado para gaming', price: 120, stock: 12, category: 'Tecnología' },
          { id: 6, name: 'Monitor 24"', description: 'Monitor Full HD', price: 300, stock: 6, category: 'Tecnología' }
        ];
      }
    });
  }

  searchProducts() {
    if (this.searchTerm.trim()) {
      this.products = this.products.filter(product =>
        product.name.toLowerCase().includes(this.searchTerm.toLowerCase()) ||
        product.description.toLowerCase().includes(this.searchTerm.toLowerCase())
      );
    } else {
      this.loadProducts();
    }
  }

  addToCart(product: any) {
    if (product.stock <= 0) {
      alert('Producto sin stock disponible');
      return;
    }

    const existingItem = this.cart.find(item => item.id === product.id);
    if (existingItem) {
      if (existingItem.quantity >= product.stock) {
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
  }

  removeFromCart(index: number) {
    this.cart.splice(index, 1);
  }

  getCartTotal(): number {
    return this.cart.reduce((total, item) => total + (item.price * item.quantity), 0);
  }

  checkout() {
    if (this.cart.length === 0) {
      alert('El carrito está vacío');
      return;
    }

    const checkoutData = {
      deliveryAddress: 'Dirección de ejemplo',
      paymentMethod: 'tarjeta'
    };

    this.apiService.createOrder(checkoutData).subscribe({
      next: (order: any) => {
        alert('Compra realizada exitosamente!');
        this.cart = [];
        this.loadCompras();
      },
      error: (error: any) => {
        console.error('Error realizando compra:', error);
        alert('Error al realizar la compra: ' + (error.error?.message || 'Error desconocido'));
      }
    });
  }

  loadCompras() {
    this.apiService.getOrderHistory().subscribe({
      next: (compras: any) => {
        this.compras = compras;
      },
      error: (error: any) => {
        console.error('Error cargando compras:', error);
        // Datos de ejemplo como fallback
        this.compras = [
          { id: 1, fecha: new Date('2024-01-15'), estado: 'ENTREGADO', total: 950 },
          { id: 2, fecha: new Date('2024-01-20'), estado: 'ENVIADO', total: 1200 }
        ];
      }
    });
  }

  getStatusClass(estado: string): string {
    switch (estado) {
      case 'PENDIENTE': return 'status-pendiente';
      case 'ENVIADO': return 'status-enviado';
      case 'ENTREGADO': return 'status-entregado';
      case 'CANCELADO': return 'status-cancelado';
      default: return '';
    }
  }
}
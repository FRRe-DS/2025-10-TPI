import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PurchaseHistoryService, Purchase } from '../../services/purchase-history.service';

@Component({
  selector: 'app-purchase-history',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container mt-4">
      <h2 class="mb-4">
        <i class="bi bi-receipt me-2"></i>
        Mis Compras
      </h2>
      
      <!-- Loading -->
      <div *ngIf="loading" class="text-center my-5">
        <div class="spinner-border text-primary" role="status">
          <span class="visually-hidden">Cargando...</span>
        </div>
        <p class="text-muted mt-2">Buscando tus compras...</p>
      </div>

      <!-- Error -->
      <div *ngIf="error" class="alert alert-danger alert-dismissible fade show">
        <i class="bi bi-exclamation-triangle me-2"></i>
        <strong>Error:</strong> {{ error }}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        <button class="btn btn-sm btn-outline-danger ms-3" (click)="loadPurchases()">
          <i class="bi bi-arrow-clockwise"></i> Reintentar
        </button>
      </div>

      <!-- No hay compras -->
      <div *ngIf="!loading && !error && purchases.length === 0" class="alert alert-info">
        <i class="bi bi-info-circle me-2"></i>
        No tienes compras realizadas.
        <a routerLink="/paginaproductos" class="alert-link ms-2">Ir a comprar</a>
      </div>

      <!-- Tabla de compras -->
      <div *ngIf="!loading && !error && purchases.length > 0">
        <div class="card shadow-sm">
          <div class="card-header bg-white">
            <div class="d-flex justify-content-between align-items-center">
              <h5 class="mb-0">Historial de Compras</h5>
              <span class="badge bg-primary">{{ purchases.length }} compras</span>
            </div>
          </div>
          <div class="card-body p-0">
            <div class="table-responsive">
              <table class="table table-hover mb-0">
                <thead class="table-light">
                  <tr>
                    <th>ID Envío</th>
                    <th>Fecha</th>
                    <th>Estado</th>
                    <th class="text-end">Total</th>
                    <th>Seguimiento</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let purchase of purchases" class="align-middle">
                    <td>
                      <div class="fw-bold">#{{ purchase.shippingId }}</div>
                      <small class="text-muted">Orden: {{ purchase.orderId }}</small>
                    </td>
                    <td>
                      <div>{{ purchase.orderDate }}</div>
                      <small class="text-muted">
                        <i class="bi bi-truck me-1"></i>
                        Entrega: {{ purchase.estimatedDelivery }}
                      </small>
                    </td>
                    <td>
                      <span [class]="getStatusClass(purchase.status)" class="badge">
                        <i [class]="getStatusIcon(purchase.status)" class="me-1"></i>
                        {{ purchase.status }}
                      </span>
                    </td>
                    
                    <td class="text-end">
                      <div class="fw-bold text-success">
                        {{ purchase.currency }} {{ purchase.totalCost | number:'1.2-2' }}
                      </div>
                    </td>
                    <td>
                      <div *ngIf="purchase.trackingNumber">
                        <code class="bg-light p-2 rounded d-block text-center">
                          {{ purchase.trackingNumber }}
                        </code>
                        <button *ngIf="purchase.status === 'En camino'" 
                                class="btn btn-sm btn-outline-primary mt-1 w-100"
                                (click)="trackShipping(purchase.trackingNumber)">
                          <i class="bi bi-geo-alt me-1"></i> Rastrear
                        </button>
                      </div>
                      <span *ngIf="!purchase.trackingNumber" class="text-muted">
                        No disponible
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
          <div class="card-footer bg-white text-muted">
            <small>
              <i class="bi bi-clock-history me-1"></i>
              Última actualización: {{ currentDate | date:'dd/MM/yyyy HH:mm' }}
            </small>
          </div>
        </div>
      </div>
    </div>
  `
})
export class PurchaseHistoryComponent implements OnInit {
  purchases: Purchase[] = [];
  loading = true;
  error = '';
  currentDate = new Date();

  constructor(private purchaseService: PurchaseHistoryService) {}

  ngOnInit(): void {
    this.loadPurchases();
  }

  loadPurchases(): void {
    this.loading = true;
    this.error = '';
    this.currentDate = new Date();
    
    this.purchaseService.getPurchaseHistory().subscribe({
      next: (response) => {
        if (response.success) {
          this.purchases = response.data;
          console.log('Compras cargadas:', this.purchases);
        } else {
          this.error = 'No se pudieron cargar las compras';
        }
        this.loading = false;
      },
      error: (err) => {
        console.error('Error detallado:', err);
        this.error = err.error?.message || 'Error de conexión con el servidor';
        this.loading = false;
      }
    });
  }

  getStatusClass(status: string): string {
  const statusLower = status.toLowerCase();
  
  if (statusLower.includes('creado') || statusLower.includes('reservado')) 
    return 'bg-secondary text-white';
  
  if (statusLower.includes('tránsito') || statusLower.includes('distribución')) 
    return 'bg-info text-white';
  
  if (statusLower.includes('entregado') || statusLower.includes('llegó')) 
    return 'bg-success text-white';
  
  if (statusLower.includes('cancelado')) 
    return 'bg-danger text-white';
  
  return 'bg-warning text-dark';
}

  getStatusIcon(status: string): string {
    switch(status.toLowerCase()) {
      case 'procesando': return 'bi-hourglass-split';
      case 'en camino': return 'bi-truck';
      case 'entregado': return 'bi-check-circle';
      case 'cancelado': return 'bi-x-circle';
      default: return 'bi-question-circle';
    }
  }

  trackShipping(trackingNumber: string): void {
    // Abre en nueva pestaña para rastrear
    window.open(`https://www.google.com.ar`, '_blank');
  }
}
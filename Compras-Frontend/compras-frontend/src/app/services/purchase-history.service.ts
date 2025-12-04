import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface Purchase {
  shippingId: number;
  orderId: number;
  status: string;
  trackingNumber: string;
  totalCost: number;
  currency: string;
  estimatedDelivery: string;
  orderDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class PurchaseHistoryService {
  // CAMBIA ESTA URL por la de tu API .NET
  private apiUrl = 'https://localhost:7248/api';

  constructor(private http: HttpClient) {}

  getPurchaseHistory(): Observable<{ success: boolean; data: Purchase[] }> {
    return this.http.get<{ success: boolean; data: Purchase[] }>(
      `${this.apiUrl}/purchases/history`
    );
  }
}
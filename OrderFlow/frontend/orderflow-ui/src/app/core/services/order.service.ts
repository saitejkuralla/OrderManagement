import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateOrderRequest, Order, OrderSummary } from '../models/order.model';

@Injectable({ providedIn: 'root' })
export class OrderService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/orders`;

  getAll(): Observable<OrderSummary[]> {
    return this.http.get<OrderSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateOrderRequest): Observable<Order> {
    return this.http.post<Order>(this.baseUrl, request);
  }

  confirm(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/${id}/confirm`, null);
  }

  cancel(id: string): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/${id}/cancel`, null);
  }
}

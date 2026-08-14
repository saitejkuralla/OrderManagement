import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Product, CreateProductRequest } from '../models/product.model';

@Injectable({ providedIn: 'root' })
export class ProductService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/products`;

  getAll(): Observable<Product[]> {
    return this.http.get<Product[]>(this.baseUrl);
  }

  getById(id: string): Observable<Product> {
    return this.http.get<Product>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateProductRequest): Observable<Product> {
    return this.http.post<Product>(this.baseUrl, request);
  }
}

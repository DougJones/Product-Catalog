import {  Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ProductSummary, ProductDetail, CatalogMetrics } from '../entities/product';

@Injectable({
  providedIn: 'root'
})
export class ProductService {
  private apiUrl = 'http://localhost:5000/api/products'; 

  constructor(private http: HttpClient) { }

  getProducts(): Observable<ProductSummary[]> {
    return this.http.get<ProductSummary[]>(this.apiUrl);
  }

  getProductById(id: number): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${this.apiUrl}/${id}`);
  }

  getMetrics(): Observable<CatalogMetrics> {
    return this.http.get<CatalogMetrics>(`${this.apiUrl}/metrics`);
  }
}

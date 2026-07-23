import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ProductService } from '../services/product.service';
import { ProductSummary, CatalogMetrics } from '../entities/product';
import { Observable, BehaviorSubject, combineLatest } from 'rxjs';
import { map, startWith } from 'rxjs/operators';

@Component({
  selector: 'app-products-overview',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule], // CommonModule is required for async pipe
  templateUrl: 'products-overview.component.html',
  styleUrls: ['products-overview.component.css']
})
export class ProductsOverviewComponent implements OnInit {
  // 1. Core Data Stream Observables
  metrics$!: Observable<CatalogMetrics>;
  filteredProducts$!: Observable<ProductSummary[]>;

  // 2. Reactive Filter Inputs (Using BehaviorSubjects as reactive data valves)
  private searchQuery$ = new BehaviorSubject<string>('');
  private selectedPriceUnit$ = new BehaviorSubject<string>('');

  // Plain properties for ngModel template binding syntax
  searchQuery: string = '';
  selectedPriceUnit: string = '';

  constructor(private productService: ProductService) { }

  ngOnInit(): void {
    // A. Stream metrics directly from your port 5000 service
    this.metrics$ = this.productService.getMetrics();

    // B. Create a master stream that watches for raw products AND input changes simultaneously
    const masterProducts$ = this.productService.getProducts();

    this.filteredProducts$ = combineLatest([
      masterProducts$,
      this.searchQuery$.pipe(startWith('')),
      this.selectedPriceUnit$.pipe(startWith(''))
    ]).pipe(
      map(([products, search, unit]) => {
        const query = search.trim().toLowerCase();
        const filterUnit = unit.trim().toLowerCase();

        if (query === '' && filterUnit === '') {
          return products;
        }

        return products.filter(product => {
          const matchesTitle = query === '' || product.title.toLowerCase().includes(query);
          const matchesUnit = filterUnit === '' || product.unitOfMeasure.toLowerCase() === filterUnit;
          return matchesTitle && matchesUnit;
        });
      })
    );
  }

  // Task 2: Emits changes down the reactive stream pipes whenever a user interacts with the UI
  applyFilters(): void {
    this.searchQuery$.next(this.searchQuery);
    this.selectedPriceUnit$.next(this.selectedPriceUnit);
  }
}

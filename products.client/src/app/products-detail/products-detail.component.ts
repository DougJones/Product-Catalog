import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { ProductService } from '../services/product.service'
import { ProductDetail } from '../entities/product';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: 'products-detail.component.html',
  styleUrls: []
})
export class ProductDetailComponent implements OnInit {
  product$!: Observable<ProductDetail>;
  cityName: string = 'Local Location'; // Fallback default
  isLocating: boolean = false;
  shareableMessage: string = '';

  constructor(
    private route: ActivatedRoute,
    private productService: ProductService,
    private http: HttpClient
  ) { }

  ngOnInit(): void {
    // 1. Extract the product ID parameter from the active route URL
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      // Stream the full detail DTO directly from port 5000 using our service
      this.product$ = this.productService.getProductById(+idParam);
    }

    // 2. Proactively capture the user's location to determine the city name
    this.getUserCityLocation();
  }

  getUserCityLocation(): void {
    if (!navigator.geolocation) {
      return;
    }

    this.isLocating = true;

    navigator.geolocation.getCurrentPosition(
      (position) => {
        const lat = position.coords.latitude;
        const lon = position.coords.longitude;

        // Free reverse-geocoding endpoint requiring no custom API key registrations
        const geocodeUrl = `https://bigdatacloud.net{lat}&longitude=${lon}&localityLanguage=en`;

        this.http.get<any>(geocodeUrl).subscribe({
          next: (res) => {
            // Traverse potential locality attributes to isolate the city string cleanly
            this.cityName = res.city || res.locality || res.principalSubdivision || 'Your City';
            this.isLocating = false;
          },
          error: () => {
            this.isLocating = false; // Graceful fallback to default on network error
          }
        });
      },
      () => {
        this.isLocating = false; // Graceful fallback if user blocks location permissions
      }
    );
  }

  // Task 2 Action Item: "Add to List" Sharing Core Engine
  addToListAndShare(product: ProductDetail): void {
    // Compile share string exactly to the assessment's formatting sequence specifications
    this.shareableMessage = `${product.title} - ${product.price} from ${this.cityName} added to list`;

    // Code Challenge Best Practice: Use modern Web Share API, with clipboard backup
    if (navigator.share) {
      navigator.share({
        title: 'Product Selection List Interest',
        text: this.shareableMessage
      }).catch(err => console.log('Web share aborted', err));
    } else {
      // Fallback fallback: Copy directly to user clipboard
      navigator.clipboard.writeText(this.shareableMessage);
      alert(`Product logged! Copied share string to clipboard:\n\n"${this.shareableMessage}"`);
    }
  }
}

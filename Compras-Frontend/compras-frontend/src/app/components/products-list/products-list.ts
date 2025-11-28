import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api';
import { CartService } from '../../services/cartservice';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './products-list.html',
  styleUrl: './products-list.css'
})
export class ProductsListComponent implements OnInit {

  private apiService = inject(ApiService);
  private cartService = inject(CartService);

  searchTerm: string = '';
  products: any[] = [];
  filteredProducts: any[] = [];
  loading: boolean = false;

  ngOnInit() {
    this.loadProducts();
  }

  loadProducts() {
    this.loading = true;
    this.apiService.getProducts().subscribe({
      next: (products: any) => {
        this.products = products;
        this.filteredProducts = products;
        this.loading = false;
      },
      error: (error: any) => {
        console.error('❌ Error cargando productos:', error);
        this.products = [];
        this.filteredProducts = [];
        this.loading = false;
      }
    });
  }

  searchProducts() {
    if (this.searchTerm.trim()) {
      const term = this.searchTerm.toLowerCase();
      this.filteredProducts = this.products.filter(product =>
        product.nombre.toLowerCase().includes(term) ||
        product.descripcion.toLowerCase().includes(term) ||
        (product.categorias &&
          product.categorias.some((cat: any) =>
            cat.nombre.toLowerCase().includes(term)
          ))
      );
    } else {
      this.filteredProducts = this.products;
    }
  }

  getProductImage(product: any): string {
    if (product.imagenes && product.imagenes.length > 0) {
      return product.imagenes[0].url;
    }
    return 'https://via.placeholder.com/300x200?text=Sin+Imagen';
  }

  onImageError(event: any) {
    event.target.src = 'https://via.placeholder.com/300x200?text=Imagen+No+Disponible';
  }

  addToCart(product: any) {
    this.cartService.addToCart(product);
  }
}

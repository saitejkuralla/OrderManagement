import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { ProductService } from '../core/services/product.service';
import { Product } from '../core/models/product.model';
import { EmptyStateComponent } from '../shared/components/empty-state/empty-state.component';
import { ProductFormDialogComponent } from './product-form-dialog.component';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    EmptyStateComponent
  ],
  templateUrl: './products.component.html',
  styleUrl: './products.component.scss'
})
export class ProductsComponent implements OnInit {
  private readonly productService = inject(ProductService);
  private readonly dialog = inject(MatDialog);

  products: Product[] = [];
  searchTerm = '';
  loading = true;
  readonly displayedColumns = ['name', 'sku', 'price', 'isActive'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.productService.getAll().subscribe((products) => {
      this.products = products;
      this.loading = false;
    });
  }

  get filteredProducts(): Product[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.products;
    return this.products.filter(
      (p) => p.name.toLowerCase().includes(term) || p.sku.toLowerCase().includes(term)
    );
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(ProductFormDialogComponent, { width: '420px' });
    ref.afterClosed().subscribe((created) => {
      if (created) this.load();
    });
  }
}

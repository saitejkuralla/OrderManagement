import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormArray, FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { CustomerService } from '../../core/services/customer.service';
import { ProductService } from '../../core/services/product.service';
import { OrderService } from '../../core/services/order.service';
import { Customer } from '../../core/models/customer.model';
import { Product } from '../../core/models/product.model';
import { DiscountPreview, previewDiscount } from '../../core/services/discount-preview.util';

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCardModule
  ],
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.scss'
})
export class OrderCreateComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly productService = inject(ProductService);
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);

  customers: Customer[] = [];
  products: Product[] = [];
  submitting = false;
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    customerId: ['', Validators.required],
    items: this.fb.array<ReturnType<typeof this.createItemGroup>>([])
  });

  ngOnInit(): void {
    forkJoin({
      customers: this.customerService.getAll(),
      products: this.productService.getAll()
    }).subscribe(({ customers, products }) => {
      this.customers = customers;
      this.products = products.filter((p) => p.isActive);
      this.addItem();
    });
  }

  get items(): FormArray {
    return this.form.controls.items;
  }

  createItemGroup() {
    return this.fb.nonNullable.group({
      productId: ['', Validators.required],
      quantity: [1, [Validators.required, Validators.min(1)]]
    });
  }

  addItem(): void {
    this.items.push(this.createItemGroup());
  }

  removeItem(index: number): void {
    this.items.removeAt(index);
  }

  priceFor(productId: string): number {
    return this.products.find((p) => p.id === productId)?.price ?? 0;
  }

  lineTotal(index: number): number {
    const item = this.items.at(index).getRawValue();
    return this.priceFor(item.productId) * item.quantity;
  }

  get subtotal(): number {
    return this.items.controls.reduce((sum: number, _, index: number) => sum + this.lineTotal(index), 0);
  }

  get discountPreview(): DiscountPreview | null {
    const customer = this.customers.find((c) => c.id === this.form.controls.customerId.value);
    if (!customer) return null;
    return previewDiscount(this.subtotal, customer.tier);
  }

  submit(): void {
    if (this.form.invalid || this.items.length === 0) return;
    this.submitting = true;
    this.errorMessage = '';
    const value = this.form.getRawValue();
    this.orderService
      .create({
        customerId: value.customerId,
        items: value.items.map((i) => ({ productId: i.productId, quantity: i.quantity }))
      })
      .subscribe({
        next: (order) => this.router.navigate(['/orders', order.id]),
        error: () => {
          this.submitting = false;
          this.errorMessage = 'Failed to create order. Please check your inputs.';
        }
      });
  }
}

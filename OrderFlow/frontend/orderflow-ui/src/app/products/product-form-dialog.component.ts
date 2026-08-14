import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { ProductService } from '../core/services/product.service';

@Component({
  selector: 'app-product-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './product-form-dialog.component.html',
  styleUrl: './product-form-dialog.component.scss'
})
export class ProductFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly productService = inject(ProductService);
  private readonly dialogRef = inject(MatDialogRef<ProductFormDialogComponent, boolean>);

  submitting = false;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    sku: ['', Validators.required],
    price: [0, [Validators.required, Validators.min(0.01)]]
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting = true;
    this.productService.create(this.form.getRawValue()).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => (this.submitting = false)
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}

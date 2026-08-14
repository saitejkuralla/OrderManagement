import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { CustomerService } from '../core/services/customer.service';
import { CustomerTier } from '../core/models/customer.model';

@Component({
  selector: 'app-customer-form-dialog',
  standalone: true,
  imports: [ReactiveFormsModule, MatDialogModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule],
  templateUrl: './customer-form-dialog.component.html',
  styleUrl: './customer-form-dialog.component.scss'
})
export class CustomerFormDialogComponent {
  private readonly fb = inject(FormBuilder);
  private readonly customerService = inject(CustomerService);
  private readonly dialogRef = inject(MatDialogRef<CustomerFormDialogComponent, boolean>);

  readonly tiers: CustomerTier[] = ['Standard', 'Silver', 'Gold', 'VIP'];
  submitting = false;

  readonly form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    tier: ['Standard' as CustomerTier, Validators.required]
  });

  submit(): void {
    if (this.form.invalid) return;
    this.submitting = true;
    this.customerService.create(this.form.getRawValue()).subscribe({
      next: () => this.dialogRef.close(true),
      error: () => (this.submitting = false)
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}

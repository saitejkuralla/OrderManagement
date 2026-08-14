import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CustomerService } from '../core/services/customer.service';
import { Customer } from '../core/models/customer.model';
import { TierChipComponent } from '../shared/components/tier-chip/tier-chip.component';
import { EmptyStateComponent } from '../shared/components/empty-state/empty-state.component';
import { CustomerFormDialogComponent } from './customer-form-dialog.component';

@Component({
  selector: 'app-customers',
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
    TierChipComponent,
    EmptyStateComponent
  ],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.scss'
})
export class CustomersComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly dialog = inject(MatDialog);

  customers: Customer[] = [];
  searchTerm = '';
  loading = true;
  readonly displayedColumns = ['name', 'email', 'tier', 'orderCount'];

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.customerService.getAll().subscribe((customers) => {
      this.customers = customers;
      this.loading = false;
    });
  }

  get filteredCustomers(): Customer[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.customers;
    return this.customers.filter(
      (c) => c.name.toLowerCase().includes(term) || c.email.toLowerCase().includes(term)
    );
  }

  openCreateDialog(): void {
    const ref = this.dialog.open(CustomerFormDialogComponent, { width: '420px' });
    ref.afterClosed().subscribe((created) => {
      if (created) this.load();
    });
  }
}

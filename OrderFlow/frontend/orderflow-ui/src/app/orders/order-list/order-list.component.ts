import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { OrderService } from '../../core/services/order.service';
import { OrderStatus, OrderSummary } from '../../core/models/order.model';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-order-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    MatTableModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    EmptyStateComponent
  ],
  templateUrl: './order-list.component.html',
  styleUrl: './order-list.component.scss'
})
export class OrderListComponent implements OnInit {
  private readonly orderService = inject(OrderService);
  private readonly router = inject(Router);

  orders: OrderSummary[] = [];
  loading = true;
  searchTerm = '';
  statusFilter: OrderStatus | 'All' = 'All';
  readonly statuses: (OrderStatus | 'All')[] = ['All', 'Pending', 'Confirmed', 'Cancelled'];
  readonly displayedColumns = ['customerName', 'createdAt', 'status', 'total'];

  ngOnInit(): void {
    this.orderService.getAll().subscribe((orders) => {
      this.orders = orders;
      this.loading = false;
    });
  }

  get filteredOrders(): OrderSummary[] {
    const term = this.searchTerm.trim().toLowerCase();
    return this.orders.filter((o) => {
      const matchesTerm = !term || o.customerName.toLowerCase().includes(term);
      const matchesStatus = this.statusFilter === 'All' || o.status === this.statusFilter;
      return matchesTerm && matchesStatus;
    });
  }

  viewOrder(id: string): void {
    this.router.navigate(['/orders', id]);
  }
}

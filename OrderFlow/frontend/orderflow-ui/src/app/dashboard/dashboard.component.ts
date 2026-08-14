import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { CustomerService } from '../core/services/customer.service';
import { ProductService } from '../core/services/product.service';
import { OrderService } from '../core/services/order.service';
import { OrderSummary } from '../core/models/order.model';
import { EmptyStateComponent } from '../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatTableModule, EmptyStateComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly customerService = inject(CustomerService);
  private readonly productService = inject(ProductService);
  private readonly orderService = inject(OrderService);

  customerCount = 0;
  productCount = 0;
  orderCount = 0;
  totalRevenue = 0;
  recentOrders: OrderSummary[] = [];
  loading = true;
  readonly displayedColumns = ['customerName', 'createdAt', 'status', 'total'];

  ngOnInit(): void {
    forkJoin({
      customers: this.customerService.getAll(),
      products: this.productService.getAll(),
      orders: this.orderService.getAll()
    }).subscribe(({ customers, products, orders }) => {
      this.customerCount = customers.length;
      this.productCount = products.length;
      this.orderCount = orders.length;
      this.totalRevenue = orders.reduce((sum, o) => sum + o.total, 0);
      this.recentOrders = [...orders]
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
        .slice(0, 5);
      this.loading = false;
    });
  }
}

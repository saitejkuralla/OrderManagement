import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { OrderService } from '../../core/services/order.service';
import { Order } from '../../core/models/order.model';
import { TierChipComponent } from '../../shared/components/tier-chip/tier-chip.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, RouterLink, MatCardModule, MatTableModule, MatButtonModule, MatDialogModule, TierChipComponent],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.scss'
})
export class OrderDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly orderService = inject(OrderService);
  private readonly dialog = inject(MatDialog);

  order: Order | null = null;
  loading = true;
  actionInProgress = false;
  readonly itemColumns = ['productName', 'quantity', 'unitPrice', 'lineTotal'];

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;
    this.loading = true;
    this.orderService.getById(id).subscribe((order) => {
      this.order = order;
      this.loading = false;
    });
  }

  confirmOrder(): void {
    if (!this.order) return;
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: { title: 'Confirm Order', message: 'Confirm this order? This cannot be undone.', confirmLabel: 'Confirm' }
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed || !this.order) return;
      this.actionInProgress = true;
      this.orderService.confirm(this.order.id).subscribe({
        next: (order) => {
          this.order = order;
          this.actionInProgress = false;
        },
        error: () => (this.actionInProgress = false)
      });
    });
  }

  cancelOrder(): void {
    if (!this.order) return;
    const ref = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Cancel Order',
        message: 'Are you sure you want to cancel this order?',
        confirmLabel: 'Cancel Order'
      }
    });
    ref.afterClosed().subscribe((confirmed) => {
      if (!confirmed || !this.order) return;
      this.actionInProgress = true;
      this.orderService.cancel(this.order.id).subscribe({
        next: (order) => {
          this.order = order;
          this.actionInProgress = false;
        },
        error: () => (this.actionInProgress = false)
      });
    });
  }
}

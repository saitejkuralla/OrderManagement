import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { CustomerTier } from '../../../core/models/customer.model';

@Component({
  selector: 'app-tier-chip',
  standalone: true,
  imports: [CommonModule, MatChipsModule],
  template: `
    <mat-chip [ngClass]="'tier-chip tier-' + tier.toLowerCase()">{{ tier }}</mat-chip>
  `,
  styles: [`
    .tier-chip {
      font-weight: 600;
      font-size: 12px;
      min-height: 24px;
    }
    .tier-standard { background: #e0e0e0; color: #424242; }
    .tier-silver { background: #cfd8dc; color: #37474f; }
    .tier-gold { background: #ffe082; color: #7a5b00; }
    .tier-vip { background: #d1c4e9; color: #4527a0; }
  `]
})
export class TierChipComponent {
  @Input({ required: true }) tier!: CustomerTier;
}

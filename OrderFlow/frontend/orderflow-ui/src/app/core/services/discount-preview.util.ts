import { CustomerTier } from '../models/customer.model';

export interface DiscountPreviewLine {
  name: string;
  percentage: number;
}

export interface DiscountPreview {
  subtotal: number;
  appliedDiscounts: DiscountPreviewLine[];
  totalDiscountPercentage: number;
  discountAmount: number;
  finalTotal: number;
}

const TIER_PERCENTAGES: Record<CustomerTier, number> = {
  Standard: 0,
  Silver: 0.05,
  Gold: 0.1,
  VIP: 0.1
};

const LARGE_ORDER_THRESHOLD = 10000;
const LARGE_ORDER_PERCENTAGE = 0.05;
const MAX_DISCOUNT_PERCENTAGE = 0.2;

function round2(value: number): number {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

// Client-side mirror of the backend discount rules, used for a live preview only — the server response is authoritative.
export function previewDiscount(subtotal: number, tier: CustomerTier): DiscountPreview {
  const appliedDiscounts: DiscountPreviewLine[] = [];
  const tierPercentage = TIER_PERCENTAGES[tier];
  if (tierPercentage > 0) {
    appliedDiscounts.push({ name: `${tier} Discount`, percentage: tierPercentage });
  }
  if (subtotal > LARGE_ORDER_THRESHOLD) {
    appliedDiscounts.push({ name: 'Large Order Discount', percentage: LARGE_ORDER_PERCENTAGE });
  }

  const rawPercentage = appliedDiscounts.reduce((sum, d) => sum + d.percentage, 0);
  const totalDiscountPercentage = Math.min(rawPercentage, MAX_DISCOUNT_PERCENTAGE);
  const discountAmount = round2(subtotal * totalDiscountPercentage);
  const finalTotal = round2(subtotal - discountAmount);

  return { subtotal, appliedDiscounts, totalDiscountPercentage, discountAmount, finalTotal };
}

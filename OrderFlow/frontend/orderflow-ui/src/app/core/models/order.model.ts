import { CustomerTier } from './customer.model';

export type OrderStatus = 'Pending' | 'Confirmed' | 'Cancelled';

export interface AppliedDiscount {
  name: string;
  percentage: number;
  amount: number;
}

export interface OrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderSummary {
  id: string;
  customerName: string;
  createdAt: string;
  status: OrderStatus;
  subtotal: number;
  discountAmount: number;
  total: number;
}

export interface Order {
  id: string;
  customerId: string;
  customerName: string;
  customerTier: CustomerTier;
  createdAt: string;
  status: OrderStatus;
  subtotal: number;
  discountAmount: number;
  total: number;
  items: OrderItem[];
  appliedDiscounts: AppliedDiscount[];
}

export interface CreateOrderItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateOrderRequest {
  customerId: string;
  items: CreateOrderItemRequest[];
}

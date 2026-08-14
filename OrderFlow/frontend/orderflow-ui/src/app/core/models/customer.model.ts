export type CustomerTier = 'Standard' | 'Silver' | 'Gold' | 'VIP';

export interface Customer {
  id: string;
  name: string;
  email: string;
  tier: CustomerTier;
  orderCount: number;
}

export interface CreateCustomerRequest {
  name: string;
  email: string;
  tier: CustomerTier;
}

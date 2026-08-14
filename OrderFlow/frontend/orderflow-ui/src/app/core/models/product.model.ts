export interface Product {
  id: string;
  name: string;
  sku: string;
  price: number;
  isActive: boolean;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  price: number;
}

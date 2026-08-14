import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    loadComponent: () => import('./dashboard/dashboard.component').then((m) => m.DashboardComponent)
  },
  {
    path: 'customers',
    loadComponent: () => import('./customers/customers.component').then((m) => m.CustomersComponent)
  },
  {
    path: 'products',
    loadComponent: () => import('./products/products.component').then((m) => m.ProductsComponent)
  },
  {
    path: 'orders',
    loadComponent: () => import('./orders/order-list/order-list.component').then((m) => m.OrderListComponent)
  },
  {
    path: 'orders/new',
    loadComponent: () => import('./orders/order-create/order-create.component').then((m) => m.OrderCreateComponent)
  },
  {
    path: 'orders/:id',
    loadComponent: () => import('./orders/order-detail/order-detail.component').then((m) => m.OrderDetailComponent)
  },
  { path: '**', redirectTo: 'dashboard' }
];

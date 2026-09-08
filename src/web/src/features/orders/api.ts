import type { PagedResult } from '@/features/products/types';
import { apiFetch } from '@/shared/lib/api-fetch';
import type { OrderDetail, OrderSummary } from './types';

export function fetchOrders(page = 1, pageSize = 20): Promise<PagedResult<OrderSummary>> {
  return apiFetch<PagedResult<OrderSummary>>(`/api/v1/orders?page=${page}&pageSize=${pageSize}`);
}

export function fetchOrder(id: string): Promise<OrderDetail> {
  return apiFetch<OrderDetail>(`/api/v1/orders/${encodeURIComponent(id)}`);
}

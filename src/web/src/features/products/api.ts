import { apiFetch } from '@/shared/lib/api-fetch';
import type { PagedResult, Product } from './types';

export interface ProductListParams {
  category?: string;
  sort?: 'price-asc' | 'price-desc';
  page?: number;
  pageSize?: number;
}

function buildQuery(params: ProductListParams): string {
  const search = new URLSearchParams();

  if (params.category) search.set('category', params.category);
  if (params.sort) {
    const sortMap: Record<NonNullable<ProductListParams['sort']>, string> = {
      'price-asc': 'price_asc',
      'price-desc': 'price_desc',
    };

    search.set('sort', sortMap[params.sort]);
  }
  if (params.page !== undefined) search.set('page', String(params.page));
  if (params.pageSize !== undefined) search.set('pageSize', String(params.pageSize));

  const qs = search.toString();
  return qs ? `?${qs}` : '';
}

export function fetchProducts(params: ProductListParams): Promise<PagedResult<Product>> {
  return apiFetch<PagedResult<Product>>(`/api/v1/products${buildQuery(params)}`);
}

export function fetchProductBySlug(slug: string): Promise<Product> {
  return apiFetch<Product>(`/api/v1/products/${encodeURIComponent(slug)}`);
}

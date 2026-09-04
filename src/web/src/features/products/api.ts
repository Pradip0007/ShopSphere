import { apiFetch } from '@/shared/lib/api-fetch';
import type { PagedResult, Product, ProductDetail, Review } from './types';

export interface ProductListParams {
  categoryId?: string;
  sort?: 'price-asc' | 'price-desc';
  minPrice?: number;
  maxPrice?: number;
  page?: number;
  pageSize?: number;
}

export interface ProductCategory {
  id: string;
  name: string;
  slug: string;
}

export interface ProductSearchParams {
  q: string;
  page?: number;
  pageSize?: number;
}

function buildQuery(params: ProductListParams): string {
  const search = new URLSearchParams();

  if (params.categoryId) search.set('categoryId', params.categoryId);
  if (params.sort) {
    const sortMap: Record<NonNullable<ProductListParams['sort']>, string> = {
      'price-asc': 'price_asc',
      'price-desc': 'price_desc',
    };

    search.set('sort', sortMap[params.sort]);
  }

  if (params.minPrice !== undefined) {
    search.set('minPrice', String(params.minPrice));
  }

  if (params.maxPrice !== undefined) {
    search.set('maxPrice', String(params.maxPrice));
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

export function fetchProductDetail(slug: string): Promise<ProductDetail> {
  return apiFetch<ProductDetail>(`/api/v1/products/${encodeURIComponent(slug)}`);
}

export function fetchProductReviews(
  slug: string,
  page = 1,
  pageSize = 10,
): Promise<PagedResult<Review>> {
  const query = new URLSearchParams({
    page: String(page),
    pageSize: String(pageSize),
  });

  return apiFetch<PagedResult<Review>>(
    `/api/v1/products/${encodeURIComponent(slug)}/reviews?${query.toString()}`,
  );
}

export function fetchCategories(): Promise<ProductCategory[]> {
  return apiFetch<ProductCategory[]>('/api/v1/products/categories');
}

export function fetchProductSearch(params: ProductSearchParams): Promise<PagedResult<Product>> {
  const search = new URLSearchParams();

  search.set('q', params.q);

  if (params.page !== undefined) {
    search.set('page', String(params.page));
  }

  if (params.pageSize !== undefined) {
    search.set('pageSize', String(params.pageSize));
  }

  return apiFetch<PagedResult<Product>>(`/api/v1/products/search?${search.toString()}`);
}

import { queryOptions } from '@tanstack/react-query';
import { fetchProductBySlug, fetchProducts, type ProductListParams } from './api';

export const productsQueryKey = (params: ProductListParams): readonly unknown[] =>
  ['products', 'list', params] as const;

export const productsQueryOptions = (params: ProductListParams) =>
  queryOptions({
    queryKey: productsQueryKey(params),
    queryFn: () => fetchProducts(params),
  });

export const productDetailQueryOptions = (slug: string) =>
  queryOptions({
    queryKey: ['products', 'detail', slug] as const,
    queryFn: () => fetchProductBySlug(slug),
    staleTime: 60_000,
  });

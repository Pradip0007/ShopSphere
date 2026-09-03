import { infiniteQueryOptions, queryOptions } from '@tanstack/react-query';
import { fetchProductBySlug, fetchProducts, type ProductListParams } from './api';

export const productsQueryKey = (params: ProductListParams): readonly unknown[] =>
  ['products', 'list', params] as const;

export const productsQueryOptions = (params: ProductListParams) =>
  queryOptions({
    queryKey: productsQueryKey(params),
    queryFn: () => fetchProducts(params),
  });

export interface ProductInfiniteFilters {
  category?: string;
  sort?: 'price-asc' | 'price-desc';
  pageSize?: number;
}

export const productsInfiniteQueryOptions = (filters: ProductInfiniteFilters) =>
  infiniteQueryOptions({
    queryKey: ['products', 'infinite', filters] as const,
    queryFn: ({ pageParam }) =>
      fetchProducts({
        ...(filters.category !== undefined && {
          category: filters.category,
        }),
        ...(filters.sort !== undefined && {
          sort: filters.sort,
        }),
        page: pageParam,
        pageSize: filters.pageSize ?? 24,
      }),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.hasNext ? last.page + 1 : undefined),
  });

export const productDetailQueryOptions = (slug: string) =>
  queryOptions({
    queryKey: ['products', 'detail', slug] as const,
    queryFn: () => fetchProductBySlug(slug),
    staleTime: 60_000,
  });

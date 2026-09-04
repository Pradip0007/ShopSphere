import { infiniteQueryOptions, queryOptions } from '@tanstack/react-query';
import {
  fetchCategories,
  fetchProductDetail,
  fetchProductReviews,
  fetchProductSearch,
  fetchProducts,
  type ProductListParams,
} from './api';

export const productsQueryKey = (params: ProductListParams): readonly unknown[] =>
  ['products', 'list', params] as const;

export const productsQueryOptions = (params: ProductListParams) =>
  queryOptions({
    queryKey: productsQueryKey(params),
    queryFn: () => fetchProducts(params),
  });

export interface ProductInfiniteFilters {
  categoryId?: string;
  sort?: 'price-asc' | 'price-desc';
  minPrice?: number;
  maxPrice?: number;
  q?: string;
  pageSize?: number;
}

export const productsInfiniteQueryOptions = (filters: ProductInfiniteFilters) =>
  infiniteQueryOptions({
    queryKey: ['products', 'infinite', filters] as const,
    queryFn: ({ pageParam }) => {
      if (filters.q !== undefined && filters.q.length > 0) {
        return fetchProductSearch({
          q: filters.q,
          page: pageParam,
          pageSize: filters.pageSize ?? 24,
        });
      }

      return fetchProducts({
        ...(filters.categoryId !== undefined && {
          categoryId: filters.categoryId,
        }),
        ...(filters.sort !== undefined && {
          sort: filters.sort,
        }),
        ...(filters.minPrice !== undefined && {
          minPrice: filters.minPrice,
        }),
        ...(filters.maxPrice !== undefined && {
          maxPrice: filters.maxPrice,
        }),
        page: pageParam,
        pageSize: filters.pageSize ?? 24,
      });
    },
    initialPageParam: 1,
    getNextPageParam: (last) => (last.hasNext ? last.page + 1 : undefined),
  });

export const categoriesQueryOptions = () =>
  queryOptions({
    queryKey: ['products', 'categories'] as const,
    queryFn: fetchCategories,
    staleTime: 5 * 60_000,
  });

export const productDetailQueryOptions = (slug: string) =>
  queryOptions({
    queryKey: ['products', 'detail', slug] as const,
    queryFn: () => fetchProductDetail(slug),
    staleTime: 60_000,
  });

export const productReviewsQueryOptions = (slug: string) =>
  queryOptions({
    queryKey: ['products', 'reviews', slug] as const,
    queryFn: () => fetchProductReviews(slug),
    staleTime: 30_000,
  });

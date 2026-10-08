import { queryOptions, useMutation, useQueryClient } from '@tanstack/react-query';

import { addWishlistItem, fetchWishlist, removeWishlistItem } from './api';
import type { WishlistResponse } from './types';

export const WISHLIST_QUERY_KEY = ['wishlist'] as const;

export const wishlistQueryOptions = () =>
  queryOptions({
    queryKey: WISHLIST_QUERY_KEY,
    queryFn: fetchWishlist,
    staleTime: 30_000,
  });

function optimisticAdd(
  previous: WishlistResponse | undefined,
  productId: string,
): WishlistResponse {
  const base: WishlistResponse = previous ?? {
    items: [],
  };

  const existing = base.items.some((item) => item.productId === productId);

  if (existing) {
    return base;
  }

  return {
    ...base,
    items: [
      ...base.items,
      {
        productId,
        addedAtUtc: new Date().toISOString(),
      },
    ],
  };
}

function optimisticRemove(
  previous: WishlistResponse | undefined,
  productId: string,
): WishlistResponse | undefined {
  if (!previous) {
    return previous;
  }

  return {
    ...previous,
    items: previous.items.filter((item) => item.productId !== productId),
  };
}

export function useAddToWishlist(): ReturnType<
  typeof useMutation<void, Error, string, { previous: WishlistResponse | undefined }>
> {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: addWishlistItem,

    onMutate: async (productId) => {
      await queryClient.cancelQueries({
        queryKey: WISHLIST_QUERY_KEY,
      });

      const previous = queryClient.getQueryData<WishlistResponse>(WISHLIST_QUERY_KEY);

      queryClient.setQueryData<WishlistResponse>(WISHLIST_QUERY_KEY, (old) =>
        optimisticAdd(old, productId),
      );

      return { previous };
    },

    onError: (_error, _variables, context) => {
      if (context) {
        queryClient.setQueryData(WISHLIST_QUERY_KEY, context.previous);
      }
    },

    onSettled: () => {
      void queryClient.invalidateQueries({
        queryKey: WISHLIST_QUERY_KEY,
      });
    },
  });
}

export function useRemoveFromWishlist(): ReturnType<
  typeof useMutation<void, Error, string, { previous: WishlistResponse | undefined }>
> {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: removeWishlistItem,

    onMutate: async (productId) => {
      await queryClient.cancelQueries({
        queryKey: WISHLIST_QUERY_KEY,
      });

      const previous = queryClient.getQueryData<WishlistResponse>(WISHLIST_QUERY_KEY);

      queryClient.setQueryData<WishlistResponse>(WISHLIST_QUERY_KEY, (old) =>
        optimisticRemove(old, productId),
      );

      return { previous };
    },

    onError: (_error, _variables, context) => {
      if (context) {
        queryClient.setQueryData(WISHLIST_QUERY_KEY, context.previous);
      }
    },

    onSettled: () => {
      void queryClient.invalidateQueries({
        queryKey: WISHLIST_QUERY_KEY,
      });
    },
  });
}

import { queryOptions, useMutation, useQueryClient } from '@tanstack/react-query';

import { addCartItem, fetchCart, removeCartItem, updateCartItem } from './api';
import type { Cart } from './types';

export const CART_QUERY_KEY = ['cart'] as const;

export const cartQueryOptions = () =>
  queryOptions({
    queryKey: CART_QUERY_KEY,
    queryFn: fetchCart,
    staleTime: 30_000,
  });

function optimisticAdd(previous: Cart | undefined, productId: string, quantity: number): Cart {
  const base: Cart = previous ?? {
    key: '',
    totalUnits: 0,
    lines: [],
  };

  const existing = base.lines.find((line) => line.productId === productId);

  if (existing) {
    return {
      ...base,
      totalUnits: base.totalUnits + quantity,
      lines: base.lines.map((line) =>
        line.productId === productId ? { ...line, quantity: line.quantity + quantity } : line,
      ),
    };
  }

  return {
    ...base,
    totalUnits: base.totalUnits + quantity,
    lines: [
      ...base.lines,
      {
        productId,
        quantity,
      },
    ],
  };
}

function optimisticUpdate(
  previous: Cart | undefined,
  productId: string,
  quantity: number,
): Cart | undefined {
  if (!previous) {
    return previous;
  }

  const existing = previous.lines.find((line) => line.productId === productId);

  if (!existing) {
    return previous;
  }

  const totalUnits = previous.totalUnits - existing.quantity + quantity;

  if (quantity === 0) {
    return {
      ...previous,
      totalUnits,
      lines: previous.lines.filter((line) => line.productId !== productId),
    };
  }

  return {
    ...previous,
    totalUnits,
    lines: previous.lines.map((line) =>
      line.productId === productId ? { ...line, quantity } : line,
    ),
  };
}

function optimisticRemove(previous: Cart | undefined, productId: string): Cart | undefined {
  if (!previous) {
    return previous;
  }

  const existing = previous.lines.find((line) => line.productId === productId);

  if (!existing) {
    return previous;
  }

  return {
    ...previous,
    totalUnits: previous.totalUnits - existing.quantity,
    lines: previous.lines.filter((line) => line.productId !== productId),
  };
}

export function useAddToCart(): ReturnType<
  typeof useMutation<
    Cart,
    Error,
    { productId: string; quantity: number },
    { previous: Cart | undefined }
  >
> {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: addCartItem,

    onMutate: async ({ productId, quantity }) => {
      await queryClient.cancelQueries({
        queryKey: CART_QUERY_KEY,
      });

      const previous = queryClient.getQueryData<Cart>(CART_QUERY_KEY);

      queryClient.setQueryData<Cart>(CART_QUERY_KEY, (old) =>
        optimisticAdd(old, productId, quantity),
      );

      return { previous };
    },

    onError: (_error, _variables, context) => {
      if (context) {
        queryClient.setQueryData(CART_QUERY_KEY, context.previous);
      }
    },

    onSettled: () => {
      void queryClient.invalidateQueries({
        queryKey: CART_QUERY_KEY,
      });
    },
  });
}

export function useUpdateCartItem(): ReturnType<
  typeof useMutation<
    Cart,
    Error,
    { productId: string; quantity: number },
    { previous: Cart | undefined }
  >
> {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: updateCartItem,

    onMutate: async ({ productId, quantity }) => {
      await queryClient.cancelQueries({
        queryKey: CART_QUERY_KEY,
      });

      const previous = queryClient.getQueryData<Cart>(CART_QUERY_KEY);

      queryClient.setQueryData<Cart>(CART_QUERY_KEY, (old) =>
        optimisticUpdate(old, productId, quantity),
      );

      return { previous };
    },

    onError: (_error, _variables, context) => {
      if (context) {
        queryClient.setQueryData(CART_QUERY_KEY, context.previous);
      }
    },

    onSettled: () => {
      void queryClient.invalidateQueries({
        queryKey: CART_QUERY_KEY,
      });
    },
  });
}

export function useRemoveCartItem(): ReturnType<
  typeof useMutation<Cart, Error, string, { previous: Cart | undefined }>
> {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: removeCartItem,

    onMutate: async (productId) => {
      await queryClient.cancelQueries({
        queryKey: CART_QUERY_KEY,
      });

      const previous = queryClient.getQueryData<Cart>(CART_QUERY_KEY);

      queryClient.setQueryData<Cart>(CART_QUERY_KEY, (old) => optimisticRemove(old, productId));

      return { previous };
    },

    onError: (_error, _variables, context) => {
      if (context) {
        queryClient.setQueryData(CART_QUERY_KEY, context.previous);
      }
    },

    onSettled: () => {
      void queryClient.invalidateQueries({
        queryKey: CART_QUERY_KEY,
      });
    },
  });
}

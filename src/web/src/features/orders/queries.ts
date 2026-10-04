import { infiniteQueryOptions, queryOptions } from '@tanstack/react-query';

import { fetchOrder, fetchOrders } from './api';
import type { OrderStatus } from './types';

const terminalOrderStatuses: OrderStatus[] = [
  'PaymentFailed',
  'Confirmed',
  'Shipped',
  'Delivered',
  'Cancelled',
  'Refunded',
];

export const ordersInfiniteQueryOptions = () =>
  infiniteQueryOptions({
    queryKey: ['orders', 'list'] as const,
    queryFn: ({ pageParam }) => fetchOrders(pageParam),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.hasNext ? last.page + 1 : undefined),
  });

export const orderDetailQueryOptions = (id: string) =>
  queryOptions({
    queryKey: ['orders', 'detail', id] as const,
    queryFn: () => fetchOrder(id),
    staleTime: 15_000,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      return status && terminalOrderStatuses.includes(status) ? false : 2_000;
    },
    throwOnError: true,
  });

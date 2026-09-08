import { useInfiniteQuery, useQueryClient } from '@tanstack/react-query';
import { createFileRoute, Link } from '@tanstack/react-router';
import { useMemo } from 'react';

import { orderDetailQueryOptions, ordersInfiniteQueryOptions } from '@/features/orders/queries';
import { StatusBadge } from '@/features/orders/StatusBadge';
import { useIntersect } from '@/shared/lib/use-intersect';
import { Button } from '@/shared/ui';

export const Route = createFileRoute('/_authed/orders/')({
  component: OrdersPage,
});

function OrdersPage(): React.JSX.Element {
  const queryClient = useQueryClient();
  const query = useInfiniteQuery(ordersInfiniteQueryOptions());

  const items = useMemo(() => query.data?.pages.flatMap((page) => page.items) ?? [], [query.data]);

  const sentinelRef = useIntersect<HTMLDivElement>({
    onIntersect: () => {
      if (query.hasNextPage && !query.isFetchingNextPage) {
        void query.fetchNextPage();
      }
    },
    enabled: query.hasNextPage && !query.isPending,
  });

  if (query.isPending) {
    return (
      <section className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your orders</h1>
        <p>Loading orders…</p>
      </section>
    );
  }

  if (query.isError) {
    return (
      <section className="grid gap-3">
        <h1 className="text-2xl font-semibold">Your orders</h1>

        <p role="alert" className="text-[var(--color-danger)]">
          Could not load: {query.error.message}
        </p>

        <Button onClick={() => void query.refetch()}>Try again</Button>
      </section>
    );
  }

  if (items.length === 0) {
    return (
      <section className="grid gap-3">
        <h1 className="text-2xl font-semibold">Your orders</h1>

        <p>No orders yet.</p>

        <Button asChild>
          <Link to="/products">Browse products</Link>
        </Button>
      </section>
    );
  }

  return (
    <section className="grid gap-4">
      <h1 className="text-2xl font-semibold">Your orders</h1>

      <ul className="grid list-none gap-3 p-0">
        {items.map((order) => (
          <li key={order.id}>
            <Link
              to="/orders/$id"
              params={{ id: order.id }}
              onMouseEnter={() => {
                void queryClient.prefetchQuery(orderDetailQueryOptions(order.id));
              }}
              className="grid grid-cols-[auto_1fr_auto_auto] items-center gap-4 rounded-lg border border-[var(--color-border)] p-4 hover:bg-[var(--color-surface-muted)]"
            >
              <div className="grid gap-1">
                <span className="text-sm text-[var(--color-text-muted)]">Order</span>

                <strong>{order.number}</strong>
              </div>

              <div className="text-sm text-[var(--color-text-muted)]">
                {new Date(order.placedUtc).toLocaleDateString()} · {order.itemCount} items
              </div>

              <StatusBadge status={order.status} />

              <strong>
                {new Intl.NumberFormat(undefined, {
                  style: 'currency',
                  currency: order.currency,
                }).format(order.totalAmount)}
              </strong>
            </Link>
          </li>
        ))}
      </ul>

      <div ref={sentinelRef} aria-hidden="true" className="h-1" />

      {query.isFetchingNextPage && (
        <p className="text-center text-sm text-[var(--color-text-muted)]">Loading more…</p>
      )}
    </section>
  );
}

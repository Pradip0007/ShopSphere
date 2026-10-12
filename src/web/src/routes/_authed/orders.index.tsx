import { useInfiniteQuery, useQueryClient } from '@tanstack/react-query';
import { createFileRoute, Link } from '@tanstack/react-router';
import { type FormEvent, useMemo, useState } from 'react';

import { orderDetailQueryOptions, ordersInfiniteQueryOptions } from '@/features/orders/queries';
import { StatusBadge } from '@/features/orders/StatusBadge';
import type { OrderStatus, OrderSummary } from '@/features/orders/types';
import { useIntersect } from '@/shared/lib/use-intersect';
import { Button } from '@/shared/ui';

export const Route = createFileRoute('/_authed/orders/')({
  component: OrdersPage,
});

type StatusFilter = 'on-the-way' | 'delivered' | 'cancelled' | 'returned';
type TimeFilter = 'last-30-days' | 'this-year' | 'last-year' | 'older';

const statusFilters: { id: StatusFilter; label: string }[] = [
  { id: 'on-the-way', label: 'On the way' },
  { id: 'delivered', label: 'Delivered' },
  { id: 'cancelled', label: 'Cancelled' },
  { id: 'returned', label: 'Returned' },
];

function OrdersPage(): React.JSX.Element {
  const queryClient = useQueryClient();
  const query = useInfiniteQuery(ordersInfiniteQueryOptions());
  const [searchInput, setSearchInput] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedStatuses, setSelectedStatuses] = useState<StatusFilter[]>([]);
  const [selectedTimes, setSelectedTimes] = useState<TimeFilter[]>([]);

  const items = useMemo(() => query.data?.pages.flatMap((page) => page.items) ?? [], [query.data]);
  const filteredOrders = useMemo(
    () =>
      items.filter((order) => {
        const matchesSearch =
          !searchTerm ||
          `${order.number} ${order.status} ${order.placedUtc}`.toLowerCase().includes(searchTerm);
        const matchesStatus =
          selectedStatuses.length === 0 ||
          selectedStatuses.some((filter) => matchesOrderStatus(order.status, filter));
        const matchesTime =
          selectedTimes.length === 0 ||
          selectedTimes.some((filter) => matchesOrderDate(order.placedUtc, filter));

        return matchesSearch && matchesStatus && matchesTime;
      }),
    [items, searchTerm, selectedStatuses, selectedTimes],
  );

  const sentinelRef = useIntersect<HTMLDivElement>({
    onIntersect: () => {
      if (query.hasNextPage && !query.isFetchingNextPage) {
        void query.fetchNextPage();
      }
    },
    enabled: query.hasNextPage && !query.isPending,
  });

  function toggleFilter<T extends string>(
    values: T[],
    setValues: (nextValues: T[]) => void,
    value: T,
  ): void {
    setValues(
      values.includes(value) ? values.filter((item) => item !== value) : [...values, value],
    );
  }

  function clearFilters(): void {
    setSearchInput('');
    setSearchTerm('');
    setSelectedStatuses([]);
    setSelectedTimes([]);
  }

  if (query.isPending) {
    return (
      <section className="mx-auto grid max-w-7xl gap-5">
        <h1 className="text-2xl font-semibold text-slate-900">My Orders</h1>
        <div className="grid gap-3">
          {['first', 'second', 'third'].map((skeleton) => (
            <div
              key={skeleton}
              className="h-32 animate-pulse rounded-lg border border-slate-200 bg-white"
            />
          ))}
        </div>
      </section>
    );
  }

  if (query.isError) {
    return (
      <section className="mx-auto grid max-w-xl gap-4 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h1 className="text-2xl font-semibold text-slate-900">My Orders</h1>
        <p role="alert" className="text-[var(--color-danger)]">
          Could not load your orders: {query.error.message}
        </p>
        <Button onClick={() => void query.refetch()}>Try again</Button>
      </section>
    );
  }

  return (
    <section className="mx-auto grid max-w-7xl gap-5">
      <nav aria-label="Breadcrumb" className="flex items-center gap-2 text-sm text-slate-500">
        <Link to="/" className="hover:text-blue-700 hover:underline">
          Home
        </Link>
        <span aria-hidden="true">›</span>
        <Link to="/orders" className="hover:text-blue-700 hover:underline">
          My Account
        </Link>
        <span aria-hidden="true">›</span>
        <span aria-current="page" className="font-medium text-slate-700">
          My Orders
        </span>
      </nav>

      <div className="grid items-start gap-5 lg:grid-cols-[17rem_minmax(0,1fr)]">
        <aside className="overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
          <h1 className="border-b border-slate-100 px-5 py-4 text-2xl font-bold text-slate-900">
            Filters
          </h1>
          <fieldset className="grid gap-3 border-b border-slate-100 px-5 py-4">
            <legend className="mb-1 text-sm font-bold uppercase tracking-wide text-slate-700">
              Order status
            </legend>
            {statusFilters.map((filter) => (
              <label
                key={filter.id}
                className="flex cursor-pointer items-center gap-3 text-sm text-slate-700"
              >
                <input
                  type="checkbox"
                  checked={selectedStatuses.includes(filter.id)}
                  onChange={() => toggleFilter(selectedStatuses, setSelectedStatuses, filter.id)}
                  className="h-4 w-4 rounded border-slate-300 accent-blue-600"
                />
                {filter.label}
              </label>
            ))}
          </fieldset>

          <fieldset className="grid gap-3 px-5 py-4">
            <legend className="mb-1 text-sm font-bold uppercase tracking-wide text-slate-700">
              Order time
            </legend>
            {(
              [
                ['last-30-days', 'Last 30 days'],
                ['this-year', String(new Date().getFullYear())],
                ['last-year', String(new Date().getFullYear() - 1)],
                ['older', 'Older'],
              ] as const
            ).map(([id, label]) => (
              <label
                key={id}
                className="flex cursor-pointer items-center gap-3 text-sm text-slate-700"
              >
                <input
                  type="checkbox"
                  checked={selectedTimes.includes(id)}
                  onChange={() => toggleFilter(selectedTimes, setSelectedTimes, id)}
                  className="h-4 w-4 rounded border-slate-300 accent-blue-600"
                />
                {label}
              </label>
            ))}
          </fieldset>
          {(selectedStatuses.length > 0 || selectedTimes.length > 0 || searchTerm) && (
            <div className="border-t border-slate-100 px-5 py-3">
              <button
                type="button"
                onClick={clearFilters}
                className="text-sm font-semibold text-blue-700 hover:underline"
              >
                Clear all filters
              </button>
            </div>
          )}
        </aside>

        <div className="grid min-w-0 gap-5">
          <search className="block">
            <form
              onSubmit={(event: FormEvent<HTMLFormElement>) => {
                event.preventDefault();
                setSearchTerm(searchInput.trim().toLowerCase());
              }}
              className="flex min-h-14 overflow-hidden rounded-lg border border-slate-300 bg-white shadow-sm focus-within:ring-2 focus-within:ring-blue-500"
            >
              <label className="sr-only" htmlFor="order-search">
                Search your orders
              </label>
              <input
                id="order-search"
                type="search"
                value={searchInput}
                onChange={(event) => setSearchInput(event.target.value)}
                placeholder="Search your orders here"
                className="min-w-0 flex-1 px-4 text-base text-slate-900 outline-none placeholder:text-slate-400"
              />
              <button
                type="submit"
                className="flex shrink-0 items-center gap-2 bg-blue-600 px-4 font-semibold text-white transition-colors hover:bg-blue-700 sm:px-6"
              >
                <svg
                  aria-hidden="true"
                  viewBox="0 0 24 24"
                  className="h-5 w-5"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="2"
                >
                  <circle cx="10.8" cy="10.8" r="6.8" />
                  <path d="m16 16 5 5" strokeLinecap="round" />
                </svg>
                <span className="hidden sm:inline">Search Orders</span>
              </button>
            </form>
          </search>

          <div className="flex flex-wrap items-end justify-between gap-2">
            <div>
              <h2 className="text-xl font-bold text-slate-900">Your orders</h2>
              <p className="mt-1 text-sm text-slate-500">
                {filteredOrders.length} {filteredOrders.length === 1 ? 'order' : 'orders'}
                {searchTerm || selectedStatuses.length > 0 || selectedTimes.length > 0
                  ? ' match your filters'
                  : ''}
              </p>
            </div>
            {(searchTerm || selectedStatuses.length > 0 || selectedTimes.length > 0) && (
              <button
                type="button"
                onClick={clearFilters}
                className="text-sm font-semibold text-blue-700 hover:underline"
              >
                Clear filters
              </button>
            )}
          </div>

          {items.length === 0 ? (
            <div className="grid justify-items-center gap-3 rounded-lg border border-slate-200 bg-white px-6 py-12 text-center shadow-sm">
              <div className="grid h-14 w-14 place-items-center rounded-full bg-blue-50 text-blue-700">
                <PackageIcon className="h-7 w-7" />
              </div>
              <h2 className="text-lg font-semibold text-slate-900">No orders yet</h2>
              <p className="max-w-sm text-sm text-slate-500">
                Once you place an order, you can find its status and details here.
              </p>
              <Button asChild className="mt-2 bg-blue-700 text-white hover:bg-blue-800">
                <Link to="/products">Browse products</Link>
              </Button>
            </div>
          ) : filteredOrders.length === 0 ? (
            <div className="grid justify-items-center gap-3 rounded-lg border border-slate-200 bg-white px-6 py-12 text-center shadow-sm">
              <h2 className="text-lg font-semibold text-slate-900">No matching orders</h2>
              <p className="text-sm text-slate-500">Try another search or clear your filters.</p>
              <button
                type="button"
                onClick={clearFilters}
                className="font-semibold text-blue-700 hover:underline"
              >
                Clear search and filters
              </button>
            </div>
          ) : (
            <ul className="grid list-none gap-3 p-0">
              {filteredOrders.map((order) => (
                <li key={order.id}>
                  <OrderCard
                    order={order}
                    onPrefetch={() => {
                      void queryClient.prefetchQuery(orderDetailQueryOptions(order.id));
                    }}
                  />
                </li>
              ))}
            </ul>
          )}

          <div ref={sentinelRef} aria-hidden="true" className="h-1" />
          {query.isFetchingNextPage && (
            <p className="text-center text-sm text-slate-500">Loading more orders…</p>
          )}
        </div>
      </div>
    </section>
  );
}

function OrderCard({
  order,
  onPrefetch,
}: {
  order: OrderSummary;
  onPrefetch: () => void;
}): React.JSX.Element {
  const date = new Date(order.placedUtc);
  const total = new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: order.currency,
  }).format(order.totalAmount);
  const statusLabel = getStatusLabel(order.status);

  return (
    <Link
      to="/orders/$id"
      params={{ id: order.id }}
      onMouseEnter={onPrefetch}
      className="group grid gap-4 rounded-lg border border-slate-200 bg-white p-4 shadow-sm transition hover:border-blue-300 hover:shadow-md sm:grid-cols-[5rem_minmax(0,1fr)_minmax(10rem,auto)] sm:items-center sm:gap-5 sm:p-5"
    >
      <div
        aria-hidden="true"
        className="grid h-20 w-20 place-items-center rounded-md bg-slate-50 text-slate-400"
      >
        <PackageIcon className="h-10 w-10" />
      </div>

      <div className="grid min-w-0 gap-2">
        <div className="flex flex-wrap items-center gap-2">
          <h3 className="truncate text-base font-semibold text-slate-900 group-hover:text-blue-700">
            Order {order.number}
          </h3>
          <StatusBadge status={order.status} />
        </div>
        <p className="text-sm text-slate-500">
          {order.itemCount} {order.itemCount === 1 ? 'item' : 'items'} · Placed{' '}
          {date.toLocaleDateString(undefined, {
            month: 'short',
            day: '2-digit',
            year: 'numeric',
          })}
        </p>
        <p className="text-sm font-medium text-slate-700">{statusLabel}</p>
      </div>

      <div className="flex items-center justify-between gap-4 border-t border-slate-100 pt-3 sm:grid sm:justify-items-end sm:border-0 sm:pt-0">
        <strong className="text-lg text-slate-900">{total}</strong>
        <span className="text-sm font-semibold text-blue-700">
          View order <span aria-hidden>→</span>
        </span>
      </div>
    </Link>
  );
}

function matchesOrderStatus(status: OrderStatus, filter: StatusFilter): boolean {
  switch (filter) {
    case 'on-the-way':
      return [
        'Placed',
        'InventoryReserved',
        'PaymentAuthorized',
        'Confirmed',
        'Paid',
        'Shipped',
      ].includes(status);
    case 'delivered':
      return status === 'Delivered';
    case 'cancelled':
      return status === 'Cancelled' || status === 'PaymentFailed';
    case 'returned':
      return status === 'Refunded';
  }
}

function matchesOrderDate(dateValue: string, filter: TimeFilter): boolean {
  const date = new Date(dateValue);
  const now = new Date();
  const startOfThisYear = new Date(now.getFullYear(), 0, 1);
  const startOfLastYear = new Date(now.getFullYear() - 1, 0, 1);
  const startOfNextYear = new Date(now.getFullYear() + 1, 0, 1);

  switch (filter) {
    case 'last-30-days': {
      const thirtyDaysAgo = new Date(now);
      thirtyDaysAgo.setDate(now.getDate() - 30);
      return date >= thirtyDaysAgo && date <= now;
    }
    case 'this-year':
      return date >= startOfThisYear && date < startOfNextYear;
    case 'last-year':
      return date >= startOfLastYear && date < startOfThisYear;
    case 'older':
      return date < startOfLastYear;
  }
}

function getStatusLabel(status: OrderStatus): string {
  switch (status) {
    case 'Delivered':
      return 'Your order has been delivered.';
    case 'Cancelled':
      return 'This order was cancelled.';
    case 'Refunded':
      return 'This order has been refunded.';
    case 'PaymentFailed':
      return 'Payment could not be completed.';
    case 'Shipped':
      return 'Your order is on the way.';
    default:
      return 'We are preparing your order.';
  }
}

function PackageIcon({ className }: { className: string }): React.JSX.Element {
  return (
    <svg
      aria-hidden="true"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.5"
      className={className}
    >
      <path d="m12 3 8 4.5v9L12 21l-8-4.5v-9L12 3Z" strokeLinejoin="round" />
      <path d="m4.5 7.8 7.5 4.4 7.5-4.4M12 12.2V21M8 5.3l8 4.6" strokeLinejoin="round" />
    </svg>
  );
}

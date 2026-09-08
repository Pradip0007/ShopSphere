import { createFileRoute, Link } from '@tanstack/react-router';
import { Suspense } from 'react';

import { OrderDetail } from '@/features/orders/OrderDetail';
import { OrderDetailSkeleton } from '@/features/orders/OrderDetailSkeleton';
import { orderDetailQueryOptions } from '@/features/orders/queries';
import { ApiError } from '@/shared/lib/api-error';
import { Button } from '@/shared/ui';

function OrderNotFound(): React.JSX.Element {
  return (
    <section className="grid max-w-2xl gap-3">
      <h1 className="text-2xl font-semibold">Order not found</h1>

      <p className="text-[var(--color-text-muted)]">We couldn't find that order.</p>

      <div>
        <Button asChild>
          <Link to="/orders">Back to orders</Link>
        </Button>
      </div>
    </section>
  );
}

export const Route = createFileRoute('/_authed/orders/$id')({
  loader: ({ context, params }) =>
    context.queryClient.ensureQueryData(orderDetailQueryOptions(params.id)),
  errorComponent: OrderDetailError,
  notFoundComponent: OrderNotFound,
  component: OrderDetailPage,
});

function OrderDetailPage(): React.JSX.Element {
  const { id } = Route.useParams();

  return (
    <Suspense fallback={<OrderDetailSkeleton />}>
      <OrderDetail id={id} />
    </Suspense>
  );
}

function OrderDetailError({ error }: { error: unknown }): React.JSX.Element {
  const isNotFound = error instanceof ApiError && error.isNotFound;

  return (
    <section className="grid max-w-2xl gap-3">
      <h1 className="text-2xl font-semibold">
        {isNotFound ? 'Order not found' : 'Could not load this order'}
      </h1>

      <p className="text-[var(--color-text-muted)]">
        {isNotFound
          ? "We couldn't find that order."
          : error instanceof Error
            ? error.message
            : 'Something went wrong while loading the order.'}
      </p>

      <div>
        <Button asChild>
          <Link to="/orders">Back to orders</Link>
        </Button>
      </div>
    </section>
  );
}

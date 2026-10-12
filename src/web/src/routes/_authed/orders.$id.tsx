import { createFileRoute } from '@tanstack/react-router';
import { Suspense } from 'react';
import { z } from 'zod';
import { AppError } from '@/app/AppError';
import { OrderDetail } from '@/features/orders/OrderDetail';
import { OrderDetailSkeleton } from '@/features/orders/OrderDetailSkeleton';

export const Route = createFileRoute('/_authed/orders/$id')({
  validateSearch: z.object({ placed: z.boolean().optional() }),
  pendingComponent: OrderDetailSkeleton,
  pendingMs: 200,
  errorComponent: ({ error, reset }) => <AppError error={error} resetErrorBoundary={reset} />,
  component: OrderDetailPage,
});

function OrderDetailPage(): React.JSX.Element {
  const { id } = Route.useParams();
  const { placed } = Route.useSearch();

  return (
    <Suspense fallback={<OrderDetailSkeleton />}>
      <OrderDetail id={id} justPlaced={placed === true} />
    </Suspense>
  );
}

import { createFileRoute } from '@tanstack/react-router';
import { Suspense } from 'react';
import { AppError } from '@/app/AppError';
import { OrderDetail } from '@/features/orders/OrderDetail';
import { OrderDetailSkeleton } from '@/features/orders/OrderDetailSkeleton';

export const Route = createFileRoute('/_authed/orders/$id')({
  pendingComponent: OrderDetailSkeleton,
  pendingMs: 200,
  errorComponent: ({ error, reset }) => <AppError error={error} resetErrorBoundary={reset} />,
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

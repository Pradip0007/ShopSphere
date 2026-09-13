import { createFileRoute } from '@tanstack/react-router';

import { OrderFeedPanel } from '@/features/orders/OrderFeedPanel';

export const Route = createFileRoute('/admin/orders')({
  component: AdminOrders,
});

function AdminOrders(): React.JSX.Element {
  return (
    <section className="grid gap-6">
      <header>
        <h1 className="text-2xl font-semibold">Admin · Orders</h1>
        <p className="text-sm text-[var(--color-text-muted)]">
          Monitor order status changes in real time.
        </p>
      </header>

      <OrderFeedPanel />
    </section>
  );
}

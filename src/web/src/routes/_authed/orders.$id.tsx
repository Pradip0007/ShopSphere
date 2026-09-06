import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/_authed/orders/$id')({
  component: OrderDetailPage,
});

function OrderDetailPage(): React.JSX.Element {
  const { id } = Route.useParams();

  return (
    <section className="grid gap-3">
      <h1 className="text-2xl font-semibold">Thanks for your order</h1>

      <p>
        Order ID: <code>{id}</code>
      </p>

      <p className="text-[var(--color-text-muted)]">Full order view lands Day 66.</p>
    </section>
  );
}

import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/admin/orders')({
  component: AdminOrders,
});

function AdminOrders(): React.JSX.Element {
  return (
    <section>
      <h1>Admin · Orders</h1>
      <p style={{ opacity: 0.6 }}>Ops view lands in Phase 5.</p>
    </section>
  );
}

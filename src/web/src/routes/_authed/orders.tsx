import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/_authed/orders')({
  component: OrdersPage,
});

function OrdersPage(): React.JSX.Element {
  return (
    <section>
      <h1>My Orders</h1>
      <p style={{ opacity: 0.6 }}>List lands Day 66.</p>
    </section>
  );
}

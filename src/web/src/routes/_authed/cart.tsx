import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/_authed/cart')({
  component: CartPage,
});

function CartPage(): React.JSX.Element {
  return (
    <section>
      <h1>Your Cart</h1>
      <p style={{ opacity: 0.6 }}>
        Interactive drawer lands Day 64. This page is the "cart deep-link" fallback.
      </p>
    </section>
  );
}

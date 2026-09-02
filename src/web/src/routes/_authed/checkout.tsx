import { createFileRoute } from '@tanstack/react-router';

export const Route = createFileRoute('/_authed/checkout')({
  component: CheckoutPage,
});

function CheckoutPage(): React.JSX.Element {
  return (
    <section>
      <h1>Checkout</h1>

      <p style={{ opacity: 0.6 }}>Wizard lands Day 65.</p>
    </section>
  );
}

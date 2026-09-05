import { useQuery } from '@tanstack/react-query';
import { createFileRoute, Link } from '@tanstack/react-router';
import { CartLineRow } from '@/features/cart/CartLineRow';
import { cartQueryOptions } from '@/features/cart/queries';
import { Button } from '@/shared/ui';

export const Route = createFileRoute('/_authed/cart')({
  component: CartPage,
});

function CartPage(): React.JSX.Element {
  const query = useQuery(cartQueryOptions());

  if (query.isPending) {
    return (
      <section className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your cart</h1>

        <p className="text-[var(--color-text-muted)]">Loading cart…</p>
      </section>
    );
  }

  if (query.isError) {
    return (
      <section className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your cart</h1>

        <p role="alert" className="text-[var(--color-danger)]">
          Could not load cart.
        </p>

        <Button onClick={() => void query.refetch()}>Try again</Button>
      </section>
    );
  }

  const cart = query.data;

  if (cart.lines.length === 0) {
    return (
      <section className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your cart</h1>

        <p>Your cart is empty.</p>

        <Button asChild>
          <Link to="/products">Browse products</Link>
        </Button>
      </section>
    );
  }

  return (
    <section className="grid max-w-2xl gap-6">
      <h1 className="text-2xl font-semibold">Your cart</h1>

      <ul className="m-0 list-none p-0">
        {cart.lines.map((line) => (
          <CartLineRow key={line.productId} line={line} />
        ))}
      </ul>

      <footer className="flex items-center justify-between border-t border-[var(--color-border)] pt-4">
        <p className="text-lg font-semibold">Total items: {cart.totalUnits}</p>

        <Button asChild size="lg">
          <Link to="/checkout">Proceed to checkout</Link>
        </Button>
      </footer>
    </section>
  );
}

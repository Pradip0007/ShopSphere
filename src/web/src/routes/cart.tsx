import { useQuery } from '@tanstack/react-query';
import { createFileRoute, Link, useNavigate } from '@tanstack/react-router';
import { useState } from 'react';
import { CartLineRow } from '@/features/cart/CartLineRow';
import { cartQueryOptions } from '@/features/cart/queries';
import { productsQueryOptions } from '@/features/products/queries';
import { Button } from '@/shared/ui';

export const Route = createFileRoute('/cart')({
  component: CartPage,
});

function CartPage(): React.JSX.Element {
  const cartQuery = useQuery(cartQueryOptions());
  const productsQuery = useQuery({
    ...productsQueryOptions({ page: 1, pageSize: 100 }),
    enabled: Boolean(cartQuery.data?.lines.length),
  });
  const navigate = useNavigate();
  const [showPincodeInput, setShowPincodeInput] = useState(false);
  const [pincode, setPincode] = useState('');

  if (cartQuery.isPending) {
    return (
      <section className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your cart</h1>
        <p className="text-[var(--color-text-muted)]">Loading cart…</p>
      </section>
    );
  }

  if (cartQuery.isError) {
    return (
      <section className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your cart</h1>
        <p role="alert" className="text-[var(--color-danger)]">
          Could not load cart.
        </p>
        <Button onClick={() => void cartQuery.refetch()}>Try again</Button>
      </section>
    );
  }

  const cart = cartQuery.data;
  const products = productsQuery.data?.items ?? [];
  const cartProducts = cart.lines.map((line) =>
    products.find((product) => product.id === line.productId),
  );
  const hasPricesForAllItems = cart.lines.every((line) =>
    products.some((product) => product.id === line.productId),
  );
  const currency = cartProducts.find((product) => product !== undefined)?.currency;
  const subtotal =
    currency && hasPricesForAllItems
      ? cart.lines.reduce((total, line) => {
          const product = products.find((item) => item.id === line.productId);
          return total + (product?.price ?? 0) * line.quantity;
        }, 0)
      : null;
  const formatPrice = (amount: number): string =>
    new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: currency ?? 'USD',
    }).format(amount);

  if (cart.lines.length === 0) {
    return (
      <section data-testid="cart-page-content" className="grid gap-4">
        <h1 className="text-2xl font-semibold">Your cart</h1>
        <p>Your cart is empty.</p>
        <Button asChild>
          <Link to="/products">Browse products</Link>
        </Button>
      </section>
    );
  }

  return (
    <section
      data-testid="cart-page-content"
      className="mx-auto grid w-full max-w-7xl items-start gap-5 lg:grid-cols-[minmax(0,2fr)_minmax(19rem,0.95fr)]"
    >
      <div className="grid min-w-0 gap-4">
        <header className="bg-white p-5 shadow-sm">
          <h1 className="text-2xl font-semibold text-slate-900">Your cart</h1>
          <p className="mt-1 text-sm text-slate-500">
            {cart.totalUnits} item{cart.totalUnits === 1 ? '' : 's'}
          </p>
        </header>

        <section className="flex flex-wrap items-center justify-between gap-3 bg-white px-5 py-4 shadow-sm">
          <div>
            <h2 className="font-medium text-slate-900">From Saved Addresses</h2>
            <p className="mt-1 text-sm text-slate-500">
              Address details are not available yet.
              {pincode && <span className="ml-1">Delivery pincode: {pincode}</span>}
            </p>
          </div>
          <Button
            variant="outline"
            className="border-blue-600 text-blue-700 hover:bg-blue-50"
            onClick={() => setShowPincodeInput((show) => !show)}
          >
            {showPincodeInput ? 'Close' : 'Enter Delivery Pincode'}
          </Button>
          {showPincodeInput && (
            <label className="flex w-full items-center gap-2 text-sm sm:ml-auto sm:w-auto">
              <span className="sr-only">Delivery pincode</span>
              <input
                inputMode="numeric"
                maxLength={6}
                placeholder="6-digit pincode"
                value={pincode}
                onChange={(event) => setPincode(event.target.value.replace(/\D/g, '').slice(0, 6))}
                className="h-10 rounded border border-slate-300 px-3"
              />
              <span className="text-slate-500">Placeholder only</span>
            </label>
          )}
        </section>

        {productsQuery.isError && (
          <p role="alert" className="bg-white p-4 text-sm text-[var(--color-danger)]">
            Product details are unavailable. The cart actions are still available.
          </p>
        )}

        <ul className="m-0 grid list-none gap-4 p-0">
          {cart.lines.map((line) => (
            <CartLineRow
              key={line.productId}
              line={line}
              product={products.find((item) => item.id === line.productId)}
              onBuyNow={() => {
                void navigate({ to: '/checkout' });
              }}
            />
          ))}
        </ul>
      </div>

      <aside className="grid gap-4 lg:sticky lg:top-4">
        <section className="bg-white shadow-sm">
          <h2 className="border-b-4 border-slate-100 px-5 py-4 text-xl font-semibold text-slate-800">
            Price Details
          </h2>
          <dl className="grid gap-4 px-5 py-5 text-sm">
            <div className="flex justify-between gap-4">
              <dt className="text-slate-600">Price ({cart.totalUnits} items)</dt>
              <dd className="font-medium text-slate-900">
                {subtotal === null ? 'Unavailable' : formatPrice(subtotal)}
              </dd>
            </div>
            <div className="flex justify-between gap-4">
              <dt className="text-slate-600">Delivery charges</dt>
              <dd className="font-medium text-green-700">FREE</dd>
            </div>
            <div className="flex justify-between gap-4 border-b border-dashed border-slate-300 pb-4">
              <dt className="text-slate-600">Discount</dt>
              <dd className="font-medium text-green-700">—</dd>
            </div>
            <div className="flex justify-between gap-4 text-base font-semibold text-slate-900">
              <dt>Total Amount</dt>
              <dd>{subtotal === null ? 'Unavailable' : formatPrice(subtotal)}</dd>
            </div>
            <p className="rounded-md bg-emerald-50 px-3 py-3 text-center font-medium text-emerald-700">
              Delivery estimates and discounts will be shown when available.
            </p>
          </dl>
        </section>

        <p className="px-3 text-sm font-medium text-slate-500">
          ◈ Safe and secure payments. Easy returns. Product and delivery details are placeholders.
        </p>

        <section className="grid gap-4 bg-white p-4 shadow-sm sm:grid-cols-[1fr_auto] sm:items-center">
          <div>
            <p className="text-xs text-slate-400 line-through">—</p>
            <p className="text-2xl font-bold text-slate-900">
              {subtotal === null ? 'Unavailable' : formatPrice(subtotal)}
            </p>
          </div>
          <Button
            variant="primary"
            size="lg"
            className="bg-amber-400 text-slate-900 hover:bg-amber-300"
            onClick={() => {
              void navigate({ to: '/checkout' });
            }}
          >
            Place order
          </Button>
        </section>
      </aside>
    </section>
  );
}

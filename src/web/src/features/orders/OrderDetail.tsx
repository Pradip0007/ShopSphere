import { useSuspenseQuery } from '@tanstack/react-query';
import { Link } from '@tanstack/react-router';

import { Button } from '@/shared/ui';
import { AddressBlock } from './AddressBlock';
import { orderDetailQueryOptions } from './queries';
import { StatusBadge } from './StatusBadge';
import type { OrderStatus } from './types';
import { useOrderStatus } from './useOrderStatus';

interface OrderDetailProps {
  id: string;
  justPlaced: boolean;
}

export function OrderDetail({ id, justPlaced }: OrderDetailProps): React.JSX.Element {
  const { data: order } = useSuspenseQuery(orderDetailQueryOptions(id));
  const liveStatus = useOrderStatus(order.id, order.status);

  const money = (amount: number): string =>
    new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: order.currency,
    }).format(amount);

  const optionalMoney = (amount: number | null): string =>
    amount === null ? 'Not available' : money(amount);

  return (
    <article className="mx-auto grid max-w-6xl gap-7">
      {justPlaced && (
        <section
          aria-labelledby="order-placed-title"
          data-testid="order-placed-confirmation"
          className="grid justify-items-center gap-4 rounded-2xl border border-emerald-200 bg-gradient-to-b from-emerald-50 to-white px-6 py-10 text-center shadow-sm sm:px-10 sm:py-12"
        >
          <div
            aria-hidden="true"
            className="grid h-24 w-24 place-items-center rounded-full bg-emerald-100 text-emerald-700 ring-8 ring-emerald-50"
          >
            <svg
              viewBox="0 0 48 48"
              fill="none"
              className="h-14 w-14"
              stroke="currentColor"
              strokeWidth="4"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <title>Order placed successfully</title>
              <path d="m12 25 8 8 17-18" />
            </svg>
          </div>
          <div className="grid gap-2">
            <p className="text-sm font-bold uppercase tracking-[0.16em] text-emerald-700">
              Order confirmed
            </p>
            <h1
              id="order-placed-title"
              className="text-3xl font-bold tracking-tight text-slate-950"
            >
              Your order has been placed!
            </h1>
            <p className="max-w-xl text-slate-600">
              Thank you for shopping with ShopSphere. We’ll keep you updated as your order moves
              through delivery.
            </p>
            <p className="mt-1 font-semibold text-slate-800">Order {order.number}</p>
          </div>
          <div className="mt-2 flex flex-wrap justify-center gap-3">
            <Button asChild className="bg-emerald-700 text-white hover:bg-emerald-800">
              <Link to="/orders">View all orders</Link>
            </Button>
            <Button asChild variant="outline">
              <Link to="/products">Continue shopping</Link>
            </Button>
          </div>
        </section>
      )}

      <header className="grid gap-2">
        <div className="flex items-center gap-3">
          <h1 data-testid="order-number" className="text-2xl font-semibold text-slate-950">
            Order {order.number}
          </h1>

          <StatusBadge status={liveStatus as OrderStatus} />
        </div>

        <p className="text-sm text-[var(--color-text-muted)]">
          Placed {new Date(order.placedUtc).toLocaleString()}
          {order.trackingNumber && ` · Tracking: ${order.trackingNumber}`}
        </p>
      </header>

      <section className="grid gap-3">
        <h2 className="text-lg font-semibold text-slate-900">Items</h2>

        <ul className="list-none divide-y divide-slate-200 rounded-xl border border-slate-200 bg-white p-0">
          {order.lines.map((line) => (
            <li key={line.productId} className="flex flex-wrap items-center gap-4 p-4 sm:p-5">
              <div className="h-20 w-20 shrink-0 overflow-hidden rounded-lg bg-slate-100">
                {line.imageUrl && (
                  <img src={line.imageUrl} alt="" className="h-full w-full object-cover" />
                )}
              </div>

              <div className="min-w-0 flex-1">
                <p className="font-medium text-slate-900">{line.productName}</p>

                <p className="text-sm text-[var(--color-text-muted)]">
                  {money(line.unitPriceAmount)} × {line.quantity}
                </p>
              </div>

              <strong>{money(line.lineTotalAmount)}</strong>
            </li>
          ))}
        </ul>
      </section>

      <section
        aria-label="Order total"
        className="ml-auto grid w-full max-w-sm gap-3 rounded-xl border border-slate-200 bg-white p-5"
      >
        <div className="flex items-center justify-between gap-8 text-sm">
          <span>Subtotal</span>
          <span>{money(order.subtotalAmount)}</span>
        </div>

        <div className="flex items-center justify-between gap-8 text-sm">
          <span>Shipping</span>
          <span>{optionalMoney(order.shippingAmount)}</span>
        </div>

        <div className="flex items-center justify-between gap-8 text-sm">
          <span>Tax</span>
          <span>{optionalMoney(order.taxAmount)}</span>
        </div>

        <div className="flex items-center justify-between gap-8 border-t border-[var(--color-border)] pt-2 text-lg font-semibold">
          <span>Total</span>
          <span>{money(order.totalAmount)}</span>
        </div>
      </section>

      <section className="grid gap-4 sm:grid-cols-2">
        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <h3 className="mb-2 font-semibold text-slate-900">Shipping address</h3>

          <AddressBlock address={order.shippingAddress} />
        </div>

        <div className="rounded-xl border border-slate-200 bg-white p-5">
          <h3 className="mb-2 font-semibold text-slate-900">Billing address</h3>

          {order.billingAddress ? (
            <AddressBlock address={order.billingAddress} />
          ) : (
            <p className="text-sm text-[var(--color-text-muted)]">Not available</p>
          )}
        </div>
      </section>
    </article>
  );
}

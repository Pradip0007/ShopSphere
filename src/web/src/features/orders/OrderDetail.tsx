import { useSuspenseQuery } from '@tanstack/react-query';
import { AddressBlock } from './AddressBlock';
import { orderDetailQueryOptions } from './queries';
import { StatusBadge } from './StatusBadge';

interface OrderDetailProps {
  id: string;
}

export function OrderDetail({ id }: OrderDetailProps): React.JSX.Element {
  const { data: order } = useSuspenseQuery(orderDetailQueryOptions(id));

  const money = (amount: number): string =>
    new Intl.NumberFormat(undefined, {
      style: 'currency',
      currency: order.currency,
    }).format(amount);

  const optionalMoney = (amount: number | null): string =>
    amount === null ? 'Not available' : money(amount);

  return (
    <article className="grid gap-6">
      <header className="grid gap-2">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-semibold">Order {order.number}</h1>

          <StatusBadge status={order.status} />
        </div>

        <p className="text-sm text-[var(--color-text-muted)]">
          Placed {new Date(order.placedUtc).toLocaleString()}
          {order.trackingNumber && ` · Tracking: ${order.trackingNumber}`}
        </p>
      </header>

      <section>
        <h2 className="mb-3 text-lg font-semibold">Items</h2>

        <ul className="list-none divide-y rounded-lg border border-[var(--color-border)] p-0">
          {order.lines.map((line) => (
            <li key={line.productId} className="flex gap-3 p-3">
              <div className="h-16 w-16 shrink-0 overflow-hidden rounded-md bg-[var(--color-surface-muted)]">
                {line.imageUrl && (
                  <img src={line.imageUrl} alt="" className="h-full w-full object-cover" />
                )}
              </div>

              <div className="flex-1">
                <p className="font-medium">{line.productName}</p>

                <p className="text-sm text-[var(--color-text-muted)]">
                  {money(line.unitPriceAmount)} × {line.quantity}
                </p>
              </div>

              <strong>{money(line.lineTotalAmount)}</strong>
            </li>
          ))}
        </ul>
      </section>

      <section className="ml-auto grid max-w-sm gap-2">
        <div className="flex items-center justify-between gap-8 text-sm">
          {' '}
          <span>Subtotal</span>
          <span>{money(order.subtotalAmount)}</span>
        </div>

        <div className="flex items-center justify-between gap-8 text-sm">
          {' '}
          <span>Shipping</span>
          <span>{optionalMoney(order.shippingAmount)}</span>
        </div>

        <div className="flex items-center justify-between gap-8 text-sm">
          {' '}
          <span>Tax</span>
          <span>{optionalMoney(order.taxAmount)}</span>
        </div>

        <div className="flex items-center justify-between gap-8 border-t border-[var(--color-border)] pt-2 text-lg font-semibold">
          {' '}
          <span>Total</span>
          <span>{money(order.totalAmount)}</span>
        </div>
      </section>

      <section className="grid gap-4" style={{ gridTemplateColumns: '1fr 1fr' }}>
        <div>
          <h3 className="mb-2 font-semibold">Shipping address</h3>

          <AddressBlock address={order.shippingAddress} />
        </div>

        <div>
          <h3 className="mb-2 font-semibold">Billing address</h3>

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

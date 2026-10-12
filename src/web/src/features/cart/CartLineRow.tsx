import type { Product } from '@/features/products/types';
import { Button } from '@/shared/ui';
import { useRemoveCartItem, useUpdateCartItem } from './queries';
import type { CartLine } from './types';

interface CartLineRowProps {
  line: CartLine;
  product: Product | undefined;
  onBuyNow: () => void;
}

export function CartLineRow({ line, product, onBuyNow }: CartLineRowProps): React.JSX.Element {
  const update = useUpdateCartItem();
  const remove = useRemoveCartItem();

  const isUpdating = update.isPending;
  const isRemoving = remove.isPending;
  const unitPrice = product
    ? new Intl.NumberFormat(undefined, {
        style: 'currency',
        currency: product.currency,
      }).format(product.price)
    : null;
  const linePrice = product
    ? new Intl.NumberFormat(undefined, {
        style: 'currency',
        currency: product.currency,
      }).format(product.price * line.quantity)
    : null;

  return (
    <li data-testid="cart-line" className="bg-white">
      <div className="grid gap-4 p-5 sm:grid-cols-[6.75rem_minmax(0,1fr)]">
        <div
          aria-hidden="true"
          className="grid aspect-square place-items-center rounded border border-slate-200 bg-slate-50 text-xs text-slate-500"
        >
          Product image
        </div>
        <div className="grid content-start gap-2">
          <div>
            <h2 className="text-lg font-medium text-slate-900">
              {product?.title ?? 'Product details unavailable'}
            </h2>
            <p className="text-sm text-slate-500">
              Standard pack of 1 · Product details placeholder
            </p>
          </div>
          <p className="text-sm font-medium text-green-700">
            ★★★★☆ <span className="text-slate-600">4.2 (693) · Rating placeholder</span>
          </p>
          <p className="text-sm text-slate-500">Delivery estimate: 3–5 business days</p>
          <div className="flex flex-wrap items-center gap-3">
            <label className="flex items-center gap-2 rounded border border-slate-300 px-3 py-2 text-sm font-medium">
              Qty
              <select
                aria-label="Quantity"
                value={line.quantity}
                disabled={isUpdating || isRemoving}
                onChange={(event) => {
                  update.mutate({
                    productId: line.productId,
                    quantity: Number(event.target.value),
                  });
                }}
                className="bg-transparent"
              >
                {Array.from(
                  { length: Math.max(10, line.quantity + 1) },
                  (_, index) => index + 1,
                ).map((quantity) => (
                  <option key={quantity} value={quantity}>
                    {quantity}
                  </option>
                ))}
              </select>
            </label>
            <p className="text-xl font-semibold text-slate-900">
              {linePrice ?? 'Price unavailable'}
              {unitPrice && line.quantity > 1 && (
                <span className="ml-2 text-sm font-normal text-slate-500">({unitPrice} each)</span>
              )}
            </p>
          </div>
        </div>
      </div>
      <div className="grid divide-y divide-slate-200 border-t border-slate-200 text-sm sm:grid-cols-3 sm:divide-x sm:divide-y-0">
        <Button
          variant="ghost"
          className="rounded-none py-3 text-slate-600"
          disabled
          title="Saved items are not available yet"
        >
          Save for later
        </Button>
        <Button
          variant="ghost"
          size="md"
          data-testid="cart-line-remove"
          className="rounded-none py-3 text-slate-600"
          disabled={isUpdating || isRemoving}
          onClick={() => {
            remove.mutate(line.productId);
          }}
        >
          Remove
        </Button>
        <Button
          variant="ghost"
          className="rounded-none py-3 text-slate-600"
          disabled={isUpdating || isRemoving}
          onClick={onBuyNow}
        >
          Buy this now
        </Button>
      </div>
      {(update.isError || remove.isError) && (
        <p role="alert" className="px-5 pb-3 text-sm text-[var(--color-danger)]">
          {update.isError ? 'Could not update this item.' : 'Could not remove this item.'}
        </p>
      )}
    </li>
  );
}

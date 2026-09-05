import { Button } from '@/shared/ui';
import { useRemoveCartItem, useUpdateCartItem } from './queries';
import type { CartLine } from './types';

interface CartLineRowProps {
  line: CartLine;
}

export function CartLineRow({ line }: CartLineRowProps): React.JSX.Element {
  const update = useUpdateCartItem();
  const remove = useRemoveCartItem();

  const isUpdating = update.isPending;
  const isRemoving = remove.isPending;

  return (
    <li className="grid gap-3 border-b border-[var(--color-border)] py-4">
      <div>
        <p className="text-sm font-medium">Product</p>

        <p className="break-all text-xs text-[var(--color-text-muted)]">{line.productId}</p>
      </div>

      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          aria-label="Decrease quantity"
          disabled={line.quantity <= 1 || isUpdating || isRemoving}
          onClick={() => {
            update.mutate({
              productId: line.productId,
              quantity: line.quantity - 1,
            });
          }}
        >
          −
        </Button>

        <span aria-live="polite" className="min-w-[2ch] text-center">
          {line.quantity}
        </span>

        <Button
          variant="outline"
          size="sm"
          aria-label="Increase quantity"
          disabled={isUpdating || isRemoving}
          onClick={() => {
            update.mutate({
              productId: line.productId,
              quantity: line.quantity + 1,
            });
          }}
        >
          +
        </Button>

        <Button
          variant="ghost"
          size="sm"
          className="ml-auto"
          disabled={isUpdating || isRemoving}
          onClick={() => {
            remove.mutate(line.productId);
          }}
        >
          Remove
        </Button>
      </div>

      {update.isError && (
        <p role="alert" className="text-sm text-[var(--color-danger)]">
          Could not update this item.
        </p>
      )}

      {remove.isError && (
        <p role="alert" className="text-sm text-[var(--color-danger)]">
          Could not remove this item.
        </p>
      )}
    </li>
  );
}

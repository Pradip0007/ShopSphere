import { useQuery } from '@tanstack/react-query';
import {
  Button,
  Drawer,
  DrawerClose,
  DrawerContent,
  DrawerTitle,
  DrawerTrigger,
} from '@/shared/ui';
import { CartLineRow } from './CartLineRow';
import { cartQueryOptions } from './queries';

interface CartDrawerProps {
  trigger: React.ReactNode;
}

export function CartDrawer({ trigger }: CartDrawerProps): React.JSX.Element {
  const query = useQuery(cartQueryOptions());
  const cart = query.data;

  return (
    <Drawer>
      <DrawerTrigger asChild>{trigger}</DrawerTrigger>

      <DrawerContent side="right">
        <header className="flex items-center justify-between border-b border-[var(--color-border)] p-4">
          <DrawerTitle className="text-lg font-semibold">Your cart</DrawerTitle>

          <DrawerClose asChild>
            <Button variant="ghost" size="sm" aria-label="Close cart">
              ×
            </Button>
          </DrawerClose>
        </header>

        <div className="flex-1 overflow-y-auto p-4">
          {query.isPending && <p className="text-[var(--color-text-muted)]">Loading cart…</p>}

          {query.isError && (
            <p role="alert" className="text-[var(--color-danger)]">
              Could not load cart.
            </p>
          )}

          {cart && cart.lines.length === 0 && (
            <p className="text-[var(--color-text-muted)]">Your cart is empty.</p>
          )}

          {cart && cart.lines.length > 0 && (
            <ul className="m-0 list-none p-0">
              {cart.lines.map((line) => (
                <CartLineRow key={line.productId} line={line} />
              ))}
            </ul>
          )}
        </div>

        {cart && cart.lines.length > 0 && (
          <footer className="grid gap-2 border-t border-[var(--color-border)] p-4">
            <div className="flex justify-between font-semibold">
              <span>Total items</span>
              <span>{cart.totalUnits}</span>
            </div>

            <DrawerClose asChild>
              <Button variant="primary" block>
                Continue shopping
              </Button>
            </DrawerClose>
          </footer>
        )}
      </DrawerContent>
    </Drawer>
  );
}

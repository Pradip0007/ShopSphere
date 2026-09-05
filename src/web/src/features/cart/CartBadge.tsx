import { useQuery } from '@tanstack/react-query';
import { cartQueryOptions } from './queries';

export function CartBadge(): React.JSX.Element {
  const query = useQuery(cartQueryOptions());
  const count = query.data?.totalUnits ?? 0;

  return (
    <span className="relative inline-flex items-center gap-1">
      <span>Cart</span>

      {count > 0 && (
        <span className="inline-flex min-w-5 items-center justify-center rounded-full bg-brand-500 px-1 text-xs text-white">
          {count}
        </span>
      )}
    </span>
  );
}

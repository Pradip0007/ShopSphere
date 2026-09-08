import { cva, type VariantProps } from 'class-variance-authority';

import type { OrderStatus } from './types';

const badge = cva(
  'inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold',
  {
    variants: {
      status: {
        Pending: 'bg-[oklch(0.94_0_0)] text-[oklch(0.32_0_0)]',
        Confirmed: 'bg-[oklch(0.90_0.10_145)] text-[oklch(0.35_0.15_145)]',
        Paid: 'bg-[oklch(0.90_0.10_180)] text-[oklch(0.30_0.15_180)]',
        Shipped: 'bg-[oklch(0.92_0.09_250)] text-[oklch(0.32_0.17_250)]',
        Delivered: 'bg-[oklch(0.90_0.13_145)] text-[oklch(0.28_0.16_145)]',
        Cancelled: 'bg-[oklch(0.90_0.10_25)] text-[oklch(0.35_0.18_25)]',
        Refunded: 'bg-[oklch(0.88_0.08_85)] text-[oklch(0.32_0.15_85)]',
      } satisfies Record<OrderStatus, string>,
    },
    defaultVariants: {
      status: 'Pending',
    },
  },
);

interface StatusBadgeProps extends VariantProps<typeof badge> {
  status: OrderStatus;
}

export function StatusBadge({ status }: StatusBadgeProps): React.JSX.Element {
  const dot = statusDotColor(status);

  return (
    <span className={badge({ status })}>
      <span
        aria-hidden
        style={{
          display: 'inline-block',
          width: 6,
          height: 6,
          borderRadius: '50%',
          background: dot,
        }}
      />
      {status}
    </span>
  );
}

function statusDotColor(status: OrderStatus): string {
  switch (status) {
    case 'Pending':
      return 'oklch(0.50 0 0)';
    case 'Confirmed':
      return 'oklch(0.55 0.18 145)';
    case 'Paid':
      return 'oklch(0.55 0.18 180)';
    case 'Shipped':
      return 'oklch(0.55 0.20 250)';
    case 'Delivered':
      return 'oklch(0.45 0.18 145)';
    case 'Cancelled':
      return 'oklch(0.55 0.20 25)';
    case 'Refunded':
      return 'oklch(0.55 0.16 85)';
  }
}

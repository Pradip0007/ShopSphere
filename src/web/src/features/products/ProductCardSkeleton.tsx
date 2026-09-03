const SKELETON_IDS = [
  'skeleton-1',
  'skeleton-2',
  'skeleton-3',
  'skeleton-4',
  'skeleton-5',
  'skeleton-6',
  'skeleton-7',
  'skeleton-8',
  'skeleton-9',
  'skeleton-10',
  'skeleton-11',
  'skeleton-12',
];

export function ProductCardSkeleton(): React.JSX.Element {
  return (
    <div className="grid gap-3 rounded-lg border border-[var(--color-border)] bg-[var(--color-surface)] p-3">
      <div className="aspect-square animate-pulse rounded-md bg-[var(--color-surface-muted)]" />

      <div className="grid gap-2">
        <div className="h-4 w-3/4 animate-pulse rounded bg-[var(--color-surface-muted)]" />

        <div className="h-5 w-1/3 animate-pulse rounded bg-[var(--color-surface-muted)]" />
      </div>
    </div>
  );
}

export function ProductGridSkeleton({ count = 8 }: { count?: number }): React.JSX.Element {
  return (
    <div
      className="grid gap-4"
      style={{
        gridTemplateColumns: 'repeat(auto-fill, minmax(220px, 1fr))',
      }}
    >
      {SKELETON_IDS.slice(0, count).map((id) => (
        <ProductCardSkeleton key={id} />
      ))}
    </div>
  );
}

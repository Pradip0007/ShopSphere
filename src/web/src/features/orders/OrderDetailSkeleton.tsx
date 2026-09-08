const SKELETON_ROWS = ['one', 'two', 'three'] as const;

export function OrderDetailSkeleton(): React.JSX.Element {
  return (
    <div className="grid gap-6">
      <div className="h-8 w-1/3 animate-pulse rounded bg-[var(--color-surface-muted)]" />

      <div className="h-4 w-1/2 animate-pulse rounded bg-[var(--color-surface-muted)]" />

      <div className="rounded-lg border border-[var(--color-border)]">
        {SKELETON_ROWS.map((row) => (
          <div
            key={row}
            className="flex gap-3 border-b border-[var(--color-border)] p-3 last:border-b-0"
          >
            <div className="h-16 w-16 shrink-0 animate-pulse rounded-md bg-[var(--color-surface-muted)]" />

            <div className="grid flex-1 gap-2">
              <div className="h-4 w-3/4 animate-pulse rounded bg-[var(--color-surface-muted)]" />

              <div className="h-4 w-1/3 animate-pulse rounded bg-[var(--color-surface-muted)]" />
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

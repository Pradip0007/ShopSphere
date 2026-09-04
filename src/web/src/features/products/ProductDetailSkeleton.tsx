export function ProductDetailSkeleton(): React.JSX.Element {
  const thumbnailSlots = ['first', 'second', 'third', 'fourth'];

  return (
    <section className="grid gap-8 md:grid-cols-2">
      <div className="grid gap-3">
        <div className="aspect-square animate-pulse rounded-lg bg-[var(--color-surface-muted)]" />
        <div className="grid grid-cols-4 gap-2">
          {thumbnailSlots.map((slot) => (
            <div
              key={slot}
              className="aspect-square animate-pulse rounded-md bg-[var(--color-surface-muted)]"
            />
          ))}
        </div>
      </div>
      <div className="grid content-start gap-4">
        <div className="h-8 w-3/4 animate-pulse rounded bg-[var(--color-surface-muted)]" />
        <div className="h-6 w-1/3 animate-pulse rounded bg-[var(--color-surface-muted)]" />
        <div className="h-24 animate-pulse rounded bg-[var(--color-surface-muted)]" />
        <div className="h-12 w-40 animate-pulse rounded bg-[var(--color-surface-muted)]" />
      </div>
    </section>
  );
}
